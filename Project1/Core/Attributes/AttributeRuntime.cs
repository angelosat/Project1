using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using Project1.Framework.UI.Primitives;
using System;
using System.Collections.Generic;

namespace Project1.Core.Attributes
{
    public sealed class AttributeRuntime : Inspectable, ISaveableNewNew<AttributeRuntime>, ISerializableNew<AttributeRuntime>, IListable, IDefWrapper<AttributeDef>
    {
        public class ValueModifier
        {
            readonly float _Value;
            readonly Func<float> ValueGetter;
            public ValueModifier(float value)
            {
                this._Value = value;
                this.ValueGetter = () => this._Value;
            }
            public ValueModifier(Func<float> valueGetter)
            {
                this.ValueGetter = valueGetter;
            }
            public float GetValue()
            {
                return this.ValueGetter();
            }
        }

        public float Tick = Ticks.PerSecond / 0.5f; //1 tick per 2 seconds
        public float Timer = 0;
        public float RegenerationRate = 1;
        public ProgressFloat Rec = new(0, Ticks.PerSecond, Ticks.PerSecond);
        public float DecayRate = -0.5f;
        public float GainRate = 0;
        public List<ValueModifier> Modifiers = [];
        public ProgressLeveledExp Progress;
        public AttributeDef AttributeDef;
        public AttributeDef Def => this.AttributeDef;

        public int Level { get => this.Progress.Level; set => this.Progress.Level = value; }//.SetLevel(value); }
        public int Min = 10;
        const int BaseXpToLevel = 100;//5; //placeholder
        public AttributeRuntime(AttributeDef def, int value = 10)
        {
            this.AttributeDef = def;
            this.Progress = new ProgressLeveledExp(BaseXpToLevel, value);
        }
        public AttributeRuntime()
        {
            this.Progress = new ProgressLeveledExp(BaseXpToLevel, 10);
        }
        public void Update(Entity parent)
        {
            this.AttributeDef.Worker.Tick(parent, this);
        }

        public override string ToString()
        {
            return this.AttributeDef.Name + ": " + this.Level;
        }
        public void Award(Entity parent, float p)
        {
            this.AttributeDef.Worker.Award(parent, this, p);
            parent.Map.Events.Post(new AttributeAdjustedEvent(parent as Actor, this.Def, this.Progress.Value));
        }
        internal void AddToProgress(float p)
        {
            this.Progress.Value += p;
            if (p > 0)
                this.Rec.Value = this.Rec.Max;
        }

        internal Control GetProgressControl()
        {
            return this.Progress.GetControl();
        }

        public AttributeRuntime Clone()
        {
            return new AttributeRuntime(this.AttributeDef, this.Level);
        }
        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.SaveDef("Def", this.AttributeDef);
            tag.Add(this.Progress.Save("Progress"));
            return tag;
        }

        public static AttributeRuntime Create(SaveTag tag)
        {
            var att = new AttributeRuntime();
            att.AttributeDef = tag["Def"].LoadDef<AttributeDef>();
            tag.TryGetTag("Progress", att.Progress.Load);
            return att;
        }
        public void Write(IDataWriter w)
        {
            w.Write(this.Def);
            this.Progress.Write(w);
        }

        public AttributeRuntime Read(IDataReader r)
        {
            this.AttributeDef = r.ReadDef<AttributeDef>();
            this.Progress.Read(r);
            return this;
        }

        public Control GetListControlGui()
        {
            return new Bar(this.Progress, 200, () => $"{this.AttributeDef.LabelReadable}: {this.Level}")
            {
                TooltipFunc = t => t.AddControls(this.Progress.GetControl())
            };
        }

        public static AttributeRuntime Create(IDataReader r) => new AttributeRuntime().Read(r);

        internal void SetValue(float value)
        {
            this.Progress.Value = value;
        }

        [EnsureStaticCtorCall]
        internal class Packets
        {
            static int _packetTypeIdAdjust;
            static Packets()
            {
                _packetTypeIdAdjust = Registry.PacketHandlers.Register(HandleAdjust);
            }
            internal static void SendAdjust(Actor actor, AttributeDef def, float v)
            {
                var server = actor.Net as Server;
                server.BeginPacket(_packetTypeIdAdjust)
                    .Write(actor.RefId)
                    .Write(def)
                    .Write(v);
            }
            private static void HandleAdjust(NetEndpoint endpoint, Packet packet)
            {
                var client = endpoint as Client;
                var r = packet.PacketReader;
                var actor = client.World.Get<Actor>(r.ReadInt32());
                var def = r.ReadDef<AttributeDef>();
                var delta = r.ReadSingle();
                actor.Attributes.ApplyDelta(def, delta);
            }
        }
    }
}
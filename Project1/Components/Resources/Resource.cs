using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Start_a_Town_
{
    public sealed class Resource : MetricWrapper, IProgressBar, ISaveable, ISerializableNew<Resource>, INamed
    {
        public ResourceDef ResourceDef;
        public List<ResourceRateModifier> Modifiers = new();
        public int TicksPerRecoverOne, TicksPerDrainOne;
        int TickRecover, TickDrain;
        float _max;
        
        public float Max
        {
            get => this._max; set
            {
                var oldmax = this._max;
                this._max = value;
                this.Value += (value - oldmax);
            }
        }
        float _value;
        public float Value
        {
            get => this._value;
            set => this._value = Math.Max(0, Math.Min(value, this.Max));
        }
        public ResourceThreshold CurrentThreshold => this.ResourceDef.Worker.GetCurrentThreshold(this);
        public Progress Rec = ResourceDef.Recovery;
        public float Percentage { get => this.Value / this.Max; set => this.Value = this.Max * value; }
        public float Min => 0;

        public string Name => this.ResourceDef.Name;
        Resource() { }
        public Resource(ResourceDef def)
        {
            this.ResourceDef = def;
            this.Max = def.BaseMax;
            this.Value = this.Max;
        }
        public override void Tick()
        {
            this.ResourceDef.Worker.Tick(this);
        }
        public void Tick(GameObject parent)
        {
            this.ResourceDef.Worker.Tick(this);
            //this.Value += this.ModValuePerTick;
            if (this.TicksPerRecoverOne > 0)
            {
                if (this.TickRecover-- <= 0)
                {
                    this.TickRecover = this.TicksPerRecoverOne;
                    this.Value++;
                }
            }
            if (this.TicksPerDrainOne > 0)
            {
                if (this.TickDrain-- <= 0)
                {
                    this.TickDrain = this.TicksPerDrainOne;
                    this.Value--;
                }
            }
        }

        internal void HandleRemoteCall(GameObject parent, ObjectEventArgs e)
        {
            this.ResourceDef.Worker.HandleRemoteCall(parent, e, this);
        }
        //public void SyncAdjust(Entity parent, float value)
        //{
        //    Packets.SendSyncAdjust(parent, this.ResourceDef, value);
        //}
        public void Adjust(float delta)
        {
            this.ResourceDef.Worker.Modify(this, delta);
        }
        public Resource Initialize(float max, float initPercentage)
        {
            this.Value = this.Max =  max * initPercentage;
            return this;
        }
        internal Resource Clone()
        {
            return new Resource(this.ResourceDef) { Max = this.Max, Value = this.Value, Rec = new Progress(0, this.Rec.Max, this.Rec.Value) };// this.Rec.Clone() };
        }

        internal void HandleMessage(GameObject parent, ObjectEventArgs e)
        {
            this.ResourceDef.Worker.HandleMessage(this, parent, e);
        }

        internal void OnNameplateCreated(GameObject parent, Nameplate plate)
        {
            this.ResourceDef.Worker.OnHealthBarCreated(parent, plate, this);
        }

        internal void OnHealthBarCreated(GameObject parent, Nameplate plate)
        {
            this.ResourceDef.Worker.OnHealthBarCreated(parent, plate, this);
        }

        internal Control GetControl()
        {
            return this.ResourceDef.Worker.GetControl(this);
        }

        public override string ToString()
        {
            return $"{this.ResourceDef.Name}: {this.Value.ToString(this.ResourceDef.Format)} / {this.Max.ToString(this.ResourceDef.Format)}";
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, this.ResourceDef.Name);
            tag.Add(this.Value.Save("Value"));
            tag.Add(this.Max.Save("Max"));
            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            tag.TryGetTagValueOrDefault("Value", out this._value);
            tag.TryGetTagValueOrDefault("Max", out this._max);
            return this;
        }

        public void Write(IDataWriter w)
        {
            w.Write(this.ResourceDef);
            w.Write(this._value);
            w.Write(this.Max);
        }

        public Resource Read(IDataReader r)
        {
            this.ResourceDef = r.ReadDef<ResourceDef>();
            this._value = r.ReadSingle();
            this.Max = r.ReadSingle();
            return this;
        }

        internal void AddModifier(ResourceRateModifier resourceModifier)
        {
            if (this.Modifiers.Any(m => m.Def == resourceModifier.Def))
                throw new Exception();
            this.Modifiers.Add(resourceModifier);
        }

        public float GetThresholdValue(int index)
        {
            return this.ResourceDef.Worker.GetThresholdValue(this, index);
        }
        static Resource()
        {
            Packets.Init();
        }
        internal void InitMaterials(Entity obj, Dictionary<string, MaterialDef> materials)
        {
            this.ResourceDef.Worker.InitMaterials(obj, materials);
        }
        Action _unsub = () => { };
       
        internal void OnDespawn(Entity parent)
        {
            this._unsub();
        }
        internal void Resolve(Entity parent)
        {
            foreach (var i in this.ResourceDef.Worker.GetInterests())
                _unsub += parent.Map?.Events.ListenTo(i.eventType, i.handler);
        }

        public static Resource Create(IDataReader r) => new Resource().Read(r);

        [EnsureStaticCtorCall]
        internal class Packets
        {
            static int /*PacketSyncAdjust, */_packetTypeIdAdjust;
            internal static void Init()
            {
                //PacketSyncAdjust = Registry.PacketHandlers.Register(HandleSyncAdjust);
                _packetTypeIdAdjust = Registry.PacketHandlers.Register(HandleAdjust);
            }
            internal static void SendAdjust(Actor actor, ResourceDef def, float v)
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
                var actor = client.World.GetEntity<Actor>(r.ReadInt32());
                var resDef = r.ReadDef<ResourceDef>();
                var delta = r.ReadSingle();
                //actor.Resources[resDef].Adjust(delta);
                actor.Resources.Adjust(resDef, delta);
            }
            //internal static void SendSyncAdjust(Entity actor, ResourceDef def, float value)
            //{
            //    var net = actor.Net;
            //    if (net is Server)
            //        actor.GetResource(def).Adjust(value);
            //    //net.GetOutgoingStreamOrderedReliable().Write(PacketSyncAdjust, actor.RefId, def.Name, value);
            //    var pck = actor.Net.BeginPacketNew(ReliabilityType.OrderedReliable, PacketSyncAdjust);
            //    pck.Write(actor.RefId);
            //    pck.Write(def.Name);
            //    pck.Write(value);
            //}
            //private static void HandleSyncAdjust(NetEndpoint net, Packet pck)
            //{
            //    var r = pck.PacketReader;
            //    var actor = net.World.GetEntity(r.ReadInt32()) as Actor;
            //    var resource = Def.GetDef<ResourceDef>(r.ReadString());
            //    var value = r.ReadSingle();
            //    if (net is Server)
            //        SendSyncAdjust(actor, resource, value);
            //    else
            //        actor.GetResource(resource).Adjust(value);
            //}
        }
    }
}

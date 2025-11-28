using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    public class Resource : Inspectable, IProgressBar, ISaveable, ISerializable, INamed
    {
        public readonly ResourceDef ResourceDef;
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

        public Resource(ResourceDef def)
        {
            this.ResourceDef = def;
            this.Max = def.BaseMax;
            this.Value = this.Max;
        }

        public void Tick(GameObject parent)
        {
            this.ResourceDef.Worker.Tick(parent, this);
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

        internal virtual void HandleRemoteCall(GameObject parent, ObjectEventArgs e)
        {
            this.ResourceDef.Worker.HandleRemoteCall(parent, e, this);
        }
        public void SyncAdjust(Entity parent, float value)
        {
            Packets.SendSyncAdjust(parent, this.ResourceDef, value);
        }
        public void Adjust(float add)
        {
            this.ResourceDef.Worker.Add(add, this);
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
            tag.TryGetTagValue("Value", out this._value);
            tag.TryGetTagValue("Max", out this._max);
            return this;
        }

        public void Write(BinaryWriter w)
        {
            w.Write(this._value);
            w.Write(this.Max);
        }

        public ISerializable Read(BinaryReader r)
        {
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

        class Packets
        {
            static int PacketSyncAdjust;
            public static void Init()
            {
                PacketSyncAdjust = Network.RegisterPacketHandler(HandleSyncAdjust);
            }
            public static void SendSyncAdjust(Entity actor, ResourceDef def, float value)
            {
                var net = actor.Net;
                if (net is Server)
                    actor.GetResource(def).Adjust(value);
                net.GetOutgoingStream().Write(PacketSyncAdjust, actor.RefID, def.Name, value);
            }
            private static void HandleSyncAdjust(INetPeer net, BinaryReader r)
            {
                var actor = net.GetNetworkObject(r.ReadInt32()) as Actor;
                var resource = Def.GetDef<ResourceDef>(r.ReadString());
                var value = r.ReadSingle();
                if (net is Server)
                    SendSyncAdjust(actor, resource, value);
                else
                    actor.GetResource(resource).Adjust(value);
            }
        }
    }
}

using Microsoft.Xna.Framework;
using Project1.Core.Blocks.Comps;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Serialization;
using System;

namespace Project1.Core.Blocks
{
    class BlockLightComp : BlockComp
    {
        //public new class Spec(byte intensity, IPowerSource powerSource, int consumption, Func<bool> isSwitchedOn = null) : BlockComp.Spec
        public new class Spec() : BlockComp.Spec
        {
            //readonly IPowerSource PowerSource = powerSource;
            //readonly byte Intensity = intensity;
            //readonly int Consumption = consumption;
            //readonly Func<bool> IsSwitchedOn = isSwitchedOn;

            public override Type CompType => typeof(BlockLightComp);

            //public override BlockLightComp CreateComp() => new(this.Intensity, this.PowerSource, this.Consumption, this.IsSwitchedOn);
            public override BlockLightComp CreateComp() => new();
        }
        public override BlockCompDef CompDef => throw new NotImplementedException();
        readonly IPowerSource PowerSource;
        readonly byte Intensity = 15;
        readonly int Consumption;
        readonly static int ConsumptionRate = Ticks.PerSecond;
        readonly Func<bool> IsSwitchedOn;

        public bool Powered;
        int ConsumptionTick = 0;
        public BlockLightComp()
        {
            
        }
        public BlockLightComp(byte intensity, IPowerSource powerSource, int consumption, Func<bool> isSwitchedOn = null)
        {
            this.Intensity = intensity;
            this.PowerSource = powerSource;
            this.Consumption = consumption;
            this.IsSwitchedOn = isSwitchedOn ?? (() => true);
        }
      

        public override void Tick()
        {
            return;
            var map = this.Parent.Map;
            var global = this.Parent.OriginGlobal;
            var isOn = this.IsSwitchedOn();
            if (isOn)
            {
                if (this.Powered)
                {
                    this.ConsumptionTick++;
                    if (this.ConsumptionTick >= ConsumptionRate)
                    {
                        this.ConsumptionTick = 0;
                        this.PowerSource.ConsumePower(map, this.Consumption);
                        if (!this.PowerSource.HasAvailablePower(this.Consumption))
                            this.TurnOff(map, global);
                    }
                }
                else
                {
                    if (this.PowerSource.HasAvailablePower(this.Consumption))
                        this.TurnOn(map, global);
                }
            }
            else
            {
                if(this.Powered)
                    this.TurnOff(map, global);
            }
        }
        internal override void OnSpawned(BlockEntity entity, MapBase map)
        {
            this.Map.SetBlockLuminance(this.Parent.OriginGlobal, this.Intensity);
        }
        internal override void OnDespawned(BlockEntity parent, MapBase map)
        {
            map.SetBlockLuminance(this.Parent.OriginGlobal, 0);
        }

        void TurnOn(MapBase map, Vector3 global)
        {
            this.Powered = true;
            map.SetBlockLuminance(global, this.Intensity);
        }
        void TurnOff(MapBase map, Vector3 global)
        {
            this.Powered = false;
            map.SetBlockLuminance(global, (byte)0);
        }

        protected override void SaveExtra(SaveTag tag)
        {
            tag.Add(this.ConsumptionTick.Save("Tick"));
            tag.Add(this.Powered.Save("Powered"));
        }
        public override void Load(SaveTag tag)
        {
            tag.TryGetTagValueOrDefault<int>("Tick", out this.ConsumptionTick);
            tag.TryGetTagValueOrDefault<bool>("Powered", out this.Powered);

        }
        public override void Write(IDataWriter w)
        {
            w.Write(this.Powered);
            w.Write(this.ConsumptionTick);
        }
        public override ISerializable Read(IDataReader r)
        {
            this.Powered = r.ReadBoolean();
            this.ConsumptionTick = r.ReadInt32();
            return this;
        }
    }
}

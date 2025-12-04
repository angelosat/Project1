using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class BlockEntityLuminance : BlockEntityComp
    {
        public override string Name { get; } = "Lightsource";
        readonly IPowerSource PowerSource;
        readonly byte Intensity;
        readonly int Consumption;
        readonly static int ConsumptionRate = Ticks.PerSecond;
        readonly Func<bool> IsSwitchedOn;

        public bool Powered;
        int ConsumptionTick = 0;

        public BlockEntityLuminance(byte intensity, IPowerSource powerSource, int consumption, Func<bool> isSwitchedOn = null)
        {
            this.Intensity = intensity;
            this.PowerSource = powerSource;
            this.Consumption = consumption;
            this.IsSwitchedOn = isSwitchedOn ?? (() => true);
        }
        public override void Tick()
        {
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

        public override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.ConsumptionTick.Save("Tick"));
            tag.Add(this.Powered.Save("Powered"));
        }
        public override void Load(SaveTag tag)
        {
            tag.TryGetTagValue<int>("Tick", out this.ConsumptionTick);
            tag.TryGetTagValue<bool>("Powered", out this.Powered);

        }
        public override void Write(BinaryWriter w)
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

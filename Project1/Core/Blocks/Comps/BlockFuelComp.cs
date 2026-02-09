using Project1.Core.Entities;
using Project1.Core.Helpers;
using System;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using Project1.Framework;
using Project1.Core.Crafting;

namespace Project1.Core.Blocks
{
    internal class BlockFuelComp : BlockEntityComp
    {
        internal new class Spec : BlockEntityComp.Spec
        {
            public override Type CompType => typeof(BlockFuelComp);

            public override BlockEntityComp CreateComp() => new BlockFuelComp();
        }
        public override string Name => "Fuel";

        int FuelCurrent;
        public readonly ProgressInt Fuel = new(max: 100);
        public int FuelAvailable => this.Fuel.Value;
        internal bool TryConsumeFuel(int fuel)
        {
            if (fuel > this.FuelAvailable)
                return false;
            this.Fuel.ApplyDelta(-fuel);
            this.Parent.Map.Events.Post(new BlockEntityCompUpdatedEvent(this));
            return true;
        }
        internal override bool TryConsume(Entity item)
        {
            var fuel = CraftingSystem.GetFuelValue(item);
            if (fuel == 0)
                throw new ArgumentException($"{item} is not fuel");
            var deficit = this.Fuel.Missing;
            var totake = deficit / fuel;
            if (totake == 0)
                throw new InvalidOperationException($"{nameof(totake)} was 0");
            this.Fuel.ApplyDelta(fuel * totake);
            item.Consume(totake);
            this.Parent.Map.Events.Post(new BlockEntityCompUpdatedEvent(this));
            return true;
        }
        internal override void GetSelectionInfo(Control container)
        {
            container.AddControls(new BarFinal(this.Fuel, () => "Fuel"));
        }
        public override void Write(IDataWriter w)
        {
            w.Write(this.Fuel.Value);
        }
        public override ISerializable Read(IDataReader r)
        {
            this.Fuel.SetValue(r.ReadInt32());
            return this;
        }
        protected override void SaveExtra(SaveTag tag)
        {
            tag.Save("FuelCurrent", this.FuelCurrent);
        }
        public override void Load(SaveTag tag)
        {
            this.FuelCurrent = tag.LoadInt("FuelCurrent");
        }
    }
}
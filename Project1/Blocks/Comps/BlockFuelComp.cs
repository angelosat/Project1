using Start_a_Town_.UI;
using System;

namespace Start_a_Town_
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
        ProgressInt Fuel = new(max: 100);

        internal override bool TryConsume(Entity item)
        {
            if (!CraftingSystem.IsFuel(item))
                throw new ArgumentException($"{item} is not fuel");

            var fuel = CraftingSystem.GetFuelValue(item);
            this.FuelCurrent += fuel;
            this.Parent.Map.Events.Post(new BlockEntityCompUpdatedEvent(this));
            //this.Parent.Map.Events.Post(new BlockEntityUpdatedEvent(this.Parent));

            return false;
        }
        internal override void GetSelectionInfo(Control container)
        {
            container.AddControls(new Label($"Fuel: {this.FuelCurrent}"));
            container.AddControls(new BarFinal(this.Fuel));
        }
        public override void Write(IDataWriter w)
        {
            w.Write(this.FuelCurrent);
        }
        public override ISerializable Read(IDataReader r)
        {
            this.FuelCurrent = r.ReadInt32();
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

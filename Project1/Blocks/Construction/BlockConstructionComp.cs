using Start_a_Town_.UI;
using System;
namespace Start_a_Town_
{
    internal class BlockConstructionComp : BlockEntityComp
    {
        internal new class Spec : BlockEntityComp.Spec
        {
            public override Type CompType => typeof(BlockConstructionComp);

            public override BlockEntityComp CreateComp()
            {
                return new BlockConstructionComp();
            }
        }
        public override string Name => $"{this}";

        public Block Block;

        internal override void GetSelectionInfo(Control container)
        {
            container.AddControls(new Label(this.Block));
        }
    }
}

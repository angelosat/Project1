using System;

namespace Start_a_Town_
{
    internal class BlockDoorComp : BlockEntityComp
    {
        internal new class Spec : BlockEntityComp.Spec
        {
            public override Type CompType => typeof(BlockDoorComp);

            public override BlockEntityComp CreateComp() => new BlockDoorComp();
        }
        public override string Name => "Door";

    }
}

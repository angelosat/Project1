using Start_a_Town_.Blocks;

namespace Start_a_Town_
{
    class BlockFluidEntity : BlockEntity
    {
        int FlowTimer;

        public BlockFluidEntity(IntVec3 originGlobal) : base(originGlobal)
        {
        }

        public override void Tick(MapBase m, IntVec3 global)
        {
            var map = this.Map;
            var current = global;
            var cell = m.GetCell(global);
            var mat = cell.Material;
            var visc = mat.Viscosity;
            if (this.FlowTimer++ <= visc)
                return;
            /// flow downwards
            var below = current.Below;
            var belowBlock = this.Map.GetBlock(below);
            if (belowBlock == BlockDefOf.Air)
            {
                Block.Place(BlockDefOf.Fluid, map, below, mat, 1, 0, 0);
                this.Map.SetBlock(below, BlockDefOf.Fluid, mat, 1);
            }
            else
            {
                foreach (var n in global.GetAdjacentHorLazy())
                {
                    var nblock = this.Map.GetBlock(n);
                    if (nblock != BlockDefOf.Air)
                        continue;
                    Block.Place(BlockDefOf.Fluid, map, n, mat, 0, 0, 0);
                }
            }
            /// remove entity after updating once, all flowing must complete in first update
            this.Map.RemoveBlockEntity(global);
        }
    }
}

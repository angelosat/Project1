using Project1.Core.Blocks;
using Project1.Core.Blocks.Comps;
using Project1.Core.Entities;
using Project1.Core.Input;
using Project1.Core.Plants;
using Project1.Core.UI;

namespace Project1.Core.Towns.Designations
{
    public abstract class DesignationWorker
    {
        internal abstract bool IsValid(ISelectable target);
    }
    public abstract class CellDesignationWorker : DesignationWorker
    {
        internal override bool IsValid(ISelectable target) => target is CellSelection cell && this.IsValid(cell);
        public abstract bool IsValid(CellSelection cell);
    }
    public abstract class EntityDesignationWorker : DesignationWorker
    {
        internal override bool IsValid(ISelectable target) => target is Entity entity && this.IsValid(entity);
        public abstract bool IsValid(Entity entity);
    }
    public abstract class BlockEntityDesignationWorker : DesignationWorker
    {
        internal override bool IsValid(ISelectable target) => target is BlockEntity bEntity && this.IsValid(bEntity);
        public abstract bool IsValid(BlockEntity entity);
    }
    class DesignationWorkerDeconstruct : DesignationWorker
    {
        internal override bool IsValid(ISelectable target)
        {
            return target switch
            {
                CellSelection cell => cell.Block.IsDeconstructible,
                BlockEntity blockEntity => blockEntity.Def.Block.IsDeconstructible,
                Entity => false,
                _ => false// throw new UnreachableException()
            };
        }
    }
    class DesignationWorkerConstruct : CellDesignationWorker
    {
        public override bool IsValid(CellSelection cell)
            => cell.Block is BlockAir;
    }
    //class DesignationWorkerConstruct : BlockEntityDesignationWorker
    //{
    //    public override bool IsValid(BlockEntity be)
    //    {
    //        return be.HasComp<BlockConstructionComp>();
    //    }
    //}
    class DesignationWorkerMine : CellDesignationWorker
    {
        public override bool IsValid(CellSelection cell)
            => cell.Block.IsMinable;
    }
    class DesignationWorkerSwitch : DesignationWorker
    {
        internal override bool IsValid(ISelectable target)
            => target switch
            {
                BlockEntity be => be.GetCompOrDefault<BlockSwitchableComp>()?.IsSwitchable() ?? false,
                CellSelection cell => cell.BlockEntity?.GetCompOrDefault<BlockSwitchableComp>()?.IsSwitchable() ?? false,
                _ => false
            };
    }
    class DesignationWorkerSwitchOff : DesignationWorker
    {
        internal override bool IsValid(ISelectable target)
            => target switch
            {
                BlockEntity be => be.GetCompOrDefault<BlockSwitchableComp>()?.IsOn ?? false,
                CellSelection cell => cell.BlockEntity?.GetCompOrDefault<BlockSwitchableComp>()?.IsOn ?? false,
                _ => false
            };
    }
    class DesignationWorkerChop : EntityDesignationWorker
    {
        public override bool IsValid(Entity entity)
            => entity.GetComponent<PlantComponent>()?.Species.ChoppingProduct != null;
    }
    class DesignationWorkerHarvest : EntityDesignationWorker
    {
        public override bool IsValid(Entity entity)
            => entity.GetComponent<PlantComponent>()?.IsHarvestable ?? false;
    }
}

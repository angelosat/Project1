using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Input;
using Project1.Core.Plants;
using Project1.Core.UI;
using System.Diagnostics;

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
    class DesignationWorkerDeconstruct : DesignationWorker
    {
        internal override bool IsValid(ISelectable target)
        {
            return target switch
            {
                CellSelection cell => cell.Block.IsDeconstructible,
                BlockEntity blockEntity => blockEntity.Def.Block.IsDeconstructible,
                _ => throw new UnreachableException()
            };
        }
    }
    class DesignationWorkerConstruct : CellDesignationWorker
    {
        public override bool IsValid(CellSelection cell)
        {
            return cell.Block is BlockAir;
        }
    }
    class DesignationWorkerMine : CellDesignationWorker
    {
        public override bool IsValid(CellSelection cell)
        {
            return cell.Block.IsMinable;
        }
    }
    class DesignationWorkerSwitch : CellDesignationWorker
    {
        public override bool IsValid(CellSelection cell)
        {
            return cell.BlockEntity.HasComp<BlockEntityCompSwitchable>();
        }
    }

    class DesignationWorkerChop : EntityDesignationWorker
    {
        public override bool IsValid(Entity entity)
        {
            return entity.GetComponent<PlantComponent>()?.Species.ChoppingProduct != null;
        }
    }
    class DesignationWorkerHarvest : EntityDesignationWorker
    {
        public override bool IsValid(Entity entity)
        {
            return entity.GetComponent<PlantComponent>()?.IsHarvestable ?? false;
        }
    }
}

using Start_a_Town_.Components;

namespace Start_a_Town_
{
    abstract class DesignationWorker
    {
        public abstract bool IsValid(TargetArgs target);
    }
    
    class DesignationWorkerDeconstruct : DesignationWorker
    {
        public override bool IsValid(TargetArgs target)
        {
            return target.Block?.IsDeconstructible ?? false;
        }
    }
    class DesignationWorkerMine : DesignationWorker
    {
        public override bool IsValid(TargetArgs target)
        {
            return target.Block?.IsMinable ?? false;
        }
    }
    class DesignationWorkerSwitch : DesignationWorker
    {
        public override bool IsValid(TargetArgs target)
        {
            return target.BlockEntity?.HasComp<BlockEntityCompSwitchable>() ?? false;
        }
    }

    class DesignationWorkerChop : DesignationWorker
    {
        public override bool IsValid(TargetArgs target)
        {
            return target.Object?.GetComponent<PlantComponent>()?.PlantProperties.ProductCutDown != null;
        }
    }
    class DesignationWorkerHarvest : DesignationWorker
    {
        public override bool IsValid(TargetArgs target)
        {
            return target.GetEntity<Plant>()?.IsHarvestable ?? false;
        }
    }
}

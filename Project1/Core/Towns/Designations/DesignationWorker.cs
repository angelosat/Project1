using Project1.Core.Plants;
using Project1.Core.Base;
using Project1.Core.Components.Plants;
using Project1.Core;

namespace Project1.Core.Towns.Designations
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
    class DesignationWorkerConstruct : DesignationWorker
    {
        public override bool IsValid(TargetArgs target)
        {
            return target.Block is BlockAir;
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
            return target.BlockEntityOld?.HasComp<BlockEntityCompSwitchable>() ?? false;
        }
    }

    class DesignationWorkerChop : DesignationWorker
    {
        public override bool IsValid(TargetArgs target)
        {
            return target.Object?.GetComponent<PlantComponent>()?.Species.ProductCutDown != null;
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

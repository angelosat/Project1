namespace Start_a_Town_
{
    abstract class DesignationWorker
    {
        public abstract bool IsValid(MapBase map, IntVec3 global);
    }

    class DesignationWorkerDeconstruct : DesignationWorker
    {
        public override bool IsValid(MapBase map, IntVec3 global)
        {
            return map.IsDeconstructible(global);
        }
    }
    class DesignationWorkerMine : DesignationWorker
    {
        public override bool IsValid(MapBase map, IntVec3 global)
        {
            return map.GetBlock(global).IsMinable;
        }
    }
    class DesignationWorkerSwitch : DesignationWorker
    {
        public override bool IsValid(MapBase map, IntVec3 global)
        {
            return map.GetBlockEntity(global)?.HasComp<BlockEntityCompSwitchable>() ?? false;
        }
    }
    class DesignationWorkerRemove : DesignationWorker
    {
        public override bool IsValid(MapBase map, IntVec3 global)
        {
            return true;
        }
    }
}

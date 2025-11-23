namespace Start_a_Town_
{
    class ZoneStockpileWorker : ZoneWorker
    {
        public override bool IsValidLocation(MapBase map, IntVec3 global)
        {
            if (!map.IsSolid(global))
                return false;
            if (map.IsSolid(global.Above))
                return false;
            return true;
        }
    }
}

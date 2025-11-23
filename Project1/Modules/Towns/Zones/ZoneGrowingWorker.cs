namespace Start_a_Town_
{
    class ZoneGrowingWorker : ZoneWorker
    {
        public override bool IsValidLocation(MapBase map, IntVec3 global)
        {
            if (!map.IsSolid(global))
                return false;
            if (map.IsSolid(global.Above))
                return false;
            var cell = map.GetCell(global);
            if (cell.Material != MaterialDefOf.Soil)
                return false;
            return true;
        }
    }
}

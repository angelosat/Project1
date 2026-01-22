namespace Start_a_Town_
{
    class StatMaxHaulWeight : StatWorker
    {
        public override float CalculateStat(GameObject obj)
        {
            return obj[AttributeDefOf.Strength]?.Level ?? 0;
        }
    }
}

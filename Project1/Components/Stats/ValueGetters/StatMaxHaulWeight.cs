namespace Start_a_Town_
{
    class StatMaxHaulWeight : StatValueGetter
    {
        public StatMaxHaulWeight(StatDef parent) : base(parent)
        {
        }

        public override float GetValue(GameObject obj)
        {
            return obj[AttributeDefOf.Strength]?.Level ?? 0;
        }
    }
}

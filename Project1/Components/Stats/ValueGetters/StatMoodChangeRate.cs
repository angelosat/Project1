namespace Start_a_Town_
{
    class StatMoodChangeRate : StatValueGetter
    {
        public StatMoodChangeRate(StatDef parent) : base(parent)
        {
        }

        public override float GetValue(GameObject obj)
        {
            var actor = obj as Actor;
            var resilience = actor.GetTrait(TraitDefOf.Resilience).Normalized;
            var value = 1 + resilience * .5f;
            return value;
        }
    }
}

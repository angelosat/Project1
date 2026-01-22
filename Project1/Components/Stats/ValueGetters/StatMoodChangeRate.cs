namespace Start_a_Town_
{
    class StatMoodChangeRate : StatWorker
    {
        public override float CalculateStat(GameObject obj)
        {
            var actor = obj as Actor;
            var resilience = actor.GetTrait(TraitDefOf.Resilience).Normalized;
            var value = 1 + resilience * .5f;
            return value;
        }
    }
}

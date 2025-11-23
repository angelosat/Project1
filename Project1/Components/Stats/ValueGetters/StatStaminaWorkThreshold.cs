namespace Start_a_Town_
{
    class StatStaminaWorkThreshold : StatValueGetter
    {
        public StatStaminaWorkThreshold(StatDef parent) : base(parent)
        {
        }
        public override float GetValue(GameObject obj)
        {
            var actor = obj as Actor;
            var staminaBaseThreshold = .25f; //placeholder?
            var stamina = actor.GetResource(ResourceDefOf.Stamina);
            staminaBaseThreshold = stamina.GetThresholdValue(0);
            var activity1 = actor.GetTrait(TraitDefOf.Activity).Normalized;
            var num = activity1 * staminaBaseThreshold * .5f;
            var threshold = staminaBaseThreshold - num;
            return threshold;
        }
    }
}

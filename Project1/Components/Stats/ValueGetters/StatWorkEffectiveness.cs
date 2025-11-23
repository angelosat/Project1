namespace Start_a_Town_
{
    class StatWorkEffectiveness : StatValueGetter
    {
        public StatWorkEffectiveness(StatDef parent) : base(parent)
        {
        }

        public override float GetValue(GameObject obj)
        {
            var actor = obj as Actor;
            var val = actor.GetEquipmentSlot(GearType.Mainhand)?.GetStat(StatDefOf.ToolEffectiveness) ?? actor.GetMaterial(BoneDefOf.RightHand).Density;
            return val;
        }
    }
}

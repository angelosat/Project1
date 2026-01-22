namespace Start_a_Town_
{
    class StatWorkEffectiveness : StatWorker
    {
        public override float CalculateStat(GameObject obj)
        {
            var actor = obj as Actor;
            var val = actor.GetEquipmentSlot(GearType.Mainhand)?.GetStat(StatDefOf.ToolEffectiveness) ?? actor.GetMaterial(BoneDefOf.RightHand).Density;
            return val;
        }
    }
}

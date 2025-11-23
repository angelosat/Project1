namespace Start_a_Town_
{
    class StatWorkSpeed : StatValueGetter
    {
        public StatWorkSpeed(StatDef parent) : base(parent)
        {
        }

        public override float GetValue(GameObject obj)
        {
            var actor = obj as Actor;
            var toolspeed = actor.GetEquipmentSlot(GearType.Mainhand)?.GetStat(StatDefOf.ToolSpeed) ?? 0;
            var speed = 1 + toolspeed;

            var stamina = obj[ResourceDefOf.Stamina];
            speed *= stamina.CurrentThreshold.Value;

            return speed;
        }
    }
}

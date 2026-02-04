using Project1.Core.Gear;
using Project1.Framework.Entities;
using Project1.Framework.Entities.Actors;
using Project1.Framework.Resources;
using Project1.Framework.Stats;

namespace Project1.Framework.Stats.ValueGetters
{
    class StatWorkSpeed : StatWorker
    {
        public override float CalculateStat(GameObject obj)
        {
            var actor = obj as Actor;
            //var toolspeed = actor.GetEquipmentSlot(GearTypeDefOf.Mainhand)?.GetStat(StatDefOf.ToolSpeed) ?? 0;
            var toolspeed = actor.Gear.GetGear(GearTypeDefOf.Mainhand)?.GetStat(StatDefOf.ToolSpeed) ?? 0;
            var speed = 1 + toolspeed;

            var stamina = obj[ResourceDefOf.Stamina];
            speed *= stamina.CurrentThreshold.Value;

            return speed;
        }
    }
}

using Project1.Core.Entities.Actors;
using Project1.Core.Gear;
using Project1.Core.Resources;
using Project1.Core.Stats;

namespace Project1.Core.Entities.Stats.ValueGetters
{
    sealed class StatWorkSpeed : StatWorker
    {
        public override float CalculateStat(Entity obj)
        {
            var actor = obj as Actor;
            var toolspeed = actor.Gear.GetGear(GearTypeDefOf.Mainhand)?.GetStat(StatDefOf.ToolSpeed) ?? 0;
            var speed = 1 + toolspeed;

            var stamina = obj.Resources.View(ResourceDefOf.Stamina);
            speed *= stamina.CurrentThreshold.Value;

            return speed;
        }
    }
}

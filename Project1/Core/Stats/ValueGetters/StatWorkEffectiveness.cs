using Project1.Core.Animations;
using Project1.Core.Entities;
using Project1.Core.Entities.Stats;
using Project1.Core.Gear;
using Project1.Core.Stats;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Entities.Stats.ValueGetters
{
    class StatWorkEffectiveness : StatWorker
    {
        public override float CalculateStat(GameObject obj)
        {
            var actor = obj as Actor;
            var val = actor.GetEquipmentSlot(GearTypeDefOf.Mainhand)?.GetStat(StatDefOf.ToolEffectiveness) ?? actor.GetMaterial(BoneDefOf.RightHand).Density;
            return val;
        }
    }
}

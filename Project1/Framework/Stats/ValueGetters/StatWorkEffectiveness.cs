using Project1.Core.Gear;
using Project1.Framework.Animations;
using Project1.Framework.Stats;
using Start_a_Town_;

namespace Project1.Framework.Stats.ValueGetters
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

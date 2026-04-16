using Project1.Core.Animations;
using Project1.Core.Entities.Actors;
using Project1.Core.Gear;
using Project1.Core.Stats;

namespace Project1.Core.Entities.Stats.Resolvers;

sealed class StatWorkEffectiveness : StatResolver
{
    public override float CalculateStat(Entity obj)
    {
        var actor = obj as Actor;
        var val = actor.GetEquipmentSlot(GearTypeDefOf.Mainhand)?.GetStat(StatDefOf.ToolEffectiveness) ?? actor.GetMaterial(BoneDefOf.RightHand).Density;
        return val;
    }
}

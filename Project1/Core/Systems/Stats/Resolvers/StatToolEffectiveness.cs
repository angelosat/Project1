using Project1.Core.Animations;
using Project1.Core.Systems.Quality;

namespace Project1.Core.Entities.Stats.Resolvers;

sealed class StatToolEffectiveness : StatResolver
{
    public override float CalculateStat(Entity obj)
    {
        var tool = obj;
        var material = tool.GetMaterial(BoneDefOf.ToolHead);
        if (material is null)
            return 1; // is it ever possible for this to be null?
        return material.Density * obj.QualityComp.Tier.Multiplier;
    }
}

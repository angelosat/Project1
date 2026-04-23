using Project1.Core.Animations;
using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Quality;


namespace Project1.Core.Entities.Stats.Resolvers;

sealed class StatToolSwingSpeed : StatResolver
{
    public override float CalculateStat(Entity obj)
    {
        var tool = obj as Entity;
        var material = tool?.GetMaterial(BoneDefOf.ToolHandle);
        if (material is null)
            return 1;
        //var aa = 20f; // what is this?
        //var density = Math.Max(aa, material.Density); // in case for some reason the material is air
        //                                              //var total = density / 100f; // density should add ticks between each tool hit (NOT POSSIBLE THE WAY I HAVE ANIMATIONS SET UP)
        //var total = aa / density;

        var baseline = (float)MaterialDefOf.LightWood.Density;
        var total = material.Density / baseline;
        total /= obj.QualityComp.Tier.Multiplier;
        return total;
    }
}

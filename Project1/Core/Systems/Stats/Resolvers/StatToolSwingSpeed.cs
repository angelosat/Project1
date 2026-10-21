using Project1.Core.Animations;
using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Quality;
using Project1.Core.Systems.Tools;

namespace Project1.Core.Entities.Stats.Resolvers;

sealed class StatToolSwingSpeed : StatResolver
{
    public override float CalculateStat(Entity obj)
    {
        var tool = obj;
        var material = tool?.GetMaterial(BoneDefOf.ToolHandle);
        if (material is null)
            return 1;
        //var aa = 20f; // what is this?
        //var density = Math.Max(aa, material.Density); // in case for some reason the material is air
        //                                              //var total = density / 100f; // density should add ticks between each tool hit (NOT POSSIBLE THE WAY I HAVE ANIMATIONS SET UP)
        //var total = aa / density;

        var baseline = (float)MaterialDefOf.LightWood.Density;
        var baseSpeed = (obj.Profile as GearProfileDef).BaseSpeed.Value;
        var matModifier = 1 + .2f * material.Density / baseline;
        var total = baseSpeed * matModifier;
        total /= obj.QualityComp.Tier.Multiplier;
        return total;
    }
}

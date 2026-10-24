using Project1.Core.AI.Personality;
using Project1.Core.Animations;
using Project1.Core.Attributes;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Skills;
using Project1.Core.Systems.Quality;
using Project1.Core.Systems.Tools;
using System.Diagnostics;
using System.Linq;

namespace Project1.Core.Entities.Stats.Resolvers;

sealed class StatResolver_ToolEffectiveness : StatResolver
{
    public StatResolver_ToolEffectiveness()
    {
        
    }
    public override float CalculateItemStat(Entity owner)
    {
        var material = owner.GetMaterial(BoneDefOf.ToolHead);
        if (material is null)
            return 1; // is it ever possible for this to be null?
        return material.Density * owner.QualityComp.Tier.Multiplier;
    }

    internal override string Format(StatDef def, Entity item)
    {
        //var comp = item.GetComponent<ToolComp>();
        var profile = item.Profile as GearProfileDef;
        return $"{profile.ToolUse.LabelReadable} effectiveness";
    }
}

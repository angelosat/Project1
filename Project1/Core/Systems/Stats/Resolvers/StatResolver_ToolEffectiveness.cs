using Project1.Core.Animations;
using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Quality;
using Project1.Core.Systems.Tools;

namespace Project1.Core.Entities.Stats.Resolvers;

sealed class StatResolver_ToolEffectiveness : StatResolver
{
    public override float CalculateStat(Entity owner)
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

sealed class StatResolver_ActorWorkEffectiveness(ToolUseDef toolUse) : StatResolver
{
    readonly ToolUseDef ToolUse = toolUse;
    public override float CalculateStat(Entity owner)
    {
        var actor = (Actor)owner;
        var skill = actor.Skills[this.ToolUse.Skill];
        return skill.Level;
    }
    public override float CalculateStatActor(StatDef def, Actor actor)
    {
        var skill = actor.Skills[this.ToolUse.Skill];
        return skill.Level * .01f;
    }
    internal override string Format(StatDef def, Entity item)
    {
        //var comp = item.GetComponent<ToolComp>();
        //var profile = item.Profile as GearProfileDef;
        return $"{this.ToolUse.LabelReadable} effectiveness";
    }
}

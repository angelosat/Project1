using Project1.Core.AI.Personality;
using Project1.Core.Animations;
using Project1.Core.Attributes;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Skills;
using Project1.Core.Systems.Quality;
using Project1.Core.Systems.Tools;
using System.Linq;

namespace Project1.Core.Entities.Stats.Resolvers;

sealed class StatResolver_ToolEffectiveness : StatResolver
{
    public StatResolver_ToolEffectiveness()
    {
        
    }
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

sealed class StatResolver_DamageEffectiveness : StatResolver
{
    public StatResolver_DamageEffectiveness()
    {

    }
    public override float CalculateStat(Entity owner)
    {
        var material = owner.GetMaterial(BoneDefOf.ToolHead);
        if (material is null)
            return 1; // is it ever possible for this to be null?
        var role = (owner.Profile as GearProfileDef).Role;
        return material.Density * owner.QualityComp.Tier.Multiplier;
    }

    internal override string Format(StatDef def, Entity item)
    {
        //var comp = item.GetComponent<ToolComp>();
        var profile = item.Profile as GearProfileDef;
        return $"{profile.ToolUse.LabelReadable} effectiveness";
    }
}


sealed class StatResolver_ActorWorkEffectiveness : StatResolver
{
    public StatResolver_ActorWorkEffectiveness(ToolUseDef toolUse)
    {
        ToolUse = toolUse;
    }

    public StatResolver_ActorWorkEffectiveness()
    {

    }
    readonly ToolUseDef ToolUse;
    public override float CalculateStat(Entity owner)
    {
        var actor = (Actor)owner;
        var skill = actor.Skills[this.ToolUse.Skill];
        return skill.Level * .01f;
    }
    public override float CalculateStatActor(StatDef def, Actor actor)
    {
        //var skill = actor.Skills[this.ToolUse.Skill];
        var skill = actor.Skills[(def.TargetDef as ToolUseDef).Skill];
        return skill.Level * .01f;
    }
    
    internal override string Format(StatDef def, Entity item)
    {
        //var comp = item.GetComponent<ToolComp>();
        //var profile = item.Profile as GearProfileDef;
        return $"{this.ToolUse.LabelReadable} effectiveness";
    }
}

sealed class StatResolver_ActorTraitBonus : StatResolver
{
    public StatResolver_ActorTraitBonus(TraitDef target)
    {
        Target = target;
    }

    public StatResolver_ActorTraitBonus()
    {
        
    }
    readonly TraitDef Target;
    public override float CalculateStat(Entity owner)
    {
        return 100;
    }
    public override float CalculateStatActor(StatDef def, Actor actor)
    {
        return 100;
    }
    internal override bool IsApplicable(StatDef statDef, ActorDnaDef actorDef)
    {
        var target = (TraitDef)statDef.TargetDef;
        return actorDef.Traits.Contains(target);
    }
    internal override string Format(StatDef def, Entity item)
    {
        return $"{this.Target.LabelReadable} effectiveness";
    }
}
sealed class StatResolver_ActorAttributeBonus : StatResolver
{
    public StatResolver_ActorAttributeBonus()
    {

    }
    readonly ResourceDef Target;
    public override float CalculateStat(Entity owner)
    {
        return 100;
    }
    public override float CalculateStatActor(StatDef def, Actor actor)
    {
        return 100;
    }
    internal override bool IsApplicable(StatDef statDef, ActorDnaDef actorDef)
    {
        var target = (AttributeDef)statDef.TargetDef;
        return actorDef.Attributes.Contains(target);
    }
    internal override string Format(StatDef def, Entity item)
    {
        return $"{def.TargetDef.LabelReadable} effectiveness";
    }
}
sealed class StatResolver_ActorResourceBonus : StatResolver
{
    public StatResolver_ActorResourceBonus(ResourceDef target)
    {
        Target = target;
    }

    public StatResolver_ActorResourceBonus()
    {
        
    }
    readonly ResourceDef Target;
    public override float CalculateStat(Entity owner)
    {
        return 100;
    }
    public override float CalculateStatActor(StatDef def, Actor actor)
    {
        return 100;
    }
    internal override bool IsApplicable(StatDef statDef, ActorDnaDef actorDef)
    {
        var target = (ResourceDef)statDef.TargetDef;
        return actorDef.Resources.Contains(target);
    }
    internal override string Format(StatDef def, Entity item)
    {
        return $"{this.Target.LabelReadable} effectiveness";
    }
}

sealed class StatResolver_ActorSkillBonus : StatResolver
{
    public StatResolver_ActorSkillBonus(SkillDef target)
    {
        Target = target;
    }

    public StatResolver_ActorSkillBonus()
    {

    }
    readonly SkillDef Target;
    public override float CalculateStat(Entity owner)
    {
        return 0;
    }
    public override float CalculateStatActor(StatDef def, Actor actor)
    {
        return 0;
    }
    internal override bool IsApplicable(StatDef statDef, ActorDnaDef actorDef)
    {
        var target = (SkillDef)statDef.TargetDef;
        return actorDef.Skills.Contains(target);
    }
    internal override string Format(StatDef def, Entity item)
    {
        return $"{this.Target.LabelReadable} skill";
    }
}

sealed class StatResolver_ActorResistanceBonus : StatResolver
{
    readonly DamageDef Target;

    public StatResolver_ActorResistanceBonus(DamageDef target)
    {
        Target = target;
    }

    public StatResolver_ActorResistanceBonus()
    {

    }
    public override float CalculateStat(Entity owner)
    {
        return 0;
    }
    public override float CalculateStatActor(StatDef def, Actor actor)
    {
        return 0;
    }
    
    internal override string Format(StatDef def, Entity item)
    {
        return $"{this.Target.LabelReadable} resistance";
    }
}
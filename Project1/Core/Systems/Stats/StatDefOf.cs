using Project1.Core.AI;
using Project1.Core.AI.Personality;
using Project1.Core.Attributes;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Entities.Stats.Resolvers;
using Project1.Core.Gear;
using Project1.Core.Interactions;
using Project1.Core.Resources;
using Project1.Core.Skills;
using Project1.Core.Systems.Inventory;
using Project1.Core.Systems.Stats;
using Project1.Core.Systems.Stats.Resolvers;
using Project1.Core.Systems.Tools;
using System.Collections.Generic;

namespace Project1.Core.Stats;

public static class StatDefOf
{
    public static readonly StatDef MaxHaulWeight = new("Max Haul Weight", typeof(StatWorkEffectiveness), typeof(InventoryComp))
    {
        Description = "Exceeding the limit will drain stamina",
    };

    public static readonly StatDef Encumberance = new("Encumberance", typeof(StatEncumberance), typeof(InventoryComp))
    {
        Description = "Being encumbered affects walking speed.",
    };

    public static readonly StatDef MoveSpeed = new("Move Speed", typeof(StatResolver_MoveSpeed), typeof(MobileComp))
    {
        Description = "Speed of walking",
        Type = StatDef.Types.Percentile,
        //BaseValue = 1f,
        BaseValue = 100f,
    };

    public static readonly StatDef SwingSpeed = new("Swing Speed", typeof(StatResolver))
    {
        Description = "Speed of equipped main hand swing",
        Type = StatDef.Types.Percentile,
        BaseValue = 1f//1f,
    };

    public static readonly StatDef CriticalChance = new ("Critical Chance", typeof(StatResolver))
    {
        Description = "Critical chance",
        Type = StatDef.Types.Percentile,
        //BaseValue = .05f,
        BaseValue = 5f,
    };

    public static readonly StatDef HitChance = new("Hit Chance", typeof(StatResolver_HitChance))
    {
        Description = "Hit chance",
        Type = StatDef.Types.Percentile,
        //BaseValue = .5f,
        BaseValue = 50f,
    };

    public static readonly StatDef DodgeChance = new("Dodge Chance", typeof(StatResolver))
    {
        Description = "Hit chance",
        Type = StatDef.Types.Percentile,
        //BaseValue = .05f,
        BaseValue = 5f,
    };

    public static readonly StatDef ParryChance = new("Parry Chance", typeof(StatResolver))
    {
        Description = "Hit chance",
        Type = StatDef.Types.Percentile,
        BaseValue = .05f//5f,
    };

    public static readonly StatDef StaminaThresholdForWork = new("Stamina Threshold For Work", typeof(StatStaminaWorkThreshold), typeof(ResourcesComp))
    {
        Description = "Won't start new tasks if stamina below this percentage.",
    };

    public static readonly StatDef MoodChangeRate = new("Mood Change Rate", typeof(StatMoodChangeRate), typeof(AIComp))
    {
        Description = "The speed at which mood changes to reach target value.",
    };

    public static readonly StatDef MaxPatience = new("MaxPatience Max", typeof(StatResolver_Patience), typeof(AIComp))
    {
        Description = "Capacity for patience.",
    };

    public static readonly StatDef Armor = new("Armor", typeof(StatResolver_Armor), typeof(GearComp))
    {
        Description = "Protection against damage.",
        Type = StatDef.Types.Scalar,
        BaseValue = 10f
    };

    public static readonly StatDef Damage = new("Damage", typeof(StatResolver_Damage), typeof(ToolComp))
    {
        Description = "Amount of damage.",
        Type = StatDef.Types.Scalar,
        StringFormat = "0",
        Format = "#"
    };

    public static readonly StatDef ItemPower = new("Power", typeof(StatResolver_ToolEffectiveness), typeof(ToolComp))
    {
        Description = "Amount of work produced with each hit of the tool.",
        Type = StatDef.Types.Scalar,
        StringFormat = "0",
        Format = "#"
    };
    
    public static readonly StatDef ItemSpeed = new("Speed", typeof(StatToolSwingSpeed), typeof(ToolComp))
    {
        Description = "Determines time between each tool strike.",
        Type = StatDef.Types.Scalar,
        StringFormat = "0.##",
        Format = "0.#0"
    };

    public static readonly StatDef WorkSpeed = new("Work Speed", typeof(StatWorkSpeed), typeof(WorkComponent))
    {
        Description = "Work speed",
        Type = StatDef.Types.Scalar,
        StringFormat = "+##0%",
        Format = "#.#0"
    };
    public static readonly StatDef WorkEffectiveness = new("Work Effectiveness", typeof(StatWorkEffectiveness), typeof(ToolComp))
    {
        Description = "Work effectiveness",
        Type = StatDef.Types.Scalar,
        StringFormat = "+##0"
    };

    static StatDefOf()
    {
        Def.Register(typeof(StatDefOf));

        List<Def> toRegister = [];
        foreach (var target in Def.Get<ToolUseDef>())
            toRegister.Add(new StatDef($"{target.LabelReadable} Work", target, typeof(StatResolver_ActorWorkEffectiveness), typeof(ToolComp)) { Type = StatDef.Types.Percentile });
        foreach (var target in Def.Get<ToolUseUnarmedDef>())
                toRegister.Add(new StatDef($"Unarmed {target.LabelReadable}", target, typeof(StatResolver_ActorUnarmedWorkAmount), typeof(ToolComp)) 
            {
                EligibleBonusTargets = [GearRoleDefOf.Feet, GearRoleDefOf.Torso, GearRoleDefOf.Head],  
                Type = StatDef.Types.Scalar ,
                BaseValue = 1f,
            });
        foreach (var target in Def.Get<AttributeDef>())
            toRegister.Add(new StatDef($"{target.LabelReadable}", target, typeof(StatResolver_ActorAttributeBonus), typeof(ToolComp)) { Type = StatDef.Types.Scalar });
        foreach (var target in Def.Get<TraitDef>())
            toRegister.Add(new StatDef($"{target.LabelReadable} trait", target, typeof(StatResolver_ActorTraitBonus), typeof(ToolComp)) { Type = StatDef.Types.Scalar });
        foreach (var target in Def.Get<ResourceDef>())
            toRegister.Add(new StatDef($"{target.LabelReadable}", target, typeof(StatResolver_ActorResourceBonus), typeof(ToolComp)) { Type = StatDef.Types.Scalar });
        foreach (var target in Def.Get<SkillDef>())
            toRegister.Add(new StatDef($"{target.LabelReadable} skill", target, typeof(StatResolver_ActorSkillBonus), typeof(ToolComp)) { Type = StatDef.Types.Scalar });
        foreach (var target in Def.Get<DamageResistanceDef>())
            toRegister.Add(new StatDef($"{target.LabelReadable} resistance", target, typeof(StatResolver_ActorResistanceBonus), typeof(ToolComp))
            { 
                EligibleBonusTargets = [GearRoleDefOf.Feet, GearRoleDefOf.Torso, GearRoleDefOf.Head],
                Type = StatDef.Types.Percentile 
            });
        foreach (var to in toRegister)
            Def.Register(to);
    }
}

sealed class StatResolver_HitChance : StatResolver
{
    public override float CalculateActorStat(StatDef def, Actor actor)
    {
        var meleeSkill = actor.Skills.GetLevel(SkillDefOf.Melee);
        var dexterity = actor.Attributes.GetLevel(AttributeDefOf.Dexterity);
        return (meleeSkill / 5f) + dexterity;
    }
}

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
using Project1.Core.Systems.Tools;
using System.Collections.Generic;

namespace Project1.Core.Stats;

//[EnsureStaticCtorCall]
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

    public static readonly StatDef WalkSpeed = new("Walk Speed", typeof(StatWalkSpeed), typeof(MobileComponent))
    {
        Description = "Speed of walking",
        Type = StatDef.Types.Percentile,
        BaseValue = 1f,
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

    public static readonly StatDef Armor = new("Armor", typeof(StatArmor), typeof(GearComp))
    {
        Description = "Protection against damage.",
        Type = StatDef.Types.Scalar
    };

    public static readonly StatDef ToolEffectiveness = new("Tool Effectiveness", typeof(StatResolver_ToolEffectiveness), typeof(ToolComp))
    {
        Description = "Amount of work produced with each hit of the tool.",
        Type = StatDef.Types.Scalar,
        StringFormat = "0"
    };

    public static readonly StatDef ToolSpeed = new("Tool Speed", typeof(StatToolSwingSpeed), typeof(ToolComp))
    {
        Description = "Determines time between each tool strike.",
        Type = StatDef.Types.Scalar,
        StringFormat = "0.##"
        //StringFormat = "+##0%"
    };

    public static readonly StatDef WorkSpeed = new("Work Speed", typeof(StatWorkSpeed), typeof(WorkComponent))
    {
        Description = "Work speed",
        Type = StatDef.Types.Scalar,
        StringFormat = "+##0%"
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

        // Procedurally generate statdefs based on dynamic systems like skills, damage types, work types, etc
        List<Def> toRegister = [];
        foreach (var target in Def.Get<ToolUseDef>())
            toRegister.Add(new StatDef($"{target.LabelReadable} effectiveness", target, typeof(StatResolver_ActorWorkEffectiveness), typeof(ToolComp)) { Type = StatDef.Types.Percentile });
        foreach (var target in Def.Get<AttributeDef>())
            toRegister.Add(new StatDef($"{target.LabelReadable}", target, typeof(StatResolver_ActorAttributeBonus), typeof(ToolComp)) { Type = StatDef.Types.Scalar });
        foreach (var target in Def.Get<TraitDef>())
            toRegister.Add(new StatDef($"{target.LabelReadable} trait", target, typeof(StatResolver_ActorTraitBonus), typeof(ToolComp)) { Type = StatDef.Types.Scalar });
        foreach (var target in Def.Get<ResourceDef>())
            toRegister.Add(new StatDef($"{target.LabelReadable}", target, typeof(StatResolver_ActorResourceBonus), typeof(ToolComp)) { Type = StatDef.Types.Scalar });
        foreach (var target in Def.Get<SkillDef>())
            toRegister.Add(new StatDef($"{target.LabelReadable} skill", target, typeof(StatResolver_ActorSkillBonus), typeof(ToolComp)) { Type = StatDef.Types.Scalar });
        foreach (var to in toRegister)
            Def.Register(to);
    }
}

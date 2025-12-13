namespace Start_a_Town_
{
    public static class StatDefOf
    {
        public static readonly StatDef MaxHaulWeight = new("Max Haul Weight", typeof(StatWorkEffectiveness))
        {
            Description = "Exceeding the limit will drain stamina",
        };
        public static readonly StatDef Encumberance = new("Encumberance", typeof(StatEncumberance))
        {
            Description = "Being encumbered affects walking speed.",
        };

        public static readonly StatDef WalkSpeed = new("Walk Speed")
        {
            Description = "Speed of walking",
            Type = StatDef.Types.Percentile
        };

        public static readonly StatDef StaminaThresholdForWork = new("Stamina Threshold For Work", typeof(StatStaminaWorkThreshold))
        {
            Description = "Won't start new tasks if stamina below this percentage.",
        };

        public static readonly StatDef MoodChangeRate = new("Mood Change Rate", typeof(StatMoodChangeRate))
        {
            Description = "The speed at which mood changes to reach target value.",
        };

        public static readonly StatDef Armor = new("Armor", typeof(StatArmor))
        {
            Description = "Protection against damage.",
            Type = StatDef.Types.Scalar
        };

        public static readonly StatDef ToolEffectiveness = new("Tool Effectiveness", typeof(StatToolEffectiveness))
        {
            Description = "Amount of work produced with each hit of the tool.",
            Type = StatDef.Types.Scalar
        };

        public static readonly StatDef ToolSpeed = new("Tool Speed", typeof(StatToolSpeed))
        {
            Description = "Determines time between each tool strike.",
            Type = StatDef.Types.Scalar,
            StringFormat = "+##0%"
        };

        public static readonly StatDef WorkSpeed = new("Work Speed", typeof(StatWorkSpeed))
        {
            Description = "Work speed",
            Type = StatDef.Types.Scalar,
            StringFormat = "+##0%"
        };
        public static readonly StatDef WorkEffectiveness = new("Work Effectiveness", typeof(StatWorkEffectiveness))
        {
            Description = "Work effectiveness",
            Type = StatDef.Types.Scalar,
            StringFormat = "+##0"
        };

        static StatDefOf()
        {
            Def.Register(typeof(StatDefOf));
        }

        
    }

    //public static class StatDefOf
    //{
    //    public static readonly StatDef MaxHaulWeight = new("Max Haul Weight", a => a.GetAttribute(AttributeDef.Strength)?.Level ?? 0)
    //    {
    //        Description = "Exceeding the limit will drain stamina",
    //    };
    //    public static readonly StatDef Encumberance = new(
    //        "Encumberance",
    //        a =>
    //        {
    //            var haulWeight = a.Hauled?.TotalWeight ?? 0;
    //            if (haulWeight == 0)
    //                return 0;
    //            var maxWeight = StatDefOf.MaxHaulWeight.GetValue(a);
    //            var ratio = haulWeight / maxWeight;
    //            ratio = MathHelper.Clamp(ratio, 0, 1);
    //            return ratio;
    //        })
    //    {
    //        Description = "Being encumbered affects walking speed.",
    //    };

    //    public static readonly StatDef WalkSpeed = new("Walk Speed")
    //    {
    //        Description = "Speed of walking",
    //        Type = StatDef.Types.Percentile
    //    };

    //    public static readonly StatDef StaminaThresholdForWork = new("Stamina Threshold For Work",
    //        a =>
    //        {
    //            var actor = a as Actor;
    //            var staminaBaseThreshold = .25f; //placeholder?
    //            var stamina = actor.GetResource(ResourceDefOf.Stamina);
    //            staminaBaseThreshold = stamina.GetThresholdValue(0);
    //            var activity1 = actor.GetTrait(TraitDefOf.Activity).Normalized;
    //            var num = activity1 * staminaBaseThreshold * .5f;
    //            var threshold = staminaBaseThreshold - num;
    //            return threshold;
    //        })
    //    {
    //        Description = "Won't start new tasks if stamina below this percentage.",
    //    };

    //    public static readonly StatDef MoodChangeRate = new("Mood Change Rate",
    //        a =>
    //        {
    //            var actor = a as Actor;
    //            var resilience = actor.GetTrait(TraitDefOf.Resilience).Normalized;
    //            var value = 1 + resilience * .5f;
    //            return value;
    //        })
    //    {
    //        Description = "The speed at which mood changes to reach target value.",
    //    };

    //    public static readonly StatDef Armor = new("Armor",
    //          a =>
    //          {
    //              var actor = a as Actor;
    //              var gear = actor.GetGear();
    //              var value = gear.Sum(g => g.Def.ApparelProperties.ArmorValue);
    //              return value;
    //          })
    //    {
    //        Description = "Protection against damage.",
    //        Type = StatDef.Types.Scalar
    //    };


    //    public static readonly StatDef ToolEffectiveness = new("Tool Efficiecy",
    //          a =>
    //          {
    //              var tool = a as Entity;
    //              var material = tool.GetMaterial(BoneDefOf.ToolHead);
    //              if (material is null)
    //                  return 1; // is it ever possible for this to be null?
    //              return material.Density * a.Quality.Multiplier;
    //              //var matStrength = material.Density;
    //              //var efficiency = //tool.Def.ToolProperties?.Ability.Effectiveness ?? 
    //              //                  tool.GetComponent<ToolAbilityComponent>().Props.ToolUse.Effectiveness;
    //              //var total = (float)efficiency * matStrength;
    //              //total *= a.Quality.Multiplier;
    //              //return total;
    //          })
    //    {
    //        Description = "Amount of work produced with each hit of the tool.",
    //        Type = StatDef.Types.Scalar
    //    };
    //    public static readonly StatDef ToolSpeed = new("Tool Speed",
    //          a =>
    //          {
    //              var tool = a as Entity;
    //              var material = tool?.GetMaterial(BoneDefOf.ToolHandle);
    //              if (material is null)
    //                  return 1;
    //              var aa = 20f; // what is this?
    //              var density = Math.Max(aa, material.Density); // in case for some reason the material is air
    //              //var total = density / 100f; // density should add ticks between each tool hit (NOT POSSIBLE THE WAY I HAVE ANIMATIONS SET UP)
    //              var total = aa / density;
    //              total *= a.Quality.Multiplier;
    //              return total;
    //          })
    //    {
    //        Description = "Determines time between each tool strike.",
    //        Type = StatDef.Types.Scalar,
    //        StringFormat = "+##0%"
    //    };
    //    public static readonly StatDef WorkSpeed = new("Work Speed",
    //          a =>
    //          {
    //              var actor = a as Actor;
    //              var toolspeed = actor.GetEquipmentSlot(GearType.Mainhand)?.GetStat(StatDefOf.ToolSpeed) ?? 0;
    //              var speed = 1 + toolspeed;

    //              var stamina = a.Resources[ResourceDefOf.Stamina];
    //              speed *= stamina.CurrentThreshold.Value;

    //              return speed;
    //          })
    //    {
    //        Description = "Work speed",
    //        Type = StatDef.Types.Scalar,
    //        StringFormat = "+##0%"
    //    };
    //    public static readonly StatDef WorkEffectiveness = new("Work Effectiveness",
    //          a =>
    //          {
    //              var actor = a as Actor;
    //              var val = actor.GetEquipmentSlot(GearType.Mainhand)?.GetStat(StatDefOf.ToolEffectiveness) ?? actor.GetMaterial(BoneDefOf.RightHand).Density;
    //              return val;
    //              //return 1 + val;
    //          })
    //    {
    //        Description = "Work effectiveness",
    //        Type = StatDef.Types.Scalar,
    //        StringFormat = "+##0"
    //    };
    //    public static readonly StatDef[] ToolStatPackage = { ToolEffectiveness, ToolSpeed };
    //    public static readonly StatDef[] NpcStatPackage = { MaxHaulWeight, Encumberance, WalkSpeed, Armor };
    //}
}

using Project1.Framework;
using Project1.Core.Skills;

namespace Project1.Core.Systems.Tools
{
    [EnsureStaticCtorCall]
    static class ToolUseDefOf
    {
        public static readonly ToolUseDef Digging = new("Digging", "Dig up soil and dirt blocks.", SkillDefOf.Digging);
        public static readonly ToolUseDef Building = new("Building", "Used for crafting and building.", SkillDefOf.Construction);
        public static readonly ToolUseDef Mining = new("Mining", "Dig up stone blocks.", SkillDefOf.Mining);
        public static readonly ToolUseDef Chopping = new("Chopping", "Chop down trees and enemies with axes.", SkillDefOf.Plantcutting);
        public static readonly ToolUseDef Argiculture = new("Argiculture", "Helps determine type and growth time of plants.", SkillDefOf.Argiculture);
        public static readonly ToolUseDef Planting = new("Planting", "Planting plants.", SkillDefOf.Argiculture);
        public static readonly ToolUseDef Carpentry = new("Carpentry", "The craft of converting wood to useful equipment.", SkillDefOf.Carpentry);
        public static readonly ToolUseDef ToolMaking = new("ToolMaking", "Crafting Tools.", SkillDefOf.Tinkering);

        static ToolUseDefOf()
        {
            Def.Register(typeof(ToolUseDefOf));
        }
    }
}

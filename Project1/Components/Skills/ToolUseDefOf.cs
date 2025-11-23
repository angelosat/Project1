using System.Linq;

namespace Start_a_Town_
{
    static class ToolUseDefOf
    {
        public static readonly ToolUseDef Digging = new("Digging", "Dig up soil and dirt blocks.");
        public static readonly ToolUseDef Building = new("Building", "Used for crafting and building.");
        public static readonly ToolUseDef Mining = new("Mining", "Dig up stone blocks.");
        public static readonly ToolUseDef Chopping = new("Chopping", "Chop down trees and enemies with axes.");
        public static readonly ToolUseDef Argiculture = new("Argiculture", "Helps determine type and growth time of plants.");
        public static readonly ToolUseDef Planting = new("Planting", "Planting plants.");
        public static readonly ToolUseDef Carpentry = new("Carpentry", "The craft of converting wood to useful equipment.");
        //public static readonly ToolUseDef Crafting = new("Crafting", "Crafting tools and equipment");

        static ToolUseDefOf()
        {
            Def.Register(typeof(ToolUseDefOf));
        }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class ToolPropsDefof
    {
        public static readonly ToolProps Shovel = new("Shovel")
        {
            Description = "Used to dig out grainy material like soil dirt and sand.",
            SpriteHandle = ItemContent.ShovelHandle,
            SpriteHead = ItemContent.ShovelHead,
            ToolUse = ToolUseDefOf.Digging,
            Skill = SkillDefOf.Digging,
            AssociatedJobs = new() { JobDefOf.Digger }
        };
        public static readonly ToolProps Axe = new("Axe")
        {
            Description = "Chops down trees.",
            SpriteHandle = ItemContent.AxeHandle,
            SpriteHead = ItemContent.AxeHead,
            ToolUse = ToolUseDefOf.Chopping,
            Skill = SkillDefOf.Plantcutting,
            AssociatedJobs = new() { JobDefOf.Lumberjack }
        };
        public static readonly ToolProps Hammer = new("Hammer")
        {
            Description = "Used for building.",
            SpriteHandle = ItemContent.HammerHandle,
            SpriteHead = ItemContent.HammerHead,
            ToolUse = ToolUseDefOf.Building,
            Skill = SkillDefOf.Construction,
            AssociatedJobs = new() { JobDefOf.Builder }
        };
        public static readonly ToolProps Pickaxe = new("Pickaxe")
        {
            Description = "Used for mining.",
            SpriteHandle = ItemContent.PickaxeHandle,
            SpriteHead = ItemContent.PickaxeHead,
            ToolUse = ToolUseDefOf.Mining,
            Skill = SkillDefOf.Mining,
            AssociatedJobs = new() { JobDefOf.Miner }
        };
        public static readonly ToolProps Handsaw = new("Handsaw")
        {
            Description = "Used for carpentry.",
            SpriteHandle = ItemContent.HandsawHandle,
            SpriteHead = ItemContent.HandsawHead,
            ToolUse = ToolUseDefOf.Carpentry,
            Skill = SkillDefOf.Carpentry,
            AssociatedJobs = new() { JobDefOf.Carpenter }
        };
        public static readonly ToolProps Hoe = new("Hoe")
        {
            Description = "Used to prepare soil for planting by converting it into farmland.",
            SpriteHandle = ItemContent.HoeHandle,
            SpriteHead = ItemContent.HoeHead,
            ToolUse = ToolUseDefOf.Argiculture,
            Skill = SkillDefOf.Argiculture,
            AssociatedJobs = new() { JobDefOf.Farmer }
        };
        static ToolPropsDefof()
        {
            Def.Register(typeof(ToolPropsDefof));

            //foreach (var toolProp in Def.GetDefs<ToolProps>())
            //{
            //    var obj = toolProp.Create();
            //    GameObject.AddTemplate(obj);
            //}

            //GenerateRecipesNew();
        }

        private static void GenerateRecipesNew()
        {
            var defs = Def.Database.Values.OfType<ToolProps>().ToList();
            foreach (var toolDef in defs)
            {
                var reagents = new List<Reaction.Reagent>();

                foreach (var reagent in CraftingProperties.ToolCraftingProperties.Reagents)
                    reagents.Add(reagent.Value);

                var reaction = new Reaction(
                    $"Craft {toolDef.Label}",
                    Reaction.CanBeMadeAt(IsWorkstation.Types.None, IsWorkstation.Types.Workbench),
                    reagents,
                    new List<Reaction.Product>() { new Reaction.Product(toolDef.Create) },
                    SkillDefOf.Crafting,
                    JobDefOf.Craftsman)
                { CreatesUnfinishedItem = true }
                    .ModWorkRequiredFromMaterials();

                Def.Register(reaction);
            }
        }
    }
}

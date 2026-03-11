using Project1.Core.Animations;
using Project1.Core.Entities;
using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Tools;
using Project1.Framework;
using System.Collections.Generic;

namespace Project1.Core.Systems.Consumables
{
    [EnsureStaticCtorCall]
    internal class ConsumableSystem
    {
        public static Dictionary<BoneDef, CraftingRules> Rules = [];
        static CraftingRules Rule;
        static ConsumableSystem()
        {
            //Rule = new CraftingRules(BoneDefOf.Item).Allow(MaterialDefOf.Berry);
            //CreateRuleFor(BoneDefOf.ToolHandle)
            //    .Allow(MaterialRefinementDefOf.Planks, MaterialRefinementDefOf.Ingots);
            //CreateRuleFor(BoneDefOf.ToolHead)
            //    .Allow(MaterialRefinementDefOf.Ingots, MaterialRefinementDefOf.Planks, MaterialRefinementDefOf.Chunk);
        }

        public static Entity Create(ConsumableDef profile, MaterialDef material)
        {
            var item = ItemDefOf.Consumable.Create();
            item.Profile = profile;
            item.Name = $"{material.LabelReadable} {profile.LabelReadable}";
            return item;
        }

        //public static IEnumerable<CraftingRules> GetRules(ConsumableDef def) => Rules.Values;

    }
}

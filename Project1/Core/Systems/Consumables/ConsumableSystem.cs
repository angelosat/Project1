using Project1.Core.Animations;
using Project1.Core.Entities;
using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Tools;
using Project1.Framework;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Project1.Core.Systems.Consumables
{
    [EnsureStaticCtorCall]
    internal static class ConsumableSystem
    {
        public static Dictionary<BoneDef, CraftingRules> Rules = [];
        //static CraftingRules Rule;
        static ConsumableSystem()
        {
            //Rule = new CraftingRules(BoneDefOf.Item).Allow(MaterialDefOf.Berry);
            //CreateRuleFor(BoneDefOf.ToolHandle)
            //    .Allow(MaterialRefinementDefOf.Planks, MaterialRefinementDefOf.Ingots);
            //CreateRuleFor(BoneDefOf.ToolHead)
            //    .Allow(MaterialRefinementDefOf.Ingots, MaterialRefinementDefOf.Planks, MaterialRefinementDefOf.Chunk);
        }

        public static Entity Create(ConsumableDef profile, MaterialDef material, int stackSize = -1)
        {
            var item = ItemDefOf.Consumable.Create(profile: profile, amount: stackSize);
            //item.Profile = profile;
            //item.Body.Sprite = Sprite.Default;
            item.Body.Sprite = profile.Sprite;
            item.Body.Material = material;
            item.Name = $"{material.LabelReadable} {profile.LabelReadable}";
            item.Initialize();
            profile.Worker.PostProcess(item);
            return item;
        }

        internal static Entity Create(EntityCreationRequest req)
        {
            return Create((ConsumableDef)req.Context, req.MaterialBindings[BoneDefOf.Item], req.StackSize);
        }

        //public static IEnumerable<CraftingRules> GetRules(ConsumableDef def) => Rules.Values;
        extension(Entity item)
        {
            public bool IsConsumable => item.Consumable is not null;
            public ConsumableComp Consumable => item.TryGetComponent<ConsumableComp>(out var comp) ? comp : null;
        }
    }
}

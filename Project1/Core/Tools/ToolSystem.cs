using Project1.Core.Animations;
using Project1.Core.Crafting;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Materials;
using Project1.Core.Resources;
using Project1.Core.Stats;
using Project1.Framework;
using System.Collections.Generic;

namespace Project1.Core.Tools
{
    [EnsureStaticCtorCall]
    internal static class ToolSystem
    {
        public static Dictionary<BoneDef, CraftingRules> Rules = [];
        static ToolSystem()
        {
            CreateRuleFor(BoneDefOf.ToolHandle)
                .Allow(MaterialRefinementDefOf.Planks, MaterialRefinementDefOf.Ingots);
            CreateRuleFor(BoneDefOf.ToolHead)
                .Allow(MaterialRefinementDefOf.Ingots, MaterialRefinementDefOf.Planks, MaterialRefinementDefOf.Chunk);
        }
        public static CraftingRules CreateRuleFor(BoneDef bone)
        {
            var rules = new CraftingRules(bone);
            Rules.Add(bone, rules);
            return rules;
        }
        public static CraftingRules GetRuleFor(BoneDef bone)
        {
            return Rules[bone];
        }
        public static IEnumerable<CraftingRules> GetRules() => Rules.Values;
        static public Entity Create(ToolProfileDef profile, MaterialDef handleMaterial, MaterialDef headMaterial)
        {
            var item = ItemDefOf.Tool.Create();//.Initialize();
            item.Profile = profile;
            item.ToolComponent.ToolDef = profile;

            var handle = item.Body.FindBone(BoneDefOf.ToolHandle);
            handle.Sprite = profile.SpriteHandle;
            handle.Material = headMaterial;

            var head = item.Body.FindBone(BoneDefOf.ToolHead);
            head.Sprite = profile.SpriteHead;
            head.Material = handleMaterial;

            item.Name = profile.LabelReadable;

            BakeStats(item);
            item.Initialize();
            return item;
        }
        
        static internal void BakeStats(Entity tool)
        {
            var comp = tool.GetComponent<StatsComponent>();
            comp.Bake(StatDefOf.ToolSpeed, BoneDefOf.ToolHandle);
            comp.Bake(StatDefOf.ToolEffectiveness, BoneDefOf.ToolHead);
        }
        internal static Entity Create(EntityCreationRequest req)
        {
            return Create(req.Context as ToolProfileDef, req.MaterialBindings[BoneDefOf.ToolHandle], req.MaterialBindings[BoneDefOf.ToolHead]);
        }

        internal static Entity CreateUnfinishedItem(Actor author, CraftingOrder order, MaterialDef handleMaterial, MaterialDef headMaterial)
        {
            var item = ItemDefOf.UnfinishedItem.Create();
            var profile = order.ProductDef;
            item.Profile = profile;
            var comp = item.GetComponent<UnfinishedItemComp>();
            comp.Initialize(author, order, [handleMaterial ,headMaterial]);
            var assembly = item.Resources.GetResource(ResourceDefOf.Assembly);
            assembly.SetValue(0);
            assembly.Max = 110;
            item.Initialize();
            item.SetName($"{profile.LabelReadable} (unfinished)");
            order.UnfinishedItem = item;
            return item;
        }
    }
    record CraftingRules(BoneDef Bone)
    {
        public MaterialRefinementDef Refinement;
        public readonly HashSet<MaterialRefinementDef> Types = [];
        public CraftingRules Allow(params MaterialRefinementDef[] types)
        {
            foreach (var type in types)
                this.Types.Add(type);
            return this;
        }

        public CraftingRules From(MaterialRefinementDef state)
        {
            this.Refinement = state;
            return this;
        }
    }
}

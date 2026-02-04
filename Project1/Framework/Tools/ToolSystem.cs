using Project1.Core.Entities;
using Project1.Core.Materials;
using Project1.Framework.Animations;
using Project1.Framework.Entities;
using Project1.Framework.Materials;
using Project1.Framework.Stats;
using Start_a_Town_;
using System.Collections.Generic;

namespace Project1.Framework.Tools
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

            item.Name = profile.Label;

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

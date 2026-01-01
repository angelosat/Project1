using System.Collections.Generic;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    public abstract class ItemCreationArgs;
    internal static class ToolSystem
    {
        public static Dictionary<BoneDef, CraftingRules> Rules = [];
        static ToolSystem()
        {
            CreateRuleFor(BoneDefOf.ToolHandle)
                //.ForBone(BoneDefOf.ToolHandle)
                .Allow(MaterialRefinementDefOf.Planks, MaterialRefinementDefOf.Ingots);
            CreateRuleFor(BoneDefOf.ToolHead)
                .Allow(MaterialRefinementDefOf.Ingots, MaterialRefinementDefOf.Planks, MaterialRefinementDefOf.Chunk);
            //CreateRuleFor(BoneDefOf.ToolHandle)
            //    .From(MaterialRefinementDefOf.Ingots)
            //    .From(MaterialRefinementDefOf.Planks)
            //    .Allow(MaterialTypeDefOf.Wood, MaterialTypeDefOf.Metal);
            //CreateRuleFor(BoneDefOf.ToolHead)
            //    .From(MaterialRefinementDefOf.Ingots)
            //    .From(MaterialRefinementDefOf.Planks)
            //    .From(MaterialRefinementDefOf.Chunk)
            //    .Allow(MaterialTypeDefOf.Wood, MaterialTypeDefOf.Metal, MaterialTypeDefOf.Stone);
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
            var item = ItemDefOf.Tool.Create().Initialize();

            item.ToolComponent.ToolDef = profile;

            var handle = item.Body.FindBone(BoneDefOf.ToolHandle);
            handle.Sprite = profile.SpriteHandle;
            handle.Material = headMaterial;

            var head = item.Body.FindBone(BoneDefOf.ToolHead);
            head.Sprite = profile.SpriteHead;
            head.Material = handleMaterial;

            item.Name = profile.Label;

            return item;
        }
        internal static Entity Create(EntityCreationRequest req)
        {
            return Create(req.Context as ToolProfileDef, req.MaterialBindings[BoneDefOf.ToolHandle], req.MaterialBindings[BoneDefOf.ToolHead]);
        }
    }
    record CraftingRules(BoneDef Bone)
    {
        //public BoneDef Bone;
        public MaterialRefinementDef Refinement;
        public readonly HashSet<MaterialRefinementDef> Types = [];
        //public CraftingRules()
        //{
                
        //}
        //public CraftingRules(BoneDef bone, MaterialRefinementDef state, MaterialTypeDef[] types)
        //{
        //    this.Refinement = state;
        //    this.Bone = bone;
        //    foreach (var type in types)
        //        this.Types.Add(type);
        //}
        //public CraftingRules Allow(params MaterialTypeDef[] types)
        //{
        //    foreach (var type in types)
        //        this.Types.Add(type);
        //    return this;
        //}
        public CraftingRules Allow(params MaterialRefinementDef[] types)
        {
            foreach (var type in types)
                this.Types.Add(type);
            return this;
        }
        //public CraftingRules ForBone(BoneDef bone)
        //{
        //    this.Bone = bone;
        //    return this;
        //}
        public CraftingRules From(MaterialRefinementDef state)
        {
            this.Refinement = state;
            return this;
        }
        //public CraftingRules Disallow(params MaterialTypeDef[] types)
        //{
        //    foreach (var type in types)
        //        this.Types.Remove(type);
        //    return this;
        //}
    }
}

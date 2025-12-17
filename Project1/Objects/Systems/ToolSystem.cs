using System.Collections.Generic;

namespace Start_a_Town_
{
    public abstract class ItemCreationArgs;
    internal static class ToolSystem// : IItemCreationSystem
    {
        public static Dictionary<BoneDef, CraftingRules> Rules = [];
        static ToolSystem()
        {
            CreateRuleFor(BoneDefOf.ToolHandle)
                .From(RawMaterialStageDefOf.Processed)
                .Allow(MaterialTypeDefOf.Wood, MaterialTypeDefOf.Metal);
            CreateRuleFor(BoneDefOf.ToolHead)
                .From(RawMaterialStageDefOf.Processed)
                .Allow(MaterialTypeDefOf.Wood, MaterialTypeDefOf.Metal);
        }
        public static CraftingRules CreateRuleFor(BoneDef bone)
        {
            var rules = new CraftingRules();
            Rules.Add(bone, rules);
            return rules;
        }
        public static CraftingRules GetRuleFor(BoneDef bone)
        {
            return Rules[bone];
        }
        static public Entity Create(ToolProfileDef profile, MaterialDef handleMaterial, MaterialDef headMaterial)
        {
            //if (def is not ToolProfileDef profile)
            //    throw new InvalidOperationException($"{nameof(ToolSystem)} received wrong profile");

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
    class CraftingRules
    {
        public BoneDef Bone;
        public RawMaterialStateDef State;
        public readonly HashSet<MaterialTypeDef> Types = [];
        public CraftingRules()
        {
                
        }
        public CraftingRules(BoneDef bone, RawMaterialStateDef state, MaterialTypeDef[] types)
        {
            this.State = state;
            this.Bone = bone;
            foreach (var type in types)
                this.Types.Add(type);
        }
        public CraftingRules Allow(params MaterialTypeDef[] types)
        {
            foreach (var type in types)
                this.Types.Add(type);
            return this;
        }
        public CraftingRules ForBone(BoneDef bone)
        {
            this.Bone = bone;
            return this;
        }
        public CraftingRules From(RawMaterialStateDef state)
        {
            this.State = state;
            return this;
        }
        public CraftingRules Disallow(params MaterialTypeDef[] types)
        {
            foreach (var type in types)
                this.Types.Remove(type);
            return this;
        }
    }
}

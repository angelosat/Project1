using Project1.Core.Animations;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Resources;
using Project1.Core.Skills;
using Project1.Core.Systems.Materials;
using Project1.Framework.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Crafting
{
    public abstract class WorkstationCapabilityWorker//WorkstationCapabilityDef def)
    {
        //public readonly WorkstationCapabilityDef CapabilityDef = def;
        public abstract WorkstationCapabilityDef CapabilityDef { get; }
        public abstract bool CreatesUnfinished { get; }
        public abstract SkillDef CraftingSkill { get; }
        public virtual (ResourceDef resource, int value) ResourceConsumption { get; }

        public abstract IEnumerable<AddOrderRequest> GetAddOrderRequests(BlockWorkstationComp comp);
        public abstract IEnumerable<(Def[] validRefinements, int quantity)> GetValidIngredientsPerSlot(Def recipe);
        public abstract IEnumerable<CraftingRule> GetCraftingRulesStruct(Def recipe);
        public abstract IEnumerable<BoneDef> GetBoneLayout();

        public Dictionary<BoneDef, Entity> GetIngredientMapping(Def recipe, IEnumerable<Entity> ingredients)
            => this.GetBoneLayout().Zip(ingredients).ToDictionary();
        public Dictionary<BoneDef, MaterialDef> MapBonesToMaterials(Def recipe, IEnumerable<MaterialDef> materials)
            => this.GetBoneLayout().Zip(materials).ToDictionary();

        internal virtual int GetOutputStackSize(Def recipe) => 1;
    }
}

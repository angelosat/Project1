using Project1.Core.Animations;
using Project1.Core.Materials;
using System.Collections.Generic;

namespace Project1.Core.Tools
{
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

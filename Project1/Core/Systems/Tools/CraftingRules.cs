using Project1.Core.Animations;
using System.Collections.Generic;

namespace Project1.Core.Systems.Tools;

record CraftingRules(BoneDef Bone)
{
    //public MaterialRefinementDef Refinement;
    public readonly HashSet<Def> Profiles = [];
    public CraftingRules Allow(params Def[] types)
    {
        foreach (var type in types)
            this.Profiles.Add(type);
        return this;
    }

    //public CraftingRules From(MaterialRefinementDef state)
    //{
    //    this.Refinement = state;
    //    return this;
    //}
}

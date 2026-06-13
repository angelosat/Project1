using Project1.Core.Animations;
using Project1.Core.Systems.Materials;
using System.Collections.Generic;

namespace Project1.Core.Systems.Gear;

public class BoneMaterialSet
{
    readonly Dictionary<BoneDef, MaterialFilter> Set = [];

    public BoneMaterialSet Allow(BoneDef bone, MaterialFilter filter)
    {
        this.Set.Add(bone, filter);
        return this;
    }

    static readonly public BoneMaterialSet ToolDefault = new()
    {
        Set =
        {
            { BoneDefOf.ToolHandle, MaterialFilter.Allow(MaterialTypeDefOf.Wood, MaterialTypeDefOf.Metal) },
            { BoneDefOf.ToolHead, MaterialFilter.Allow(MaterialTypeDefOf.Wood, MaterialTypeDefOf.Metal) }
        }
    };
}

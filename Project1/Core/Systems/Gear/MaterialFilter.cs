using Project1.Core.Systems.Materials;
using System.Collections.Generic;

namespace Project1.Core.Systems.Gear;
public class MaterialFilter
{
    HashSet<MaterialTypeDef> _materialTypes = [];
    public IReadOnlySet<MaterialTypeDef> MaterialTypes => this._materialTypes;

    static public MaterialFilter Allow(params MaterialTypeDef[] materialTypes)
        => new() { _materialTypes = [.. materialTypes] };
}

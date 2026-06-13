using Project1.Core.Systems.Materials;
using System.Collections.Generic;

namespace Project1.Core.Systems.Gear;
public class MaterialFilter
{
    HashSet<MaterialTypeDef> Materials = [];

    static public MaterialFilter Allow(params MaterialTypeDef[] materialTypes)
        => new() { Materials = [.. materialTypes] };
}

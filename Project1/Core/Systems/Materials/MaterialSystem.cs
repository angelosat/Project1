using Project1.Core.Animations;
using Project1.Core.Entities;
using Project1.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Materials;

[EnsureStaticCtorCall]
public class MaterialSystem
{
    internal static readonly Dictionary<MaterialTypeDef, HashSet<MaterialDef>> MaterialsByType = [];
    static readonly Dictionary<int, HashSet<MaterialDef>> MaterialsByTier = [];
    static readonly Dictionary<int, Dictionary<MaterialRefinementDef, MaterialDef>> RefinemenetsByTier = [];
    static MaterialSystem()
    {
        var allmats = Def.Get<MaterialDef>();
        foreach (var matdef in allmats)
        {
            if (!MaterialsByType.TryGetValue(matdef.Type, out var list))
                MaterialsByType[matdef.Type] = list = [];
            list.Add(matdef);
            var tier = matdef.Tier;
            if (!MaterialsByTier.TryGetValue(tier, out var listByTier))
                MaterialsByTier[tier] = listByTier = [];
            listByTier.Add(matdef);

        }

        var allRefs = Def.Get<MaterialRefinementDef>();
        foreach (var refDef in allRefs)
        {
            var typ = refDef.MaterialType;
            var mats = MaterialsByType[typ];
            foreach(var mat in mats)
            {
                var tier = mat.Tier;
                if (!RefinemenetsByTier.TryGetValue(tier, out var dicType))
                    RefinemenetsByTier[tier] = dicType = [];
                dicType.Add(refDef, mat);
            }
        }
    }
    public static List<MaterialDef> ByTier(int tier)
        => [.. RefinemenetsByTier[tier].Select(e => e.Value)];
    public static MaterialDef ByTierAndType(int tier, MaterialRefinementDef refinement)
        => RefinemenetsByTier[tier][refinement];
    public static IReadOnlySet<MaterialDef> GetMaterialsByType(MaterialTypeDef typeDef)
        => typeDef is null ? [] : MaterialsByType[typeDef];
    static public Entity Create(MaterialRefinementDef profile, MaterialDef material, int stackSize = -1)
    {
        return Create(profile, material, [], stackSize);
    }

    static public Entity Create(MaterialRefinementDef stage, MaterialDef defaultMaterial, Dictionary<BoneDef, MaterialDef> materialOverrides, int stackSize = -1)
    {
        var item = ItemDefOf.Ingredient.Create(amount: stackSize);
        item.Profile = stage;
        item.Body.Sprite = stage.Sprite;
        foreach (var bone in item.Body.GetAllBones())
            if (materialOverrides.TryGetValue(bone.Def, out var overridenMaterial))
                bone.Material = overridenMaterial;
            else
                bone.Material = defaultMaterial;
        item.Name = $"{item.Body.Material.LabelReadable} {stage.LabelReadable}";
        item.Initialize();
        return item;
    }

    static public IEnumerable<Entity> GenerateTemplates()
    {
        var states = Def.Get<MaterialRefinementDef>();
        var materials = Def.Get<MaterialDef>();

        foreach (var state in states)
            foreach (var material in materials.Where(m => m.Type == state.MaterialType))
                yield return EntityFactory
                    .Request(state, defaultMaterial: material)
                    .Create();
    }

    internal static Entity Create(EntityCreationRequest req)
    {
        return Create(req.Context as MaterialRefinementDef, req.DefaultMaterial, req.MaterialBindings, req.StackSize);// req.DefaultMaterial);
    }
}

using Project1.Core.Animations;
using Project1.Core.Entities;
using Project1.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Materials
{
    [EnsureStaticCtorCall]
    public class RawMaterialSystem
    {
        public static readonly Dictionary<MaterialTypeDef, HashSet<MaterialDef>> MaterialsByType = [];
        static RawMaterialSystem()
        {
            foreach (var matdef in Def.GetDefs<MaterialDef>())
            {
                if (!MaterialsByType.TryGetValue(matdef.Type, out var list))
                    MaterialsByType[matdef.Type] = list = [];
                list.Add(matdef);
            }
        }

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
            var states = Def.GetDefs<MaterialRefinementDef>();
            var materials = Def.GetDefs<MaterialDef>();

            foreach (var state in states)
                foreach (var material in Def.GetDefs<MaterialDef>().Where(m => m.Type == state.MaterialType))
                    yield return EntityFactory
                        .Request(state, defaultMaterial: material)
                        .Create();
        }

        internal static Entity Create(EntityCreationRequest req)
        {
            return Create(req.Context as MaterialRefinementDef, req.DefaultMaterial, req.MaterialBindings, req.StackSize);// req.DefaultMaterial);
        }
    }
}

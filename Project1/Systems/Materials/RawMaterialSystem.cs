using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    public class RawMaterialSystem
    {
        public static readonly Dictionary<(MaterialTypeDef type, RefinementPathDef process), MaterialMappingDef> ByTypeAndProcess = [];
        public static readonly Dictionary<MaterialTypeDef, HashSet<MaterialDef>> MaterialsByType = [];
        static RawMaterialSystem()
        {
            foreach(var mappingDef in Def.GetDefs<MaterialMappingDef>())
                ByTypeAndProcess[mappingDef.Mapping] = mappingDef;

            foreach(var matdef in Def.GetDefs<MaterialDef>())
            {
                if (!MaterialsByType.TryGetValue(matdef.Type, out var list))
                    MaterialsByType[matdef.Type] = list = [];
                list.Add(matdef);
            }
        }
        static public Entity Create(MaterialRefinementDef stage, Dictionary<BoneDef, MaterialDef> materials, int stackSize = -1)
        {
            var item = ItemDefOf.Ingredient.Create(amount: stackSize);
            item.Initialize();
            item.Profile = stage;
            item.Body.Sprite = stage.Sprite;
            foreach(var (bone, mat) in materials)
                item.Body.FindBone(bone).Material = mat;
            item.Name = $"{item.Body.Material.Label} {stage.Label}";
            return item;
        }
        static public IEnumerable<Entity> GenerateTemplates()
        {
            var states = Def.GetDefs<MaterialRefinementDef>();
            var materials = Def.GetDefs<MaterialDef>();

            foreach (var state in states)
                foreach (var material in Def.GetDefs<MaterialDef>().Where(m => m.Type == state.MaterialType))
                    yield return EntityFactory
                        .Request(state, null)
                        .Override(BoneDefOf.Item, material)
                        .Create();
        }
        internal static Entity Create(EntityCreationRequest req)
        {
            return Create(req.Context as MaterialRefinementDef, req.MaterialBindings, req.StackSize);// req.DefaultMaterial);
        }
    }
}

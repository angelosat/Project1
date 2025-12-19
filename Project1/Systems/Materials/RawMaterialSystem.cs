using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

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
            {
                ByTypeAndProcess[mappingDef.Mapping] = mappingDef;
            }

            foreach(var matdef in Def.GetDefs<MaterialDef>())
            {
                if (!MaterialsByType.TryGetValue(matdef.Type, out var list))
                    MaterialsByType[matdef.Type] = list = [];
                list.Add(matdef);
            }
        }
        static public Entity Create(MaterialRefinementDef stage, MaterialDef material)
        {
            //if (stage == null)
            //{
            //    Log.Warning($"No stage provided for {material}, defaulting to {RefinementPathDefOf.Raw}.");
            //    stage = RefinementPathDefOf.Raw;
            //}
            //if (!ByTypeAndProcess.TryGetValue((material.Type, stage), out var mapping))
            //    return null;
            //throw new ArgumentException($"No {nameof(MaterialMappingDef)} for {material.Label} / {stage.Label}");

            var item = ItemDefOf.Ingredient.Create();
            item.Initialize();
            item.Profile = stage;
            item.Body.Material = material;
            item.Body.Sprite = stage.Sprite;
            item.Name = $"{material.Label} {stage.Label}";

            return item;
        }
        //static public Entity Create(RefinementPathDef stage, MaterialDef material)
        //{
        //    if (stage == null)
        //    {
        //        Log.Warning($"No stage provided for {material}, defaulting to {RefinementPathDefOf.Raw}.");
        //        stage = RefinementPathDefOf.Raw;
        //    }
        //    if (!ByTypeAndProcess.TryGetValue((material.Type, stage), out var mapping))
        //        return null;
        //    //throw new ArgumentException($"No {nameof(MaterialMappingDef)} for {material.Label} / {stage.Label}");

        //    var item = stage.Item.Create();
        //    item.Initialize();
        //    item.Body.Material = material;
        //    item.Body.Sprite = mapping.Sprite;
        //    item.Name = $"{material.Label} {mapping.Label}";

        //    return item;
        //}

        //static public Entity Create(MaterialDef material, RefinementPathDef stage)
        //{
        //    if (stage == null)
        //    {
        //        Log.Warning($"No stage provided for {material}, defaulting to {RefinementPathDefOf.Raw}.");
        //        stage = RefinementPathDefOf.Raw;
        //    }
        //    if (!ByTypeAndProcess.TryGetValue((material.Type, stage), out var mapping))
        //        return null;
        //        //throw new ArgumentException($"No {nameof(MaterialMappingDef)} for {material.Label} / {stage.Label}");

        //    var item = stage.Item.Create();
        //    item.Initialize();
        //    item.Body.Material = material;
        //    item.Body.Sprite = mapping.Sprite;
        //    item.Name = $"{material.Label} {mapping.Label}";

        //    return item;
        //}

        static public IEnumerable<Entity> GenerateTemplates()
        {
            //var stages = Def.GetDefs<RefinementPathDef>();
            var states = Def.GetDefs<MaterialRefinementDef>();
            var materials = Def.GetDefs<MaterialDef>();

            foreach (var state in states)
                foreach (var material in Def.GetDefs<MaterialDef>().Where(m=>m.Type == state.MaterialType))
                    yield return EntityFactory
                        //.Request(material, state)
                        .Request(state, null, material)
                        .Create();
        }

        internal static Entity Create(EntityCreationRequest req)
        {
            //return Create(req.Context as MaterialDef, req.Stage as RefinementPathDef);
            return Create(req.Context as MaterialRefinementDef, req.DefaultMaterial);
        }
    }
}

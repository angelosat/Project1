using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    public class RawMaterialSystem
    {
        public static readonly Dictionary<(MaterialTypeDef type, MaterialFormDef process), MaterialMappingDef> ByTypeAndProcess = [];
        static RawMaterialSystem()
        {
            foreach(var mappingDef in Def.GetDefs<MaterialMappingDef>())
            {
                ByTypeAndProcess[mappingDef.Mapping] = mappingDef;
            }
        }

        static public Entity Create(MaterialDef material, MaterialFormDef stage)
        {
            if (stage == null)
            {
                Log.Warning($"No stage provided for {material}, defaulting to {MaterialFormDefOf.Raw}.");
                stage = MaterialFormDefOf.Raw;
            }
            if (!ByTypeAndProcess.TryGetValue((material.Type, stage), out var mapping))
                return null;
                //throw new ArgumentException($"No {nameof(MaterialMappingDef)} for {material.Label} / {stage.Label}");

            var item = stage.Item.Create();
            item.Initialize();
            item.Body.Material = material;
            item.Body.Sprite = mapping.Sprite;
            item.Name = $"{material.Label} {mapping.Label}";

            return item;
        }

        static public IEnumerable<Entity> GenerateTemplates()
        {
            var stages = Def.GetDefs<MaterialFormDef>();
            var materials = Def.GetDefs<MaterialDef>();

            foreach (var stage in stages)
                foreach (var material in materials)
                    yield return EntityFactory
                        .Request(material, stage)
                        .Create();
        }

        internal static Entity Create(EntityCreationRequest req)
        {
            return Create(req.Context as MaterialDef, req.Stage as MaterialFormDef);
        }
    }
}

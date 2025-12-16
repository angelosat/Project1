using Start_a_Town_.AI.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    public class RawMaterialSystem
    {
        public static readonly Dictionary<(MaterialTypeDef type, MaterialStageDef process), MaterialMappingDef> ByTypeAndProcess = [];
        static RawMaterialSystem()
        {
            foreach(var mappingDef in Def.GetDefs<MaterialMappingDef>())
            {
                ByTypeAndProcess[mappingDef.Mapping] = mappingDef;
            }
        }

        static public Entity Create(MaterialDef material, MaterialStageDef stage)
        {
            if(!ByTypeAndProcess.TryGetValue((material.Type, stage), out var mapping))
                throw new ArgumentException($"No {nameof(MaterialMappingDef)} for {material.Label} / {stage.Label}");

            var item = stage.Item.Create();
            item.Initialize();
            item.Body.Material = material;
            item.Body.Sprite = mapping.Sprite;
            item.Name = $"{material.Label} {stage.Label}";

            return item;
        }
    }
}

using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    public class RawMaterialSystem
    {
        public static readonly Dictionary<(MaterialTypeDef type, MaterialProcessDef process), MaterialMappingDef> ByTypeAndProcess = [];
        static RawMaterialSystem()
        {
            foreach(var mappingDef in Def.GetDefs<MaterialMappingDef>())
            {
                ByTypeAndProcess[mappingDef.Mapping] = mappingDef;
            }
        }

        static public Entity Create(MaterialDef material, MaterialProcessDef process)
        {
            ByTypeAndProcess.TryGetValue((material.Type, process), out var mapping);
            if (mapping == null)
                throw new ArgumentException();


            var item = ItemDefOf.Coins.Create();
            return item;
        }
    }
}

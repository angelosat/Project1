using Project1.Core.Entities;
using Project1.Core.Helpers;
using Project1.Core.Materials;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Legacy
{
    static class ItemFactory
    {
        [Obsolete]
        static public Entity CreateFrom(ItemDef def, MaterialDef mat)
        {
            var obj = def.Create();
            obj.SetMaterial(mat);
            return obj;
        }
        static public Dictionary<string, MaterialDef> GetRandomMaterialsFor(ItemDef def)
        {
            var dic = new Dictionary<string, MaterialDef>();
            foreach (var r in def.CraftingProperties.Reagents)
                dic[r.Value.Name] = r.Value.Ingredient.GetAllValidMaterials().ToArray().SelectRandom(MaterialDef.Randomizer);
            return dic;
        }
    }
}

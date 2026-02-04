using Project1.Framework.Entities;
using Project1.Framework.Materials;
using Start_a_Town_;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Framework.Legacy
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

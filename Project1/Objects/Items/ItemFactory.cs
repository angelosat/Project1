using Start_a_Town_.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    static class ItemFactory
    {
        //static public Entity CreateTool(ItemDef def)
        //{
        //    var obj = new Entity
        //    {
        //        Def = def,
        //    };

        //    obj.AddComponent<ToolAbilityComponent>();
        //    obj.AddComponent(new SpriteComponent(def));

        //    obj.InitComps();

        //    return obj;
        //}
        //static public Entity CreateTool(ItemDef def, Dictionary<string, Entity> ingredients)
        //{
        //    var obj = CreateTool(def);
        //    obj.SetMaterials(ingredients.ToDictionary(i => i.Key, i => i.Value.PrimaryMaterial));
        //    return obj;
        //}
        //static public Entity CreateFromRandomMaterials(ItemDef def)
        //{
        //    var mats = GetRandomMaterialsFor(def);
        //    var obj = CreateItem(def);
        //    obj.SetMaterials(mats);
        //    obj.SetQuality(Quality.GetRandom());
        //    return obj;
        //}
        //static public Entity CreateToolFromRandomMaterials(ItemDef def)
        //{
        //    var mats = GetRandomMaterialsFor(def);
        //    var obj = CreateTool(def);
        //    obj.SetMaterials(mats);
        //    obj.SetQuality(Quality.GetRandom());
        //    return obj;
        //}
        [Obsolete($"use {nameof(def.CreateNew)} instead")]
        static public Entity CreateItem(ItemDef def) => def.CreateNew();
        //{
        //    var obj = Activator.CreateInstance(def.ItemClass) as Entity;
        //    obj.Def = def;
        //    obj.AddComponent(new SpriteComp(def));
        //    obj.InitComps();
        //    return obj;
        //}

        static public Entity CreateFrom(ItemDef def, MaterialDef mat)
        {
            var obj = def.CreateNew();
            //var obj = CreateItem(def);
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

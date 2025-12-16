using System;
using System.Collections.Generic;
using System.Text;

namespace Start_a_Town_
{
    internal static class RawMaterialDefOfNew
    {
        static public readonly ItemDef Raw = new ItemDef("Raw", typeof(Entity))
        {
            BaseValue = 5,
            Description = "Raw Material",
            Category = ItemCategoryDefOf.RawMaterials,
            Body = new Bone(BoneDefOf.Item, Sprite.Default) { DrawMaterialColor = true },
        };
        static public readonly ItemDef Processed = new ItemDef("Processed", typeof(Entity))
        {
            BaseValue = 10,
            Description = "Processed Material",
            Category = ItemCategoryDefOf.RawMaterials,
            Body = new Bone(BoneDefOf.Item, Sprite.Default) { DrawMaterialColor = true },
        };
        static public readonly ItemDef Refined = new ItemDef("Refined", typeof(Entity))
        {
            BaseValue = 20,
            Description = "Refined Material",
            Category = ItemCategoryDefOf.RawMaterials,
            Body = new Bone(BoneDefOf.Item, Sprite.Default) { DrawMaterialColor = true },
        };
        static public readonly ItemDef Ground = new ItemDef("Ground", typeof(Entity))
        {
            BaseValue = 1,
            Description = "Ground Material",
            Category = ItemCategoryDefOf.RawMaterials,
            Body = new Bone(BoneDefOf.Item, Sprite.Default) { DrawMaterialColor = true },
        };
    }
}

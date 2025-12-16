namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal static class RawMaterialDefOfNew
    {
        static public readonly ItemDef Raw = new ItemDef("Raw", typeof(Item))
        {
            BaseValue = 5,
            Description = "Raw Material",
            Category = ItemCategoryDefOf.RawMaterials,
            Body = new Bone(BoneDefOf.Item, Sprite.Default) { DrawMaterialColor = true },
        };
        static public readonly ItemDef Processed = new ItemDef("Processed", typeof(Item))
        {
            BaseValue = 10,
            Description = "Processed Material",
            Category = ItemCategoryDefOf.RawMaterials,
            Body = new Bone(BoneDefOf.Item, Sprite.Default) { DrawMaterialColor = true },
        };
        static public readonly ItemDef Refined = new ItemDef("Refined", typeof(Item))
        {
            BaseValue = 20,
            Description = "Refined Material",
            Category = ItemCategoryDefOf.RawMaterials,
            Body = new Bone(BoneDefOf.Item, Sprite.Default) { DrawMaterialColor = true },
        };
        static public readonly ItemDef Ground = new ItemDef("Ground", typeof(Item))
        {
            BaseValue = 1,
            Description = "Ground Material",
            Category = ItemCategoryDefOf.RawMaterials,
            Body = new Bone(BoneDefOf.Item, Sprite.Default) { DrawMaterialColor = true },
        };
        static RawMaterialDefOfNew()
        {
            Def.Register(typeof(RawMaterialDefOfNew));
        }
    }
}

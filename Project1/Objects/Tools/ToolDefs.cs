using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    static class ToolDefs
    {
        public static readonly CraftingProperties ToolCraftingProperties = new()
        {
            Reagents = new Dictionary<BoneDef, Reaction.Reagent>()
                {
                    { BoneDefOf.ToolHandle, new Reaction.Reagent("Handle", new Ingredient(null, null, null).SetAllow(ItemCategoryDefOf.Manufactured, true)) }, //.IsBuildingMaterial()
                    { BoneDefOf.ToolHead, new Reaction.Reagent("Head", new Ingredient(null, null, null).SetAllow(ItemCategoryDefOf.Manufactured, true))  }
                }
        };

        static public readonly ItemDef Tool = new("Tool")
        {
            ItemClass = typeof(Tool),
            QualityLevels = true,
            Category = ItemCategoryDefOf.Equipment,
            MadeFromMaterials = true,
            GearType = GearType.Mainhand,
            DefaultMaterial = MaterialDefOf.Iron,
            Factory = d => d.CreateNew(),
            CraftingProperties = ToolCraftingProperties,
            Body = new Bone(BoneDefOf.ToolHandle, ItemContent.LogsGrayscale, Vector2.Zero, 0.001f) { DrawMaterialColor = true, OriginGroundOffset = new Vector2(0, -16) }
                            .AddJoint(Vector2.Zero, new Bone(BoneDefOf.ToolHead, ItemContent.LogsGrayscale) { DrawMaterialColor = true }),
            NameGetter = e => e.ToolComponent.Props.Label,
            StorageFilterVariations = Def.GetDefs<ToolProps>(),
            VariationGetter = e => e.ToolComponent.Props
        };

        static public void Init() { }
        static ToolDefs()
        {
            Def.Register(Tool);
        }
    }
}
 

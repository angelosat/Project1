using Project1.Core.Animations;
using Project1.Core.Assets;
using Project1.Core.Entities;
using Project1.Core.Graphics;
using Project1.Core.Legacy.Properties;
using Project1.Core.Legacy.Storage;
using Project1.Framework;
using System.Collections.Generic;

namespace Project1.Core.Systems.Materials
{
    [EnsureStaticCtorCall]
    public class RawMaterialDefOf
    {
        static public readonly ItemDef Planks = new ItemDef("Plank", typeof(Entity))
        {
            BaseValue = 5,
            Description = "Processed logs",
            StackCapacity = 24,
            Weight = .1f,
            Category = ItemCategoryDefOf.Manufactured,
            Body = new Bone(BoneDefOf.Item, ItemContent.PlanksGrayscale) { DrawMaterialColor = true },
            CanProcessInto = new List<Reaction.Product.Types>() { Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks, Reaction.Product.Types.Workbenches },
            DefaultMaterialType = MaterialTypeDefOf.Wood,
            CraftingProperties = new CraftingProperties()
            {
                IsBuildingMaterial = true,
                IsCraftingMaterial = true
            }
        }.SetMadeFrom(MaterialTypeDefOf.Wood)
            ;

        static public readonly ItemDef Logs = new ItemDef("Logs", typeof(Entity))
        {
            BaseValue = 1,
            Description = "It came from a tree",
            StackCapacity = 6,
            Body = new Bone(BoneDefOf.Item, ItemContent.LogsGrayscale) { DrawMaterialColor = true },
            Category = ItemCategoryDefOf.RawMaterials,
            CanProcessInto = new List<Reaction.Product.Types>() { Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks, Reaction.Product.Types.Workbenches },
            CraftingProperties = new CraftingProperties() { IsBuildingMaterial = true },
            DefaultMaterialType = MaterialTypeDefOf.Wood
        }.SetMadeFrom(MaterialTypeDefOf.Wood)
            ;


        static public readonly ItemDef Bags = new ItemDef("Bag", typeof(Entity))
        {
            BaseValue = 1,
            Description = "A bag containing grainy material",
            StackCapacity = 10,
            Category = ItemCategoryDefOf.RawMaterials,
            Body = new Bone(BoneDefOf.Item, ItemContent.BagsGrayscale) { DrawMaterialColor = true },
            CanProcessInto = new List<Reaction.Product.Types>() { Reaction.Product.Types.Blocks },
            DefaultMaterialType = MaterialTypeDefOf.Soil,
        }.SetMadeFrom(MaterialTypeDefOf.Soil)
            ;


        static public readonly ItemDef Ingots = new ItemDef("Ingot", typeof(Entity))
        {
            BaseValue = 5,
            Description = "Used for crafting of weapons, armor, and tools.",
            StackCapacity = 20,
            Category = ItemCategoryDefOf.Manufactured,
            Body = new Bone(BoneDefOf.Item, ItemContent.BarsGrayscale) { DrawMaterialColor = true },
            CanProcessInto = new List<Reaction.Product.Types>() { Reaction.Product.Types.Tools },
            DefaultMaterialType = MaterialTypeDefOf.Metal,
            CraftingProperties = new CraftingProperties() { IsCraftingMaterial = true, IsBuildingMaterial = true },
        }.SetMadeFrom(MaterialTypeDefOf.Metal)
            ;


        static public readonly ItemDef Ore = new ItemDef("Ore", typeof(Entity))
        {
            BaseValue = 1,
            Description = "A piece of mineral ore",
            StackCapacity = 10,
            Category = ItemCategoryDefOf.RawMaterials,
            Body = new Bone(BoneDefOf.Item, ItemContent.OreGrayscale) { DrawMaterialColor = true },
            CanProcessInto = new List<Reaction.Product.Types>() { Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks },
            DefaultMaterialType = MaterialTypeDefOf.Metal,
        };

        static public readonly ItemDef Boulders = new ItemDef("Boulders", typeof(Entity))
        {
            BaseValue = 1,
            Description = "Chunks of rock",
            StackCapacity = 10,
            Category = ItemCategoryDefOf.RawMaterials,
            Body = new Bone(BoneDefOf.Item, ItemContent.OreGrayscale) { DrawMaterialColor = true },
            CanProcessInto = new List<Reaction.Product.Types>() { Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks },
            DefaultMaterialType = MaterialTypeDefOf.Stone,
            CraftingProperties = new CraftingProperties() { IsBuildingMaterial = true, IsCraftingMaterial = true },
        }.SetMadeFrom(MaterialTypeDefOf.Stone)
            ;

        static public readonly ItemDef Scraps = new ItemDef("Scraps", typeof(Entity))
        {
            StackDimension = 4,
            StackCapacity = 50,
            BaseValue = 0,
            Description = "Worthless but can be repurposed",
            Category = ItemCategoryDefOf.RawMaterials,
            Body = new Bone(BoneDefOf.Item, Sprite.Default) { DrawMaterialColor = true },
        }.SetMadeFrom(MaterialTypeDefOf.Wood, MaterialTypeDefOf.Stone, MaterialTypeDefOf.Metal)
            ;
    }
}

using Microsoft.Xna.Framework;
using Project1.Core.Animations;
using Project1.Core.Assets;
using Project1.Core.Components;
using Project1.Core.Entities.Stats;
using Project1.Core.Gear;
using Project1.Core.Graphics;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Legacy.Properties;
using Project1.Core.Legacy.Storage;
using Project1.Core.Resources;
using Project1.Core.Skills;
using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Plants;
using Project1.Core.Systems.Tools;
using Project1.Core.Towns.Duties;
using Project1.Framework;
using System.Linq;

namespace Project1.Core.Entities
{
    [EnsureStaticCtorCall]
    static class ItemDefOf
    {
        static public readonly ItemDef Ingredient = new ItemDef("Ingredient", typeof(Entity))
        {
            StackCapacity = 5,
            BaseValue = 5,
            Description = "Used as an input for crafting final products",
            Category = ItemCategoryDefOf.RawMaterials,
            Body = new Bone(BoneDefOf.Item, Sprite.Default) { DrawMaterialColor = true },
            ProfileType = typeof(MaterialRefinementDef)
        };
        static public readonly ItemDef UnfinishedItem = new ItemDef("UnfinishedItem", typeof(Entity))
        {
            BaseValue = 5,
            Description = "An unfinished crafting item",
            Category = ItemCategoryDefOf.Manufactured,
            CompDefs = [EntityCompDefOf.UnfinishedItem, EntityCompDefOf.Resources],
            Body = new Bone(BoneDefOf.Item, Sprite.Default) { DrawMaterialColor = true },
        }.AddSpec(new ResourcesComp.Spec([ResourceDefOf.Assembly]));

        static public readonly ItemDef Seeds = new ItemDef("Seeds", typeof(Entity))
        {
            StackCapacity = 32,//64,
            Category = ItemCategoryDefOf.RawMaterials,
            DefaultMaterial = MaterialDefOf.Seed,
            Comps = [typeof(SeedComponent)],
            CompDefs = [EntityCompDefOf.Seed],
            Body = new Bone(BoneDefOf.Item, ItemContent.SeedsFull),
            ProfileType = typeof(PlantSpeciesDef)
        };
        static public readonly ItemDef Fruit = new ItemDef("Fruit", typeof(Entity))
        {
            StackCapacity = 32,
            Category = ItemCategoryDefOf.FoodRaw,
            ReplaceName = true,
            Comps = [typeof(ConsumableComp)],
            CompDefs = [EntityCompDefOf.Consumable],
            Body = new Bone(BoneDefOf.Item, ItemContent.BerriesFull),
            ProfileType = typeof(PlantSpeciesDef)
        };
        static public readonly ItemDef Consumable = new ItemDef("Consumable", typeof(Entity))
        {
            StackCapacity = 32,
            ReplaceName = true,
            Comps = [typeof(ConsumableComp)],
            CompDefs = [EntityCompDefOf.Consumable],
            Body = new Bone(BoneDefOf.Item, Sprite.Default),
            //DefaultSprite = Sprite.Default
        };

        static public readonly ItemDef Meat = new ItemDef("Meat", typeof(Entity))
        {
            StackCapacity = 8,
            Category = ItemCategoryDefOf.FoodRaw,
            DefaultMaterialType = MaterialTypeDefOf.Flesh,
            Comps = [typeof(ConsumableComp)],
            CompDefs = [EntityCompDefOf.Consumable],
            Body = new Bone(BoneDefOf.Item, Sprite.Default),
            ProfileType = typeof(MaterialRefinementDef)
        };


        static public readonly ItemDef Pie = new ItemDef("Pie", typeof(Entity))
        {
            StackCapacity = 4,
            Category = ItemCategoryDefOf.FoodCooked,
            CraftingProperties = new CraftingProperties().MakeableFrom(ItemCategoryDefOf.FoodRaw),
            Body = new Bone(BoneDefOf.Item, Sprite.Default),
            RecipeProperties =
                new RecipeProperties("Bake") { Job = DutyDefOf.Cook, Skill = SkillDefOf.Cooking }
                    .AddWorkstation(IsWorkstation.Types.Baking)
                    .AddIngredientMaker(def =>
                        new Ingredient("Filling") { DefaultRestrictions = new IngredientRestrictions().Restrict(MaterialTypeDefOf.Flesh) }
                            .SetAllow(def.ValidMaterialTypes, true)
                            .SetAllowed(ItemCategoryDefOf.FoodRaw, true))
                    .AddProductMaker(def => new Reaction.Product(def).GetMaterialFromIngredient("Filling")),
            Comps = [typeof(ConsumableComp)],
            CompDefs = [EntityCompDefOf.Consumable],

        }.SetMadeFrom(MaterialTypeDefOf.Fruit, MaterialTypeDefOf.Flesh)
            .AddSpec(new ConsumableComp.Spec());// { FoodClasses = [FoodClass.Dish] });

        static public readonly ItemDef Coins = new("Coins", typeof(Entity))
        {
            StackCapacity = ushort.MaxValue,
            Category = ItemCategoryDefOf.RawMaterials,
            DefaultMaterial = MaterialDefOf.Gold,
            BaseValue = 1,
            Weight = .01f,
            Body = new Bone(BoneDefOf.Item, ItemContent.BarsGrayscale)
        };

        static public readonly ItemDef Helmet = new ItemDef("ItemHelmet", typeof(Entity))
        {
            BaseValue = 5,
            QualityLevels = true,
            Category = ItemCategoryDefOf.Wearables,
            Description = "Protects the head but ruins the hairstyle.",
            DefaultSprite = ItemContent.HelmetFull,
            MadeFromMaterials = true,
            GearType = GearTypeDefOf.Head,
            ApparelProperties = new ApparelDef(GearTypeDefOf.Head, 10),
            DefaultMaterial = MaterialDefOf.Iron,
            Comps = [typeof(OwnershipComponent)],
            CompDefs = [EntityCompDefOf.Ownership],
            Body = new Bone(BoneDefOf.Item, ItemContent.HelmetFull)
        };

        static public readonly ItemDef Tool = new ItemDef("Tool", typeof(Entity))
        {
            QualityLevels = true,
            Category = ItemCategoryDefOf.Equipment,
            MadeFromMaterials = true,
            GearType = GearTypeDefOf.Mainhand,
            DefaultMaterial = MaterialDefOf.Iron,
            CraftingProperties = CraftingProperties.ToolCraftingProperties,
            NameGetter = e => e.Def.LabelReadable,
            VariantType = typeof(ToolProfileDef),
            StorageFilterVariations = Def.Get<ToolProfileDef>(),
            VariationGetter = e => e.Def,
            Comps = [typeof(ToolComp), typeof(OwnershipComponent), typeof(ResourcesComp), typeof(StatsComponent)],
            CompDefs = [EntityCompDefOf.Tool, EntityCompDefOf.Ownership, EntityCompDefOf.Resources, EntityCompDefOf.Stats],
            Body = new Bone(BoneDefOf.ToolHandle, ItemContent.LogsGrayscale, Vector2.Zero, 0.001f) { DrawMaterialColor = true, OriginGroundOffset = new Vector2(0, -16) }
                            .AddJoint(Vector2.Zero, new Bone(BoneDefOf.ToolHead, ItemContent.LogsGrayscale) { DrawMaterialColor = true })
        }
            .AddSpec(new ResourcesComp.Spec([ResourceDefOf.Durability]));
        static ItemDefOf()
        {
            Def.Register(typeof(ItemDefOf));
            GenerateCookingRecipes();
        }
        private static void GenerateCookingRecipes()
        {
            var cookables = Def.Get<ItemDef>().Where(d => d.RecipeProperties != null).ToList();
            foreach (var def in cookables)
                Def.Register(def.CreateRecipe());
        }
    }
}

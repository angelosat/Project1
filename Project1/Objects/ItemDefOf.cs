using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.Components;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class ItemDefOf
    {
        static public readonly ItemDef Seeds = new ItemDef("Seeds", typeof(Item))
        {
            StackCapacity = 32,//64,
            Category = ItemCategoryDefOf.RawMaterials,
            //Body = new Bone(BoneDefOf.Item, ItemContent.SeedsFull),
            DefaultMaterial = MaterialDefOf.Seed,
            //CompProps = [new SeedComponent.Props()]
        }
            .AddProp(new SpriteComp.Props(new Bone(BoneDefOf.Item, ItemContent.SeedsFull)))
            .AddProp(new SeedComponent.Props());

        static public readonly ItemDef Fruit = new ItemDef("Fruit", typeof(Item))
        {
            StackCapacity = 32,
            Category = ItemCategoryDefOf.FoodRaw,
            //Body = new Bone(BoneDefOf.Item, ItemContent.BerriesFull),
            ReplaceName = true,
            //CompProps =
            //[
            //    new ConsumableComponent.Props() {
            //        FoodClasses = [FoodClass.Fruit],
            //        Effects = [new NeedEffect(NeedDefOf.Hunger, 50)]}
            //],
        }
            .SetMadeFrom(MaterialTypeDefOf.Fruit)
            .AddProp(new ConsumableComponent.Props()
            {
                FoodClasses = [FoodClass.Fruit],
                Effects = [new NeedEffect(NeedDefOf.Hunger, 50)]
            })
            .AddProp(new SpriteComp.Props(new Bone(BoneDefOf.Item, ItemContent.BerriesFull)));

        static public readonly ItemDef Meat = new ItemDef("Meat", typeof(Item))
        {
            StackCapacity = 8,
            Category = ItemCategoryDefOf.FoodRaw,
            //Body = new Bone(BoneDefOf.Item, Sprite.Default),
            DefaultMaterialType = MaterialTypeDefOf.Meat,
            //ConsumableProperties = new ConsumableProperties(),
            //CompProps =
            //[
            //    new ConsumableComponent.Props() {
            //        Effects = [new NeedEffect(NeedDefOf.Hunger, 50)] }],
        }.SetMadeFrom(MaterialTypeDefOf.Meat)
            .AddProp(new SpriteComp.Props(new Bone(BoneDefOf.Item, Sprite.Default)))
            .AddProp(new ConsumableComponent.Props()
            {
                Effects = [new NeedEffect(NeedDefOf.Hunger, 50)]
            });


        static public readonly ItemDef Pie = new ItemDef("Pie", typeof(Item))
        {
            StackCapacity = 4,
            Category = ItemCategoryDefOf.FoodCooked,
            //Body = new Bone(BoneDefOf.Item, Sprite.Default),
            //ConsumableProperties = new()
            //{
            //    FoodClasses = new[] { FoodClass.Dish }
            //},
            CraftingProperties = new CraftingProperties().MakeableFrom(ItemCategoryDefOf.FoodRaw),
            RecipeProperties =
                new RecipeProperties("Bake") { Job = JobDefOf.Cook, Skill = SkillDefOf.Cooking }
                    .AddWorkstation(IsWorkstation.Types.Baking)
                    .AddIngredientMaker(def =>
                        new Ingredient("Filling") { DefaultRestrictions = new IngredientRestrictions().Restrict(MaterialTypeDefOf.Meat) }
                            .SetAllow(def.ValidMaterialTypes, true)
                            .SetAllowed(ItemCategoryDefOf.FoodRaw, true))
                    .AddProductMaker(def => new Reaction.Product(def).GetMaterialFromIngredient("Filling")),
            //CompProps =
            //[
            //    new ConsumableComponent.Props() {FoodClasses= [FoodClass.Dish]}
            //],
        }.SetMadeFrom(MaterialTypeDefOf.Fruit, MaterialTypeDefOf.Meat)
            .AddProp(new SpriteComp.Props(new Bone(BoneDefOf.Item, Sprite.Default)))
            .AddProp(new ConsumableComponent.Props() { FoodClasses = [FoodClass.Dish] });


        static public readonly ItemDef UnfinishedCraft = new ItemDef("UnfinishedCraft", typeof(Item))
        {
            Category = ItemCategoryDefOf.Unfinished,
            //Body = new Bone(BoneDefOf.Item, Sprite.Default),
            //CompProps = [new UnfinishedItemComp.Props()]
        }
        .AddProp(new SpriteComp.Props(new Bone(BoneDefOf.Item, Sprite.Default)))
        .AddProp(new UnfinishedItemComp.Props());
            

        static public readonly ItemDef Coins = new ItemDef("Coins", typeof(Item))
        {
            StackCapacity = ushort.MaxValue,
            //Body = new Bone(BoneDefOf.Item, ItemContent.BarsGrayscale),
            Category = ItemCategoryDefOf.RawMaterials,
            DefaultMaterial = MaterialDefOf.Gold,
            BaseValue = 1,
        }
        .AddProp(new SpriteComp.Props(new Bone(BoneDefOf.Item, ItemContent.BarsGrayscale)));

        static public readonly ItemDef Helmet = new ItemDef("ItemHelmet", typeof(Item))
        {
            BaseValue = 5,
            QualityLevels = true,
            Category = ItemCategoryDefOf.Wearables,
            Description = "Protects the head but ruins the hairstyle.",
            DefaultSprite = ItemContent.HelmetFull,
            MadeFromMaterials = true,
            GearType = GearType.Head,
            ApparelProperties = new ApparelDef(GearType.Head, 10),
            DefaultMaterial = MaterialDefOf.Iron,
            //Body = new Bone(BoneDefOf.Item, ItemContent.HelmetFull),
            //CompProps = [new OwnershipComponent.Props()] /*new List<ComponentProps>() { new ComponentProps() { CompClass = typeof(OwnershipComponent) } }*/
        }
        .AddProp(new SpriteComp.Props(new Bone(BoneDefOf.Item, ItemContent.HelmetFull)))
        .AddProp(new OwnershipComponent.Props());

        //public static readonly CraftingProperties ToolCraftingProperties = new()
        //{
        //    Reagents = new Dictionary<BoneDef, Reaction.Reagent>()
        //        {
        //            { BoneDefOf.ToolHandle, new Reaction.Reagent("Handle", new Ingredient(null, null, null).SetAllowed(ItemCategoryDefOf.Manufactured, true)) }, //.IsBuildingMaterial()
        //            { BoneDefOf.ToolHead, new Reaction.Reagent("Head", new Ingredient(null, null, null).SetAllowed(ItemCategoryDefOf.Manufactured, true))  }
        //        }
        //};

        static public readonly ItemDef Tool = new ItemDef("Tool", typeof(Tool))
        {
            QualityLevels = true,
            Category = ItemCategoryDefOf.Equipment,
            MadeFromMaterials = true,
            GearType = GearType.Mainhand,
            DefaultMaterial = MaterialDefOf.Iron,
            Factory = d => d.CreateNew(),
            CraftingProperties = CraftingProperties.ToolCraftingProperties,
            //Body = new Bone(BoneDefOf.ToolHandle, ItemContent.LogsGrayscale, Vector2.Zero, 0.001f) { DrawMaterialColor = true, OriginGroundOffset = new Vector2(0, -16) }
                            //.AddJoint(Vector2.Zero, new Bone(BoneDefOf.ToolHead, ItemContent.LogsGrayscale) { DrawMaterialColor = true }),
            NameGetter = e => e.ToolComponent.ToolProperties.Label,
            StorageFilterVariations = Def.GetDefs<ToolProps>(),
            VariationGetter = e => e.ToolComponent.ToolProperties,
            //CompProps = [new ToolAbilityComponent.Props()]
        }
        .AddProp(new SpriteComp.Props(new Bone(BoneDefOf.ToolHandle, ItemContent.LogsGrayscale, Vector2.Zero, 0.001f) { DrawMaterialColor = true, OriginGroundOffset = new Vector2(0, -16) }
                            .AddJoint(Vector2.Zero, new Bone(BoneDefOf.ToolHead, ItemContent.LogsGrayscale) { DrawMaterialColor = true })))
        .AddProp(new ToolAbilityComponent.Props());
        static ItemDefOf()
        {
            Def.Register(typeof(ItemDefOf));

            GameObject.AddTemplate(ItemFactory.CreateItem(ItemDefOf.Coins).SetStackSize(100));
            GameObject.AddTemplates(Fruit.CreateFromAllMAterials());
            GameObject.AddTemplates(Meat.CreateFromAllMAterials());
            GameObject.AddTemplates(Pie.CreateFromAllMAterials());

            GenerateCookingRecipes();

            //Reaction.Register(new Reaction("Extract Seeds", SkillDefOf.Argiculture)
            //    .AddBuildSite(IsWorkstation.Types.PlantProcessing)
            //    .AddIngredient("a", new Ingredient()
            //        .SetAllow(ItemDefOf.Fruit, true))
            //    .AddProduct(new Reaction.Product(ItemDefOf.Seeds, 4)
            //        .GetMaterialFromIngredient("a"))
            //    ); 
        }

        private static void GenerateCookingRecipes()
        {
            var cookables = Def.GetDefs<ItemDef>().Where(d => d.RecipeProperties != null).ToList();
            foreach (var def in cookables)
                Def.Register(def.CreateRecipe());
        }
    }
}

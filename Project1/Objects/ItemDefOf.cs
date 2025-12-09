using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    static class ItemDefOf
    {
        static public readonly ItemDef Seeds = new("Seeds", typeof(Item))
        {
            StackCapacity = 32,//64,
            Category = ItemCategoryDefOf.RawMaterials,
            Body = new Bone(BoneDefOf.Item, ItemContent.SeedsFull),
            DefaultMaterial = MaterialDefOf.Seed,
            CompProps = new List<ComponentProps>() { new SeedComponent.Props() }
        };

        static public readonly ItemDef Fruit = new ItemDef("Fruit", typeof(Item))
        {
            StackCapacity = 32,
            Category = ItemCategoryDefOf.FoodRaw,
            Body = new Bone(BoneDefOf.Item, ItemContent.BerriesFull),
            ReplaceName = true,
            ConsumableProperties = new ConsumableProperties()
            {
                FoodClasses = new[] { FoodClass.Fruit }
            },
            CompProps = new List<ComponentProps>()
            {
                new ConsumableComponent.Props(new[] { new NeedEffect(NeedDefOf.Hunger, 50) })
            },
        }.SetMadeFrom(MaterialTypeDefOf.Fruit);

        static public readonly ItemDef Meat = new ItemDef("Meat", typeof(Item))
        {
            StackCapacity = 8,
            Category = ItemCategoryDefOf.FoodRaw,
            Body = new Bone(BoneDefOf.Item, Sprite.Default),
            DefaultMaterialType = MaterialTypeDefOf.Meat,
            ConsumableProperties = new ConsumableProperties(),
            CompProps = new List<ComponentProps>()
            {
                new ConsumableComponent.Props(new[] { new NeedEffect(NeedDefOf.Hunger, 50) })
            },
        }.SetMadeFrom(MaterialTypeDefOf.Meat);

        static public readonly ItemDef Pie = new ItemDef("Pie", typeof(Item))
        {
            StackCapacity = 4,
            Category = ItemCategoryDefOf.FoodCooked,
            Body = new Bone(BoneDefOf.Item, Sprite.Default),
            ConsumableProperties = new()
            {
                FoodClasses = new[] { FoodClass.Dish }
            },
            CraftingProperties = new CraftingProperties().MakeableFrom(ItemCategoryDefOf.FoodRaw),
            RecipeProperties =
                new RecipeProperties("Bake") { Job = JobDefOf.Cook, Skill = SkillDefOf.Cooking }
                    .AddWorkstation(IsWorkstation.Types.Baking)
                    .AddIngredientMaker(def =>
                        new Ingredient("Filling") { DefaultRestrictions = new IngredientRestrictions().Restrict(MaterialTypeDefOf.Meat) }
                            .SetAllow(def.ValidMaterialTypes, true)
                            .SetAllow(ItemCategoryDefOf.FoodRaw, true))
                    .AddProductMaker(def => new Reaction.Product(def).GetMaterialFromIngredient("Filling")),
            CompProps = new List<ComponentProps>()
            {
                new ConsumableComponent.Props()
            },
        }.SetMadeFrom(MaterialTypeDefOf.Fruit, MaterialTypeDefOf.Meat);

        static public readonly ItemDef UnfinishedCraft = new ItemDef("UnfinishedCraft", typeof(Item))
        {
            Category = ItemCategoryDefOf.Unfinished,
            Body = new Bone(BoneDefOf.Item, Sprite.Default),
            CompProps = new() { 
                new ComponentProps(typeof(UnfinishedItemComp))
            },
        };

        static public readonly ItemDef Coins = new("Coins", typeof(Item))
        {
            StackCapacity = ushort.MaxValue,
            Body = new Bone(BoneDefOf.Item, ItemContent.BarsGrayscale),
            Category = ItemCategoryDefOf.RawMaterials,
            DefaultMaterial = MaterialDefOf.Gold,
            BaseValue = 1,
        };

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

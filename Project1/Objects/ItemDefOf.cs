using System.Linq;
using Start_a_Town_.Components;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
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
        };

        static public readonly ItemDef Seeds = new ItemDef("Seeds", typeof(Entity))
        {
            StackCapacity = 32,//64,
            Category = ItemCategoryDefOf.RawMaterials,
            DefaultMaterial = MaterialDefOf.Seed,
            CompTypes = [typeof(SeedComponent)],
            Body = new Bone(BoneDefOf.Item, ItemContent.SeedsFull)
        }
              //.AddSpec(new SpriteComp.Spec(new Bone(BoneDefOf.Item, ItemContent.SeedsFull)))
              //.AddSpec(new SeedComponent.Props());
              ;
        static public readonly ItemDef Fruit = new ItemDef("Fruit", typeof(Entity))
        {
            StackCapacity = 32,
            Category = ItemCategoryDefOf.FoodRaw,
            ReplaceName = true,
            CompTypes = [typeof(ConsumableComponent)],
            Body = new Bone(BoneDefOf.Item, ItemContent.BerriesFull)
        }
            //.SetMadeFrom(MaterialTypeDefOf.Fruit)
            //.AddSpec(new ConsumableComponent.Spec()
            //{
            //    FoodClasses = [FoodClass.Fruit],
            //    Effects = [new NeedEffect(NeedDefOf.Hunger, 50)]
            //})
            ;

        static public readonly ItemDef Meat = new ItemDef("Meat", typeof(Entity))
        {
            StackCapacity = 8,
            Category = ItemCategoryDefOf.FoodRaw,
            DefaultMaterialType = MaterialTypeDefOf.Flesh,
            CompTypes = [typeof(ConsumableComponent)],
            Body = new Bone(BoneDefOf.Item, Sprite.Default)
        }
        //.SetMadeFrom(MaterialTypeDefOf.Flesh)
            //.AddSpec(new ConsumableComponent.Spec()
            //{
            //    Effects = [new NeedEffect(NeedDefOf.Hunger, 50)]
            //})
            ;


        static public readonly ItemDef Pie = new ItemDef("Pie", typeof(Entity))
        {
            StackCapacity = 4,
            Category = ItemCategoryDefOf.FoodCooked,
            CraftingProperties = new CraftingProperties().MakeableFrom(ItemCategoryDefOf.FoodRaw),
            Body = new Bone(BoneDefOf.Item, Sprite.Default),
            RecipeProperties =
                new RecipeProperties("Bake") { Job = JobDefOf.Cook, Skill = SkillDefOf.Cooking }
                    .AddWorkstation(IsWorkstation.Types.Baking)
                    .AddIngredientMaker(def =>
                        new Ingredient("Filling") { DefaultRestrictions = new IngredientRestrictions().Restrict(MaterialTypeDefOf.Flesh) }
                            .SetAllow(def.ValidMaterialTypes, true)
                            .SetAllowed(ItemCategoryDefOf.FoodRaw, true))
                    .AddProductMaker(def => new Reaction.Product(def).GetMaterialFromIngredient("Filling")),
            CompTypes = [typeof(ConsumableComponent)]
        }.SetMadeFrom(MaterialTypeDefOf.Fruit, MaterialTypeDefOf.Flesh)
            //.AddSpec(new SpriteComp.Spec(new Bone(BoneDefOf.Item, Sprite.Default)))
            .AddSpec(new ConsumableComponent.Spec() { FoodClasses = [FoodClass.Dish] });


        static public readonly ItemDef UnfinishedCraft = new ItemDef("UnfinishedCraft", typeof(Entity))
        {
            Category = ItemCategoryDefOf.Unfinished,
            CompTypes = [typeof(UnfinishedItemComp)],
            Body = new Bone(BoneDefOf.Item, Sprite.Default)
        };
        //.AddSpec(new SpriteComp.Spec(new Bone(BoneDefOf.Item, Sprite.Default)));
        //.AddSpec(new UnfinishedItemComp.Props());


        static public readonly ItemDef Coins = new("Coins", typeof(Entity))
        {
            StackCapacity = ushort.MaxValue,
            Category = ItemCategoryDefOf.RawMaterials,
            DefaultMaterial = MaterialDefOf.Gold,
            BaseValue = 1,
            Weight = .01f,
            Body = new Bone(BoneDefOf.Item, ItemContent.BarsGrayscale)
        };
        //.AddSpec(new SpriteComp.Spec(new Bone(BoneDefOf.Item, ItemContent.BarsGrayscale)));

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
            CompTypes = [typeof(OwnershipComponent)],
            Body = new Bone(BoneDefOf.Item, ItemContent.HelmetFull)
        };
        //.AddSpec(new SpriteComp.Spec(new Bone(BoneDefOf.Item, ItemContent.HelmetFull)));
        //.AddSpec(new OwnershipComponent.Props());

        static public readonly ItemDef Tool = new ItemDef("Tool", typeof(Entity))
        {
            QualityLevels = true,
            Category = ItemCategoryDefOf.Equipment,
            MadeFromMaterials = true,
            GearType = GearTypeDefOf.Mainhand,
            DefaultMaterial = MaterialDefOf.Iron,
            //Factory = d => d.CreateBase(),
            CraftingProperties = CraftingProperties.ToolCraftingProperties,
            NameGetter = e => e.Def.Label,
            VariantType = typeof(ToolProfileDef),
            //StorageFilterVariations = Def.GetDefs<ToolProps>(),
            StorageFilterVariations = Def.GetDefs<ToolProfileDef>(),
            VariationGetter = e => e.Def,
            CompTypes = [typeof(ToolComp), typeof(OwnershipComponent), typeof(ResourcesComponent), typeof(StatsComponent)],
            Body = new Bone(BoneDefOf.ToolHandle, ItemContent.LogsGrayscale, Vector2.Zero, 0.001f) { DrawMaterialColor = true, OriginGroundOffset = new Vector2(0, -16) }
                            .AddJoint(Vector2.Zero, new Bone(BoneDefOf.ToolHead, ItemContent.LogsGrayscale) { DrawMaterialColor = true })
        }
            //.AddSpec(new SpriteComp.Spec(new Bone(BoneDefOf.ToolHandle, ItemContent.LogsGrayscale, Vector2.Zero, 0.001f) { DrawMaterialColor = true, OriginGroundOffset = new Vector2(0, -16) }
            //                .AddJoint(Vector2.Zero, new Bone(BoneDefOf.ToolHead, ItemContent.LogsGrayscale) { DrawMaterialColor = true })))
            .AddSpec(new ResourcesComponent.Spec([ResourceDefOf.Durability]));
        static ItemDefOf()
        {
            Def.Register(typeof(ItemDefOf));

            
            //GameObject.AddTemplates(Fruit.CreateFromAllMAterials());
            //GameObject.AddTemplates(Meat.CreateFromAllMAterials());
            //GameObject.AddTemplates(Pie.CreateFromAllMAterials());

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

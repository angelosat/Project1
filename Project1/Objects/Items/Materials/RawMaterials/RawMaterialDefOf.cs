using Start_a_Town_.Components;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    public class RawMaterialDefOf
    {
        static public readonly ItemDef Planks = new ItemDef("Plank", typeof(Item))
        {
            BaseValue = 5,
            Description = "Processed logs",
            StackCapacity = 24,
            Weight = .1f,
            Category = ItemCategoryDefOf.Manufactured,
            //Body = new Bone(BoneDefOf.Item, ItemContent.PlanksGrayscale) { DrawMaterialColor = true },
            CanProcessInto = new List<Reaction.Product.Types>() { Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks, Reaction.Product.Types.Workbenches },
            DefaultMaterialType = MaterialTypeDefOf.Wood,
            CraftingProperties = new CraftingProperties()
            {
                IsBuildingMaterial = true,
                IsCraftingMaterial = true
            }
        }.SetMadeFrom(MaterialTypeDefOf.Wood)
            .AddSpec(new SpriteComp.Spec(new Bone(BoneDefOf.Item, ItemContent.PlanksGrayscale) { DrawMaterialColor = true }));

        static public readonly ItemDef Logs = new ItemDef("Logs", typeof(Item))
        {
            BaseValue = 1,
            Description = "It came from a tree",
            StackCapacity = 6,
            //Body = new Bone(BoneDefOf.Item, ItemContent.LogsGrayscale) { DrawMaterialColor = true },
            Category = ItemCategoryDefOf.RawMaterials,
            CanProcessInto = new List<Reaction.Product.Types>() { Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks, Reaction.Product.Types.Workbenches },
            CraftingProperties = new CraftingProperties() { IsBuildingMaterial = true },
            DefaultMaterialType = MaterialTypeDefOf.Wood
        }.SetMadeFrom(MaterialTypeDefOf.Wood)
            .AddSpec(new SpriteComp.Spec(new Bone(BoneDefOf.Item, ItemContent.LogsGrayscale) { DrawMaterialColor = true }));


        static public readonly ItemDef Bags = new ItemDef("Bag", typeof(Item))
        {
            BaseValue = 1,
            Description = "A bag containing grainy material",
            StackCapacity = 10,
            Category = ItemCategoryDefOf.RawMaterials,
            //Body = new Bone(BoneDefOf.Item, ItemContent.BagsGrayscale) { DrawMaterialColor = true },
            CanProcessInto = new List<Reaction.Product.Types>() { Reaction.Product.Types.Blocks },
            DefaultMaterialType = MaterialTypeDefOf.Soil,
        }.SetMadeFrom(MaterialTypeDefOf.Soil)
            .AddSpec(new SpriteComp.Spec(new Bone(BoneDefOf.Item, ItemContent.BagsGrayscale) { DrawMaterialColor = true }));


        static public readonly ItemDef Ingots = new ItemDef("Ingot", typeof(Item))
        {
            BaseValue = 5,
            Description = "Used for crafting of weapons, armor, and tools.",
            StackCapacity = 20,
            Category = ItemCategoryDefOf.Manufactured,
            //Body = new Bone(BoneDefOf.Item, ItemContent.BarsGrayscale) { DrawMaterialColor = true },
            CanProcessInto = new List<Reaction.Product.Types>() { Reaction.Product.Types.Tools },
            DefaultMaterialType = MaterialTypeDefOf.Metal,
            CraftingProperties = new CraftingProperties() { IsCraftingMaterial = true, IsBuildingMaterial = true },
        }.SetMadeFrom(MaterialTypeDefOf.Metal)
            .AddSpec(new SpriteComp.Spec(new Bone(BoneDefOf.Item, ItemContent.BarsGrayscale) { DrawMaterialColor = true }));


        static public readonly ItemDef Ore = new ItemDef("Ore", typeof(Item))
        {
            BaseValue = 1,
            Description = "A piece of mineral ore",
            StackCapacity = 10,
            Category = ItemCategoryDefOf.RawMaterials,
            //Body = new Bone(BoneDefOf.Item, ItemContent.OreGrayscale) { DrawMaterialColor = true },
            CanProcessInto = new List<Reaction.Product.Types>() { Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks },
            DefaultMaterialType = MaterialTypeDefOf.Metal,
        }.SetMadeFrom(MaterialToken.Metal)
            .AddSpec(new SpriteComp.Spec(new Bone(BoneDefOf.Item, ItemContent.OreGrayscale) { DrawMaterialColor = true }));


        static public readonly ItemDef Boulders = new ItemDef("Boulders", typeof(Item))
        {
            BaseValue = 1,
            Description = "Chunks of rock",
            StackCapacity = 10,
            Category = ItemCategoryDefOf.RawMaterials,
            //Body = new Bone(BoneDefOf.Item, ItemContent.OreGrayscale) { DrawMaterialColor = true },
            CanProcessInto = new List<Reaction.Product.Types>() { Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks },
            DefaultMaterialType = MaterialTypeDefOf.Stone,
            CraftingProperties = new CraftingProperties() { IsBuildingMaterial = true, IsCraftingMaterial = true },
        }.SetMadeFrom(MaterialTypeDefOf.Stone)
            .AddSpec(new SpriteComp.Spec(new Bone(BoneDefOf.Item, ItemContent.OreGrayscale) { DrawMaterialColor = true }));
            

        static public readonly ItemDef Scraps = new ItemDef("Scraps", typeof(Item))
        {
            StackDimension = 4,
            StackCapacity = 50,
            BaseValue = 0,
            Description = "Worthless but can be repurposed",
            Category = ItemCategoryDefOf.RawMaterials,
            //Body = new Bone(BoneDefOf.Item, Sprite.Default) { DrawMaterialColor = true },
        }.SetMadeFrom(MaterialTypeDefOf.Wood, MaterialTypeDefOf.Stone, MaterialTypeDefOf.Metal)
            .AddSpec(new SpriteComp.Spec(new Bone(BoneDefOf.Item, Sprite.Default) { DrawMaterialColor = true }));




        static void InitRecipes()
        {
            Reaction.Register(new Reaction(
                "Saw logs", SkillDefOf.Carpentry, JobDefOf.Carpenter)
                .AddBuildSite(IsWorkstation.Types.Carpentry)
                .AddIngredient(new Ingredient("Base")
                    .SetAllow(MaterialTypeDefOf.Wood, true)
                    .SetAllow(Logs, true)
                    .SetAllow(Scraps, true))
                .AddProduct(new Reaction.Product(Planks).GetMaterialFromIngredient("Base"))
                .GetWorkRequiredFromMaterial("Base", (work, obj) => work * obj.PrimaryMaterial.Density));
        }

        static RawMaterialDefOf()
        {
            Def.Register(typeof(RawMaterialDefOf));
            var smelting = new Reaction("Smelt", SkillDefOf.Crafting, JobDefOf.Smelter)
                .AddBuildSite(IsWorkstation.Types.Smeltery)
                .AddIngredient(new Ingredient("a")
                    .SetAllow(RawMaterialDefOf.Scraps, true)
                    .SetAllow(RawMaterialDefOf.Ore, true)
                    .SetAllow(MaterialTypeDefOf.Metal, true))
                .AddProduct(new Reaction.Product(RawMaterialDefOf.Ingots).GetMaterialFromIngredient("a")
                );
            Def.Register(smelting);

            Generate();
        }
        static void Generate()
        {
            var defs = Def.Database.Values.OfType<ItemDef>().ToList();
            foreach (var def in defs.Where(d => d.DefaultMaterialType != null))
            {
                foreach (var m in def.DefaultMaterialType.SubTypes)
                    GameObject.AddTemplate(def.CreateFrom(m));//  CreateFrom(def, m));
            }

            foreach (var mt in new[] { MaterialTypeDefOf.Metal, MaterialTypeDefOf.Wood, MaterialTypeDefOf.Stone }) // TODO make scraps from solid rawmaterials
                foreach (var m in mt.SubTypes)
                    GameObject.AddTemplate(Scraps.CreateFrom(m));// CreateFrom(Scraps, m));

            InitRecipes();
        }
    }
}

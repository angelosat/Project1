using Start_a_Town_.Components;
using System.Linq;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    public static class PlantDefOf
    {
        static public ItemDef Tree = new ItemDef("Tree", typeof(Plant))
        {
            Description = "A lovely tree",
            Height = 4,
            Weight = 100,
            Haulable = false,
            DefaultMaterial = MaterialDefOf.LightWood,
            //Body = new Bone(BoneDefOf.TreeTrunk, ItemContent.TreeFull),
            Body = new Bone(BoneDefOf.TreeTrunk, ItemContent.TreeFull).AddJoint(new Bone(BoneDefOf.PlantFruit) { DrawMaterialColor = true }),
            Size = ObjectSize.Haulable,
            CompTypes = [typeof(PlantComponent), typeof(ResourcesComponent)]
        }
        //.AddSpec(new SpriteComp.Spec(new Bone(BoneDefOf.TreeTrunk, ItemContent.TreeFull)))
        .AddSpec(new ResourcesComponent.Spec([ResourceDefOf.HitPoints]))
        //.AddSpec(new PlantComponent.Spec())

        ;

        static public ItemDef Bush = new ItemDef("Bush", typeof(Plant))
        {
            Description = "A lovely fluffy bush.",
            Height = 1,
            Weight = 5,
            Haulable = false,
            DefaultMaterial = MaterialDefOf.ShrubStem,
            Body = new Bone(BoneDefOf.PlantStem, ItemContent.BerryBushGrowing).AddJoint(new Bone(BoneDefOf.PlantFruit) { DrawMaterialColor = true }),
            Size = ObjectSize.Haulable,
            CompTypes = [typeof(PlantComponent), typeof(ResourcesComponent)]
        }
        .AddSpec(new ResourcesComponent.Spec([ResourceDefOf.HitPoints]))
        //.AddSpec(new PlantComponent.Spec())
        //.AddSpec(new SpriteComp.Spec(new Bone(BoneDefOf.PlantStem, ItemContent.BerryBushGrowing).AddJoint(new Bone(BoneDefOf.PlantFruit) { DrawMaterialColor = true })))
            ;

        static PlantDefOf()
        {
            Def.Register(Tree);
            Def.Register(Bush);

            var bush = PlantSpiecesDefOf.Berry.Create(PlantStageDefOf.Plant);
            var plantComp = bush.GetComponent<PlantComponent>();
            plantComp.GrowthBody.Percentage = 1;
            plantComp.GrowthFruit.Percentage = 1;
            GameObject.AddTemplate(bush);

            var tree = PlantSpiecesDefOf.LightTree.Create(PlantStageDefOf.Plant);
            tree.GetComponent<PlantComponent>().GrowthBody.Percentage = 1;
            GameObject.AddTemplate(tree);

            var allPlants = Def.GetDefs<PlantSpeciesDef>();
            GameObject.AddTemplates(allPlants.Select(p => p.Create(PlantStageDefOf.Seed)));

            Def.Register(new Reaction("Extract Seeds", SkillDefOf.Argiculture)
                .AddBuildSite(IsWorkstation.Types.PlantProcessing)
                .AddIngredient("a", new Ingredient()
                    .SetAllow(ItemDefOf.Fruit, true))
                .AddProduct(new Reaction.Product(i => Def.GetDefs<PlantSpeciesDef>().First(d => d.FruitMaterial == i["a"].PrimaryMaterial).Create(PlantStageDefOf.Seed) as Entity, 4))
                );
        }
    }
}

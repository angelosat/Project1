using Project1.Core.Animations;
using Project1.Core.Assets;
using Project1.Core.Entities;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Resources;
using Project1.Core.Simulation.Physics;
using Project1.Core.Skills;
using Project1.Core.Systems.Gear;
using Project1.Core.Systems.Materials;
using Project1.Framework;
using System.Linq;

namespace Project1.Core.Systems.Plants
{
    [EnsureStaticCtorCall]
    public static class PlantDefOf
    {
        static public ItemDef Tree = new ItemDef("Tree", typeof(Entity))
        {
            Description = "A lovely tree",
            Height = 4,
            Weight = 100,
            IsHaulable = false,
            DefaultMaterial = MaterialDefOf.LightWood,
            Body = new Bone(BoneDefOf.TreeTrunk, ItemContent.TreeFull).AddJoint(new Bone(BoneDefOf.PlantFruit) { DrawMaterialColor = true }),
            DefaultBoneStruct = BoneStructureDefOf.Tree,
            Size = ObjectSize.Haulable,
            Comps = [typeof(PlantComponent), typeof(ResourcesComp)],
            CompDefs = [EntityCompDefOf.Plant, EntityCompDefOf.Resources]
        }
        .AddSpec(new ResourcesComp.Spec([ResourceDefOf.HitPoints]))
        ;

        static public ItemDef Bush = new ItemDef("Bush", typeof(Entity))
        {
            Description = "A lovely fluffy bush.",
            Height = 1,
            Weight = 5,
            IsHaulable = false,
            DefaultMaterial = MaterialDefOf.ShrubStem,
            Body = new Bone(BoneDefOf.PlantStem, ItemContent.BerryBushGrowing).AddJoint(new Bone(BoneDefOf.PlantFruit) { DrawMaterialColor = true }),
            DefaultBoneStruct = BoneStructureDefOf.Bush,
            Size = ObjectSize.Haulable,
            Comps = [typeof(PlantComponent), typeof(ResourcesComp)],
            CompDefs = [EntityCompDefOf.Plant, EntityCompDefOf.Resources]

        }
        .AddSpec(new ResourcesComp.Spec([ResourceDefOf.HitPoints]));

        static PlantDefOf()
        {
            Def.Register(Tree);
            Def.Register(Bush);

            var bush = PlantSpeciesDefOf.Berry.Create(PlantStageDefOf.Plant);
            var plantComp = bush.GetComponent<PlantComponent>();
            plantComp.GrowthBody.Percentage = 1;
            plantComp.GrowthFruit.Percentage = 1;
            GameObject.AddTemplate(bush);

            var tree = PlantSpeciesDefOf.LightTree.Create(PlantStageDefOf.Plant);
            tree.GetComponent<PlantComponent>().GrowthBody.Percentage = 1;
            GameObject.AddTemplate(tree);

            var allPlants = Def.Get<PlantSpeciesDef>();
            GameObject.AddTemplates(allPlants.Select(p => p.Create(PlantStageDefOf.Seed)));
            GameObject.AddTemplates(allPlants.Where(p => p.ProducesFruit).Select(p => p.Create(PlantStageDefOf.Fruit)));

            Def.Register(new Reaction("Extract Seeds", SkillDefOf.Argiculture)
                .AddBuildSite(IsWorkstation.Types.PlantProcessing)
                .AddIngredient("a", new Ingredient()
                    .SetAllow(ItemDefOf.Fruit, true))
                .AddProduct(new Reaction.Product(i => Def.Get<PlantSpeciesDef>().First(d => d.FruitMaterial == i["a"].PrimaryMaterial).Create(PlantStageDefOf.Seed) as Entity, 4))
                );
        }
    }
}

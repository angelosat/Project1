using Project1.Core.Effects;
using Project1.Core.Entities;
using Project1.Core.Graphics;
using Project1.Core.Needs;
using System;

namespace Project1.Core.Systems.Plants
{
    public class PlantSystem
    {
        static Entity CreateSeeds(PlantSpeciesDef species)
        {
            var seeds = ItemDefOf.Seeds.Create();
            seeds.Initialize();

            seeds.Profile = species;
            seeds.Name = $"{species.LabelReadable} {species.SeedsName}";
            seeds.Body.Sprite = Sprite.Load(species.TextureSeeds);
            return seeds;
        }

        static Entity CreatePlant(PlantSpeciesDef species)
        {
            var entity = species.PlantEntity.Create();
            entity.Profile = species;

            var plantcomp = entity.GetComponent<PlantComponent>();
            entity.Initialize();
            if (species.PlantEntity == PlantDefOf.Tree)
                entity.SetMaterial(species.StemMaterial);
            else if (species.ProducesFruit)
                entity.Name = entity.Name.Insert(0, $"{species.FruitMaterial.LabelReadable} ");
            return entity;
        }
        static Entity CreateFruit(PlantSpeciesDef species)
        {
            var entity = ItemDefOf.Fruit.Create();
            entity.Profile = species;
            var comp = entity.GetComponent<ConsumableComponent>();
            comp.EffectsNew.Add(new EntityEffectWrapper(EffectDefOf.ModifyNeed, NeedDefOf.Hunger, Budget: 5, Rate: 0));
            entity.Name = $"{species.LabelReadable}";
            entity.SetMaterial(species.FruitMaterial);
            return entity;
        }
        public static Entity Create(PlantSpeciesDef species, PlantStageDef form)
        {
            if (form == null)
            {
                Log.Warning($"No stage provided for {species}, defaulting to {PlantStageDefOf.Seed}.");
                form = PlantStageDefOf.Seed;
            }
            if (form == PlantStageDefOf.Seed) return CreateSeeds(species);
            else if (form == PlantStageDefOf.Plant) return CreatePlant(species);
            else if (form == PlantStageDefOf.Fruit) return CreateFruit(species);
            throw new InvalidOperationException();
        }

        internal static Entity Create(EntityCreationRequest req)
        {
            return Create(req.Context as PlantSpeciesDef, req.Stage as PlantStageDef);
        }

    }
}

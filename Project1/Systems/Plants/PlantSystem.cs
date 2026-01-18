using Start_a_Town_.Components;
using System;

namespace Start_a_Town_
{
    public class PlantSystem// : IItemCreationSystem
    {
        static Entity CreateSeeds(PlantSpeciesDef species)
        {
            var seeds = ItemDefOf.Seeds.Create();
            seeds.Initialize();

            //seeds.GetComponent<SeedComponent>().SetPlant(species);
            seeds.Profile = species;
            seeds.Name = $"{species.Label} {species.SeedsName}";
            seeds.Body.Sprite = Sprite.Load(species.TextureSeeds);
            return seeds;
        }

        static Entity CreatePlant(PlantSpeciesDef species)
        {
            var entity = species.PlantEntity.Create();
            entity.Profile = species;

            var plantcomp = entity.GetComponent<PlantComponent>();
            entity.Initialize();
            //plantcomp.SetSpecies(species);
            if (species.PlantEntity == PlantDefOf.Tree)
                entity.SetMaterial(species.StemMaterial);
            else if (species.ProducesFruit)
                entity.Name = entity.Name.Insert(0, $"{species.FruitMaterial.Label} ");
            return entity;
        }
        static Entity CreateFruit(PlantSpeciesDef species)
        {
            var entity = ItemDefOf.Fruit.Create();
            entity.Profile = species;
            var comp = entity.GetComponent<ConsumableComponent>();
            comp.EffectsNew.Add(new EntityEffectWrapper(EffectDefOf.ModifyNeed, NeedDefOf.Hunger, 20));
            entity.Name = $"{species.Label}";
            entity.SetMaterial(species.FruitMaterial);
            return entity;
        }
        //public Entity Create(Def def, ItemCreationArgs args)
        //{
        //    if (args is not Args a)
        //        throw new InvalidOperationException($"{nameof(PlantSystem)} received wrong args");
        //    if (def is not PlantSpeciesDef profile)
        //        throw new InvalidOperationException($"{nameof(PlantSystem)} received wrong profile");
        //    if (a.Form == PlantStageDefOf.Seed)
        //        return CreateSeeds(profile);
        //    else if (a.Form == PlantStageDefOf.Plant)
        //        return CreatePlant(profile);
        //    else throw new InvalidOperationException($"{nameof(a.Form)} was invalid");
        //}

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

        public class Args(PlantStageDef form) : ItemCreationArgs
        {
            public PlantStageDef Form = form;
        }
    }
}

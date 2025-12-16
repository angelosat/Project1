using Start_a_Town_.Components;
using System;

namespace Start_a_Town_
{
    public class PlantSystem : IItemCreationSystem
    {
        static Entity CreateSeeds(PlantSpeciesDef species)
        {
            var seeds = ItemDefOf.Seeds.Create();
            seeds.Initialize();

            seeds.GetComponent<SeedComponent>().SetPlant(species);

            seeds.Name = $"{species.Label} {species.SeedsName}";
            seeds.Body.Sprite = Sprite.Load(species.TextureSeeds);
            return seeds;
        }

        static Entity CreatePlant(PlantSpeciesDef species)
        {
            var entity = species.PlantEntity.Create();
            var plantcomp = entity.GetComponent<PlantComponent>();
            entity.Initialize();

            plantcomp.SetSpecies(species);
            if (species.PlantEntity == PlantDefOf.Tree)
                entity.SetMaterial(species.StemMaterial);
            else if (species.ProducesFruit)
                entity.Name = entity.Name.Insert(0, $"{species.FruitMaterial.Label} ");
            return entity;
        }

        public Entity Create(Def def, ItemCreationArgs args)
        {
            if (args is not Args a)
                throw new InvalidOperationException($"{nameof(PlantSystem)} received wrong args");
            if (def is not PlantSpeciesDef profile)
                throw new InvalidOperationException($"{nameof(PlantSystem)} received wrong profile");
            if (a.Form == PlantStageDefOf.Seed)
                return CreateSeeds(profile);
            else if (a.Form == PlantStageDefOf.Plant)
                return CreatePlant(profile);
            else throw new InvalidOperationException($"{nameof(a.Form)} was invalid");
        }

        public static Entity Create(PlantSpeciesDef species, PlantStageDef form)
        {
            if (form == PlantStageDefOf.Seed)
                return CreateSeeds(species);
            else if (form == PlantStageDefOf.Plant)
                return CreatePlant(species);
            throw new InvalidOperationException();
        }

        static public Entity Create<TArgs>(Def profile, TArgs args)
        where TArgs : Args
        {
            return null;
            //return (TEntity)System.Create(profile, args);
        }

        public class Args(PlantStageDef form) : ItemCreationArgs
        {
            public PlantStageDef Form = form;
        }
    }
}

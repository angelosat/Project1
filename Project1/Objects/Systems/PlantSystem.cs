using Start_a_Town_.Components;
using System;

namespace Start_a_Town_
{
    public class PlantSystem : IItemCreationSystem
    {
        static Entity CreateSeeds(PlantProperties species)
        {
            //var seeds = (Entity)Activator.CreateInstance(ItemDefOf.Seeds.ItemClass);
            var seeds = ItemDefOf.Seeds.Create();
            seeds.GetComponent<SeedComponent>().SetPlant(species);
            seeds.Name = $"{species.Label} {species.SeedsName}";
            seeds.Body.Sprite = Sprite.Load(species.TextureSeeds);
            return seeds;
        }

        static Entity CreatePlant(PlantProperties species)
        {
            //var entity = (Entity)Activator.CreateInstance(species.PlantEntity.ItemClass);
            var entity = species.PlantEntity.Create();
            var plantcomp = entity.GetComponent<PlantComponent>();
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
            if (def is not PlantProperties profile)
                throw new InvalidOperationException($"{nameof(PlantSystem)} received wrong profile");
            if (a.Form == PlantFormDefOf.Seed)
                return CreateSeeds(profile);
            else if (a.Form == PlantFormDefOf.Plant)
                return CreatePlant(profile);
            else throw new InvalidOperationException($"{nameof(a.Form)} was invalid");
        }

        public static Entity Create(PlantProperties species, PlantFormDef form)
        {
            if (form == PlantFormDefOf.Seed)
                return CreateSeeds(species);
            else if (form == PlantFormDefOf.Plant)
                return CreatePlant(species);
            throw new InvalidOperationException();
        }

        static public Entity Create<TArgs>(Def profile, TArgs args)
        where TArgs : Args
        {
            return null;
            //return (TEntity)System.Create(profile, args);
        }

        public class Args(PlantFormDef form) : ItemCreationArgs
        {
            public PlantFormDef Form = form;
        }
    }
}

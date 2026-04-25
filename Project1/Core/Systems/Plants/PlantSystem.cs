using Project1.Core.Entities;
using Project1.Core.Graphics;
using Project1.Core.Systems.Materials;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Plants;

public sealed class PlantSystem
{
    static Dictionary<MaterialDef, PlantSpeciesDef> _matsToSpecies = [];
    static PlantSystem()
    {
        _matsToSpecies = Def.Get<PlantSpeciesDef>().Where(s=>s.FruitMaterial is not null).ToDictionary(s => s.FruitMaterial);
    }

    static public Entity CreateSeeds(PlantSpeciesDef species)
    {
        var seeds = ItemDefOf.Seeds.Create();
        seeds.Initialize();

        seeds.Profile = species;
        seeds.Name = $"{species.LabelReadable} {species.SeedsName}";
        seeds.Body.Sprite = Sprite.Load(species.TextureSeeds);
        seeds.GetComponent<SeedComponent>().Species = species;
        return seeds;
    }
    static public Entity CreateSeeds(MaterialDef material)
    {
        var seeds = ItemDefOf.Seeds.Create();
        seeds.Initialize();
        var species = _matsToSpecies[material];
        seeds.Profile = species;
        seeds.Name = $"{species.LabelReadable} {species.SeedsName}";
        seeds.Body.Sprite = Sprite.Load(species.TextureSeeds);
        seeds.GetComponent<SeedComponent>().Species = species;
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
        //var entity = ItemDefOf.Fruit.Create();
        var entity = ItemDefOf.Ingredient.Create();
        entity.Profile = MaterialRefinementDefOf.FruitRaw;
        entity.Body.Sprite = Sprite.Load(species.TextureFruit);
        //entity.Profile = species;
        //var comp = entity.GetComponent<ConsumableComponent>();
        //comp.EffectsNew.Add(new EntityEffectWrapper(EffectDefOf.ModifyNeed, NeedDefOf.Hunger, Budget: 5, Rate: 0));
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

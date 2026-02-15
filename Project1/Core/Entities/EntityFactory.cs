using Project1.Core.Entities.Actors;
using Project1.Core.Materials;
using Project1.Core.Plants;
using Project1.Core.Tools;
using System;

namespace Project1.Core.Entities
{
    internal static class EntityFactory
    {
        //static public Entity Create(ItemDef context, Def state = null, MaterialDef defaultMaterial = null)
        //{
        //    return Create(new(context, state, defaultMaterial));
        //}
        //static public Entity Create(Def profile, MaterialDef defaultMaterial = null)
        //{
        //    return profile switch
        //    {
        //        MaterialRefinementDef => RawMaterialSystem.Create(new(ItemDefOf.Ingredient, profile, defaultMaterial)),
        //        ActorDnaDef => ActorSystem.Create(new(ActorDefOf.Npc, profile, defaultMaterial)),
        //        PlantSpeciesDef => PlantSystem.Create(new(ItemDefOf., profile, defaultMaterial)),
        //        ToolProfileDef => ToolSystem.Create(new(ItemDefOf.Ingredient, profile, defaultMaterial)),
        //        _ => throw new InvalidOperationException($"No system claims {profile}"),
        //    };
        //}
        static public Entity Create(Def profile, MaterialDef material)
        {
            return Create(new(profile, null, material));
        }
        static public Entity Create(EntityCreationRequest req)
        {
            return req.Context switch
            {
                MaterialRefinementDef => RawMaterialSystem.Create(req),
                ActorDnaDef => ActorSystem.Create(req),
                PlantSpeciesDef => PlantSystem.Create(req),
                ToolProfileDef => ToolSystem.Create(req),
                _ => throw new InvalidOperationException($"No system claims {req.Context.GetType().Name}"),
            };
        }

        static public EntityCreationRequest Request(Def profile, Def state = null, MaterialDef defaultMaterial = null)
        {
            var req = new EntityCreationRequest(profile, state, defaultMaterial);
            return req;
        }
    }
}

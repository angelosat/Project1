using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Plants;
using Project1.Core.Systems.Tools;
using System;

namespace Project1.Core.Entities
{
    internal static class EntityFactory
    {
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

        static public Entity Create(Def profile, Def state = null, MaterialDef defaultMaterial = null)
        {
            return Create(new EntityCreationRequest(profile, state, defaultMaterial));
        }
    }
}

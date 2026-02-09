using Project1.Core.Plants;
using Project1.Core.Entities.Actors;
using Project1.Core.Materials;
using Project1.Core.Tools;
using System;

namespace Project1.Core.Entities
{
    internal static class EntityFactory
    {

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

        static public EntityCreationRequest Request(Def context, Def state = null, MaterialDef defaultMaterial = null)
        {
            var req = new EntityCreationRequest(context, state, defaultMaterial);
            return req;
        }
    }
}

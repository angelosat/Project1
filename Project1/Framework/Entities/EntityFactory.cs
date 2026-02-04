using Project1.Core.Plants;
using Project1.Framework.Base;
using Project1.Framework.Entities.Actors;
using Project1.Framework.Materials;
using Project1.Framework.Tools;
using Start_a_Town_;
using System;

namespace Project1.Framework.Entities
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

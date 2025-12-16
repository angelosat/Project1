using System;

namespace Start_a_Town_
{
    internal static class EntityFactory
    {

        static public Entity Create(EntityCreationRequest req)
        {
            return req.Context switch
            {
                MaterialDef => RawMaterialSystem.Create(req),
                ActorDnaDef => ActorSystem.Create(req),
                PlantSpeciesDef => PlantSystem.Create(req),
                ToolProfileDef => ToolSystem.Create(req),
                _ => throw new InvalidOperationException($"No system claims {req.Context.GetType().Name}"),
            };
        }

        static public EntityCreationRequest Request(Def template, Def stage = null)
        {
            var req = new EntityCreationRequest(template, stage);
            return req;
        }
    }
}

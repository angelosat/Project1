using System;

namespace Start_a_Town_
{
    public class ActorSystem
    {
        static public Actor Create(ActorDnaDef profile)
        {
            //if (profile is not ActorProfileDef typedProfile)
            //    throw new System.Exception();
            var entity = ActorDefOf.Npc.Create(profile) as Actor;
            entity.Components.ApplySpecs(profile.GenerateSpecs());
            //entity.AI.Root = typedProfile.Behavior;
            entity.Initialize();
            return entity;
        }

        internal static Entity Create(EntityCreationRequest req)
        {
            return Create(req.Context as ActorDnaDef);
        }
    }
}

using Project1.Core.AI.MetaRoles;

namespace Project1.Core.Entities.Actors
{
    public class ActorSystem
    {
        static public Actor Create(ActorDnaDef profile, RoleMetaDef roleMeta)
        {
            var actor = ActorDefOf.Npc.Create(profile) as Actor;
            actor.Components.ApplySpecs(profile.GenerateSpecs());
            roleMeta.AssignTo(actor);
            actor.Initialize();
            return actor;
        }

        internal static Entity Create(EntityCreationRequest req)
        {
            return Create(req.Context as ActorDnaDef, req.Stage as RoleMetaDef);
        }
    }
}

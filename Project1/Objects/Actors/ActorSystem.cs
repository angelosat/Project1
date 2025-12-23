namespace Start_a_Town_
{
    public class ActorSystem
    {
        static public Actor Create(ActorDnaDef profile, RoleMetaDef roleMeta)
        {
            var actor = ActorDefOf.Npc.Create(profile) as Actor;
            actor.Components.ApplySpecs(profile.GenerateSpecs());
            //var roleWrapper = roleMeta.Create();
            //roleWrapper.AssignTo(actor);
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

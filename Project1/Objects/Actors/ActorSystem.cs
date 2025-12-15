namespace Start_a_Town_
{
    public class ActorSystem// : IItemCreationSystem
    {
        static public Actor Create(ActorProfileDef profile)
        {
            //if (profile is not ActorProfileDef typedProfile)
            //    throw new System.Exception();
            var entity = ActorDefOf.Npc.Create(profile) as Actor;
            entity.Components.ApplySpecs(profile.GenerateSpecs());
            //entity.AI.Root = typedProfile.Behavior;
            entity.Initialize();
            return entity;
        }
    }
}

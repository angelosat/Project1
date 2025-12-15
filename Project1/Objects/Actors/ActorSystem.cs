namespace Start_a_Town_
{
    public class ActorSystem// : IItemCreationSystem
    {
        static public Entity Create(Def profile)
        {
            if (profile is not ActorProfileDef typedProfile)
                throw new System.Exception();
            var entity = ActorDefOf.Npc.Create() as Actor;
            entity.Components.ApplySpecs(typedProfile.GenerateSpecs());
            //entity.AI.Root = typedProfile.Behavior;
            entity.Initialize();
            return entity;
        }
    }
}

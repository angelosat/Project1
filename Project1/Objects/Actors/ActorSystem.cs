namespace Start_a_Town_
{
    public class ActorSystem : IItemCreationSystem
    {
        public Entity Create(Def profile, ItemCreationArgs args)
        {
            if (profile is not ActorProfileDef typedProfile)
                throw new System.Exception();
            var entity = ActorDefOf.Npc.Create() as Actor;
            entity.Components.ApplySpecs(typedProfile.GenerateSpecs());
            //entity.AI.Root = typedProfile.Behavior;
            return entity;
        }
    }
}

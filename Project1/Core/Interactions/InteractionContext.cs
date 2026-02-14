using Project1.Core.Entities.Actors;

namespace Project1.Core.Interactions
{
    public class InteractionContext
    {
        public Actor Actor;
        public TargetArgs Target;
        public int Count;
        public virtual float ProgressPercentage { get; }
    }
}

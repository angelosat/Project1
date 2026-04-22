using Project1.Core.Entities.Actors;
using Project1.Core.Skills;

namespace Project1.Core.Interactions
{
    public class InteractionContext
    {
        public Actor Actor;
        public InteractionTarget Target;
        public int Count;
        public virtual float ProgressBarPercentage { get; }

        internal virtual float GetPercentage(Interaction i) => i.Progress.Percentage;

        public virtual SkillDef? Skill => null;
    }
}

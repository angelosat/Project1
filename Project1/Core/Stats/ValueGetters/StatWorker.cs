using Project1.Core.Skills;

namespace Project1.Core.Entities.Stats.ValueGetters
{
    public abstract class StatWorker
    {
        public abstract float CalculateStat(Entity obj);
        public virtual float CalculateStat(Skill skill) => skill.Level;
    }
}

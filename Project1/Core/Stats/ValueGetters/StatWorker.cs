using Project1.Core.Entities;
using Project1.Core.Skills;

namespace Project1.Core.Entities.Stats.ValueGetters
{
    public abstract class StatWorker
    {
        //public StatDef Stat;
        //public StatWorker(StatDef parent)
        //{
        //    this.Stat = parent;
        //}
        public abstract float CalculateStat(GameObject obj);
        public virtual float CalculateStat(Skill skill) => skill.Level;
    }
}

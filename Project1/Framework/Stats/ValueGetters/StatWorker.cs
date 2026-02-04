using Project1.Framework.Entities;
using Project1.Framework.Skills;

namespace Project1.Framework.Stats.ValueGetters
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

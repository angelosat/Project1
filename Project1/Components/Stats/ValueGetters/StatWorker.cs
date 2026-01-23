namespace Start_a_Town_
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

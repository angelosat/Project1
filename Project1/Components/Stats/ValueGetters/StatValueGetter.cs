namespace Start_a_Town_
{
    abstract class StatValueGetter
    {
        public StatDef Stat;
        public StatValueGetter(StatDef parent)
        {
            this.Stat = parent;
        }
        public abstract float GetValue(GameObject obj);
    }
}

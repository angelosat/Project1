namespace Start_a_Town_
{
    public abstract class AttributeWorker
    {
        AttributeDef Def;

        public AttributeWorker(AttributeDef def)
        {
            this.Def = def;
        }

        public abstract void Tick(GameObject obj, AttributeStat attributeStat);
        internal virtual void Award(GameObject obj, AttributeStat attributeStat, float p)
        {
            attributeStat.AddToProgress(p);
        }
    }
}

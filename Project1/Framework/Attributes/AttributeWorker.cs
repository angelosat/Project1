using Start_a_Town_;

namespace Project1.Framework.Attributes
{
    public abstract class AttributeWorker
    {
        AttributeDef Def;

        public AttributeWorker(AttributeDef def)
        {
            this.Def = def;
        }

        public abstract void Tick(GameObject obj, AttributeRuntime attributeStat);
        internal virtual void Award(GameObject obj, AttributeRuntime attributeStat, float p)
        {
            attributeStat.AddToProgress(p);
        }
    }
}

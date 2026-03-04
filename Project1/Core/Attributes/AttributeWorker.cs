using Project1.Core.Entities;

namespace Project1.Core.Attributes
{
    public abstract class AttributeWorker
    {
        public abstract void Tick(Entity obj, AttributeRuntime attributeStat);
        internal virtual void Award(Entity obj, AttributeRuntime attributeStat, float p)
        {
            attributeStat.AddToProgress(p);
        }
    }
}

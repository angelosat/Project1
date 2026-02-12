using Project1.Core.Entities;
using Project1.Framework;

namespace Project1.Core.Helpers
{
    public abstract class MetricWrapper: Inspectable
    {
        public Entity Owner;
        public int _value;
        public abstract void Tick();
    }
}

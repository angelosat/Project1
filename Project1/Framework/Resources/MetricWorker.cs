using Project1.Framework.Helpers;

namespace Project1.Framework.Resources
{
    public abstract class MetricWorker
    {
        public virtual void Tick(MetricWrapper wrapper) { }
    }
}

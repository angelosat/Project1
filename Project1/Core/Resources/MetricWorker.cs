using Project1.Core.Helpers;

namespace Project1.Core.Resources
{
    public abstract class MetricWorker
    {
        public virtual void Tick(MetricWrapper wrapper) { }
    }
}

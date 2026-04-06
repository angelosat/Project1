using Project1.Core.Blocks;
using Project1.Core.Entities;

namespace Project1.Core.Simulation
{
    public abstract class MapComponent(MapBase map)
    {
        readonly protected MapBase Map = map;

        public virtual void Tick() { }
        protected virtual internal void ResolveReferences() { }

        internal virtual void Scan(BlockEntity be) { }

        internal virtual void Scan(Entity entity) { }
        
    }
}
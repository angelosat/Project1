using Project1.Framework.WorldGen;

namespace Project1.Framework.StaticMaps.Components
{
    public abstract class MapComponent(MapBase map)
    {
        readonly protected MapBase Map = map;

        public abstract void Tick();
        protected virtual internal void ResolveReferences() { }
    }
}

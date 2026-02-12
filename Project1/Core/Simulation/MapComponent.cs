namespace Project1.Core.Simulation
{
    public abstract class MapComponent(MapBase map)
    {
        readonly protected MapBase Map = map;

        public virtual void Tick() { }
        protected virtual internal void ResolveReferences() { }
    }
}
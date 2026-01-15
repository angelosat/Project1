namespace Start_a_Town_
{
    public abstract class MapComponent(MapBase map)
    {
        readonly protected MapBase Map = map;

        public abstract void Tick();
        protected virtual internal void ResolveReferences() { }
    }
}

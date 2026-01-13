namespace Start_a_Town_
{
    public abstract class MapComponent
    {
        public abstract void Tick();
        protected virtual internal void ResolveReferences() { }
    }
}

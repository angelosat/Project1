namespace Project1.Core.Simulation
{
    internal abstract class SimulationSystem(MapBase map)
    {
        internal readonly MapBase Map = map;
        public virtual void Tick() { }
    }
}
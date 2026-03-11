using Project1.Core.Simulation;
using Project1.Core.Simulation.Physics;

namespace Project1.Core.Systems.Plants
{
    internal class PlantLifeCycleSystem : SimulationSystem
    {
        public PlantLifeCycleSystem(MapBase map) : base(map)
        {
            map.Events.ListenTo<EntityCollisionEvent>(HandleEntityCollisionEvent);
        }

        private void HandleEntityCollisionEvent(EntityCollisionEvent e)
        {
            e.Source.GetComponent<PlantComponent>()?.Wiggle();
            e.Target.GetComponent<PlantComponent>()?.Wiggle();
        }
    }
}

using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;

namespace Project1.Core.Systems.Lifecycle;

internal class WorldComp_LifeCycle : WorldComp
{
    public WorldComp_LifeCycle(WorldBase world) : base(world)
    {
        world.Events.ListenTo<EntityKilledEvent>(HandleEntityKilled);
    }

    private void HandleEntityKilled(EntityKilledEvent e)
    {
        if (e.Entity is not Actor)
            return;
    }
}

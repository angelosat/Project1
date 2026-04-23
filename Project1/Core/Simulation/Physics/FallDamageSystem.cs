using Project1.Core.Entities.Actors;
using Project1.Core.Resources;

namespace Project1.Core.Simulation.Physics;
internal sealed class FallDamageSystem : SimulationSystem
{
    public FallDamageSystem(MapBase map) : base(map)
    {
        map.Events.ListenTo<EntityHitGroundEvent>(OnEntityHitGroundEvent);
    }

    private void OnEntityHitGroundEvent(EntityHitGroundEvent e)
    {
        var force = e.Force;
        if (force < 1)
            return;
        if (e.Entity is not Actor actor)
            return;
        actor
            .GetResource(ResourceDefOf.Health)
            .ApplyAccumulatorDelta(force);
    }
}
using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;

namespace Project1.Core.World.Actions;

internal sealed class WorldActionWorker_CombatEngage : WorldActionWorker
{
    internal override void Resolve(WorldActionData data)
    {
        var typed = data as WorldActionData_CombatEngage;
        var world = (StaticWorld)data.Actor.World;
        var other = world.Resolve<Actor>(typed.TargetId);
        world.Combat.Engage(data.Actor, other);
    }
}

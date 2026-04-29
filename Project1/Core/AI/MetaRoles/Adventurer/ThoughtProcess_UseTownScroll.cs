using Project1.Core.AI.Thought;
using Project1.Core.Entities;
using Project1.Core.Systems.Consumables;

namespace Project1.Core.AI.MetaRoles.Adventurer;

internal class ThoughtProcess_UseTownScroll : ThoughtProcess
{
    internal override void TickOffMap(AIState state)
    {
        var actor = state.Owner;
        if (actor.Net.IsClient)
            return;
        var meta = actor.AI.Meta;
        if (meta.TargetFrontier is not null)
            return;
        if (actor.Inventory.First(i => i.Profile == ConsumableDefOf.Scroll) is not Entity item)
            return;
        var map = actor.Net.World.MainMap;
        if (!map.Town.Waypoint.HasValue)
            return;
        ConsumableDefOf.Scroll.Effect.Execute(actor);
        item.Consume(1);
    }

    internal override void TickOnMap(AIState state)
    {
    }
}

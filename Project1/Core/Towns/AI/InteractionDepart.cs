using Project1.Core.Interactions;
using Project1.Core.Simulation;

namespace Project1.Core.Towns.AI;

class InteractionDepart : InteractionLogic
{
    internal override void OnStart(Interaction i)
    {
        var a = i.Actor;
        // TODO make this interaction authoritative and create a actoreneteredoffmap event to replicate to clients
        if (a.Net.IsClient)
            return;
        var world = a.World as StaticWorld;
        world.Space.PlaceAt(a, 0);
        i.Finish();
    }
}

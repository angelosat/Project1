using Project1.Core.Interactions;
using Project1.Core.Simulation;
using Project1.Core.World.WorldAreas;

namespace Project1.Core.Towns.AI
{
    class InteractionDepartLogic : InteractionLogic
    {
        internal override void OnStart(Interaction i)
        {
            var a = i.Actor;
            var area = FrontierDefOf.Forest; //TODO store target visitor area in the visitorproperites class when the decision to depart occurs and fetch it from there
            var world = a.World as StaticWorld;
            world.Space.Enter(a);
            i.Finish();
        }
    }
}

using Project1.Core.Towns.AI.Needs;
using Project1.Core.Helpers;
using Project1.Core.Net;

namespace Project1.Core.World.MetaRoles
{
    internal class RoleAdventurerWorker : RoleMetaWorker
    {
        internal override void Tick(RoleMetaWrapper meta)
        {
            var actor = meta.Actor;
            if (actor.Net is not Server server)
                return;
            var world = actor.World;
            if (!meta.LocationDecision.CanEvaluate(world.CurrentTick))
                return;
            var adventureNeed = actor.GetNeed(AdventurerNeedsDefOf.Adventuring);
            var roll = world.Random.Roll(adventureNeed.Percentage);
            if (roll)
            {
                meta.LocationDecision.RegisterSuccess();
                actor.AI.Meta.ReturnToTown();
                actor.AI.State.Log.Write("I'm returning to town.");
            }
            else
            {
                meta.LocationDecision.RegisterFailure();
                actor.AI.State.Log.Write("I'll stay out adventuring some more.");
            }
            meta.LocationDecision.ScheduleNext(world);
        }
    }
}

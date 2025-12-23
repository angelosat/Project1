using Start_a_Town_.Net;
namespace Start_a_Town_
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
            meta.LocationDecision.ScheduleNext(world);
            var adventureNeed = actor.GetNeed(AdventurerNeedsDefOf.Adventuring);
            var roll = world.Random.Roll(adventureNeed.Percentage);
            if (roll)
            {
                meta.LocationDecision.RegisterSuccess();

                actor.AI.Meta.ReturnToTown();
                AILog.SyncWrite(actor, "I'm returning to town.");
                //actor.Log.Write("I'm returning to town.");
            }
            else
            {
                meta.LocationDecision.RegisterFailure();
                AILog.SyncWrite(actor, "I'll stay out adventuring some more.");
                //actor.Log.Write("I'll stay out adventuring some more.");
            }
        }
    }
}

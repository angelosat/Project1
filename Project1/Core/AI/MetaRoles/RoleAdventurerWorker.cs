using Project1.Core.Towns.AI.Needs;
using Project1.Framework.Helpers;

namespace Project1.Core.AI.MetaRoles
{
    internal class RoleAdventurerWorker : RoleMetaWorker
    {
        internal override void Tick(RoleMetaWrapper meta)
        {
            var actor = meta.Actor;
            if (actor.Net.IsClient)
                return;
           
            var needDelta = (actor.IsSpawned ? -1 : 1) / (float)Ticks.PerGameMinute;
            actor.Needs.ApplyAccumulatorDelta(AdventurerNeedsDefOf.Adventuring, needDelta);

            var world = actor.World;
            if (!meta.LocationDecision.CanEvaluate(world.CurrentTick))
                return;
            var roll = world.Random.Roll(actor.Needs.GetPercentage(AdventurerNeedsDefOf.Adventuring));
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

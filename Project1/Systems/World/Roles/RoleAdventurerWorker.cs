using Start_a_Town_.Net;

namespace Start_a_Town_
{
    internal class RoleAdventurerWorker : RoleMetaWorker
    {
        internal override void Tick(Actor actor)
        {
            if (actor.Net is not Server server)
                throw new System.Exception();
            var adventureNeed = actor.GetNeed(AdventurerNeedsDefOf.Adventuring);
            var roll = actor.World.Random.Roll(adventureNeed.Percentage);
            if (roll)
                actor.AI.Meta.ReturnToTown();
        }
    }
}

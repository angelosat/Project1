using Project1.Core.Entities.Actors;

namespace Project1.Core.Systems.Factions;

static class FactionsSystem
{
    extension(Actor actor)
    {
        public Faction Faction => actor.FactionId.HasValue ? actor.World.GetFaction(actor.FactionId.Value) : null;
    }

    static public bool AreEnemies(Actor first, Actor second)
    {
        var thisFaction = first.Faction;

        if (!second.FactionId.HasValue)
            return false;

        var relationship = thisFaction.GetRelation(second.FactionId.Value);
        return relationship < 0;
    }
}

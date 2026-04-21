using Project1.Core.Towns.AI.Needs;
using Project1.Core.World.WorldAreas;

namespace Project1.Core.AI.MetaRoles.Adventurer;

internal sealed class FrontierDecider_ReturnToTown : FrontierDecider
{
    internal override (FrontierDecider decider, FrontierDef frontier, int score) GetScore(AIComp comp)
    {
        var meta = comp.GetMeta<RoleAdventurerData>();
        //var desire = meta.NextDesiredLoot;
        //if (!desire.HasValue)
        //    return (this, default, default);
        var perc = comp.Owner.Needs.GetPercentage(AdventurerNeedsDefOf.Adventuring);
        var score = 100 - (int)(perc * perc * 100);
        return (this, null, score);
    }
}

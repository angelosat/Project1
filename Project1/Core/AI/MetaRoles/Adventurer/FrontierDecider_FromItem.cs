using Project1.Core.World.WorldAreas;

namespace Project1.Core.AI.MetaRoles.Adventurer;

internal sealed class FrontierDecider_FromItem : FrontierDecider
{
    internal override (FrontierDecider decider, FrontierDef frontier, int score) GetScore(AIComp comp)
    {
        var meta = comp.GetMeta<RoleAdventurerData>();
        var desire = meta.NextDesiredLoot;
        if (!desire.HasValue)
            return (this, default, default);
        return (this, FrontierManager.GetFrontier(desire.Value.matdef.Tier), 100);
    }
}

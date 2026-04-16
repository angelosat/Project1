using Project1.Core.World.WorldAreas;

namespace Project1.Core.AI.MetaRoles.Adventurer;

internal abstract class FrontierDecider
{
    internal abstract (FrontierDecider decider, FrontierDef frontier, int score) GetScore(AIComp comp);
}

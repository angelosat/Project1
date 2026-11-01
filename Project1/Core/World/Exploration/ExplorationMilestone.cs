using Project1.Core.World.Frontiers;

namespace Project1.Core.World.Exploration;

internal abstract class ExplorationMilestone
{
    internal abstract void Apply(FrontierRuntime frontier);
}

internal class ExplorationMilestone_ResourceNode : ExplorationMilestone
{
    const int TargetCount = 10;
    internal override void Apply(FrontierRuntime frontier)
    {
    }
}


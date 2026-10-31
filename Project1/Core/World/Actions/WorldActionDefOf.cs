using Project1.Framework;

namespace Project1.Core.World.Actions;

[EnsureStaticCtorCall]
static public class WorldActionDefOf
{
    static public readonly WorldActionDef EnterPoI = new("EnterPoI", typeof(WorldActionWorker_EnterPoI), duration: Ticks.FromMinutes(5));
    static public readonly WorldActionDef HarvestResource = new("HarvestResource", typeof(WorldActionWorker_HarvestResource), duration: Ticks.FromMinutes(20));
    static public readonly WorldActionDef Rest = new("Resting", typeof(WorldActionWorker_Resting), duration: Ticks.FromMinutes(30));
    static public readonly WorldActionDef AvoidInjury = new("AvoidInjury", typeof(WorldActionWorker_AvoidInjury), duration: Ticks.FromMinutes(10));
    static public readonly WorldActionDef Explore = new("Exploring", typeof(WorldActionWorker_Exploration), duration: Ticks.FromMinutes(10));
    static WorldActionDefOf()
    {
        Def.Register(typeof(WorldActionDefOf));
    }
}

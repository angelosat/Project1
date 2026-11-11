using Project1.Core.World.Combat;
using Project1.Framework;

namespace Project1.Core.World.Actions;

[EnsureStaticCtorCall]
static public class WorldActionDefOf
{
    static public readonly WorldActionDef EnterPoI = new("EnterPoI",
                                                         workerType: typeof(WorldActionWorker_PoIEnter),
                                                         dataType: typeof(WorldActionData_PoI),
                                                         duration: Ticks.FromMinutes(5));

    static public readonly WorldActionDef ExitPoI = new("ExitPoI",
                                                         workerType: typeof(WorldActionWorker_PoIExit),
                                                         //dataType: typeof(WorldActionData_PoI),
                                                         duration: Ticks.FromMinutes(5));

    static public readonly WorldActionDef HarvestResource = new("HarvestResource",
                                                                workerType: typeof(WorldActionWorker_HarvestResource),
                                                                dataType: typeof(WorldActionData_HarvestResource),
                                                                duration: Ticks.FromMinutes(20));
    static public readonly WorldActionDef Rest = new("Resting", typeof(WorldActionWorker_Resting), duration: Ticks.FromMinutes(30));
    static public readonly WorldActionDef AvoidInjury = new("AvoidInjury", typeof(WorldActionWorker_AvoidInjury), duration: Ticks.FromMinutes(10));
    static public readonly WorldActionDef Explore = new("Exploring", typeof(WorldActionWorker_Exploration), duration: Ticks.FromMinutes(10));
    static public readonly WorldActionDef LootTreasure = new("LootTreasure", typeof(WorldActionWorker_LootTreasure), duration: Ticks.FromMinutes(10));
    static public readonly WorldActionDef Combat = new("Combat", typeof(WorldActionWorker_CombatBegin), duration: Ticks.FromMinutes(1));
    static public readonly WorldActionDef Attack = new("Attacking",
                                                       workerType: typeof(WorldActionWorker_Attack),
                                                       dataType: typeof(WorldActionWorker_Attack.Data),
                                                       duration: Ticks.FromMinutes(5));
    static WorldActionDefOf()
    {
        Def.Register(typeof(WorldActionDefOf));
    }
}

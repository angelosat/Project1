using Project1.Core.World.Encounters;
using Project1.Core.World.PointsOfInterest;
using Project1.Framework;

#nullable enable

namespace Project1.Core.World.Actions;

[EnsureStaticCtorCall]
static public class EncounterDefOf
{
    static public readonly EncounterDef FarmLoot = new("FarmLoot", typeof(Encounter_FarmLoot), minutes: 10);
    static public readonly EncounterDef FindTreasure = new("FindTreasure", typeof(Encounter_FindTreasure), minutes: 10);
    static public readonly EncounterDef ActorEncounter = new("ActorEncounter", typeof(Encounter_OtherActor), minutes: 1);
    static public readonly EncounterDef PoIExploration = new("PoIExploration", typeof(Encounter_PoIExploration), minutes: 10);
    static public readonly EncounterDef Injury = new("Injury", typeof(Encounter_Injury), minutes: 10);
    static public readonly EncounterDef RecoverHealth = new("RecoverHealth", typeof(Encounter_RecoverHealth), minutes: 10);
    static public readonly EncounterDef Camp = new("Camp", typeof(Encounter_Camp), minutes: 10);
    static public readonly EncounterDef Exploration = new("Exploration", typeof(Encounter_Exploration), minutes: 10);

    static EncounterDefOf()
    {
        Def.Register(typeof(EncounterDefOf));
    }
}
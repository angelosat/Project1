using Project1.Core.World.Combat;
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
    static public readonly EncounterDef PoIEntrance = new("PoIEntrance", typeof(Encounter_PoIEntrance), minutes: 10);
    static public readonly EncounterDef PoIExit = new("PoIExit", typeof(Encounter_PoIExit), minutes: 10);
    static public readonly EncounterDef Injury = new("Injury", typeof(Encounter_Injury), minutes: 10);
    static public readonly EncounterDef RecoverHealth = new("RecoverHealth", typeof(Encounter_RecoverHealth), minutes: 10);
    static public readonly EncounterDef Camp = new("Camp", typeof(Encounter_Camp), minutes: 10);
    static public readonly EncounterDef Exploration = new("Exploration", typeof(Encounter_Exploration), minutes: 10);
    static public readonly EncounterDef Resource = new("Resource", typeof(Encounter_ResourceNode), minutes: 10);
    static public readonly EncounterDef Combat = new("Combat", typeof(Encounter_Combat), minutes: 0);

    static EncounterDefOf()
    {
        Def.Register(typeof(EncounterDefOf));
    }
}
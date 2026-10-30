using Project1.Core.World.PointsOfInterest;
using Project1.Framework;

#nullable enable

namespace Project1.Core.World;

[EnsureStaticCtorCall]
static public class EncounterDefOf
{
    static public readonly EncounterDef FarmLoot = new("FarmLoot", typeof(OffmapActivity_FarmLoot), minutes: 10);
    static public readonly EncounterDef FindTreasure = new("FindTreasure", typeof(OffmapActivity_FindTreasure), minutes: 10);
    static public readonly EncounterDef ActorEncounter = new("ActorEncounter", typeof(Encounter_OtherActor), minutes: 1);
    static public readonly EncounterDef PoIExploration = new("PoIExploration", typeof(Encounter_PoIExploration), minutes: 10);
    static public readonly EncounterDef Injury = new("Injury", typeof(OffmapActivity_Injury), minutes: 10);
    static public readonly EncounterDef RecoverHealth = new("RecoverHealth", typeof(OffmapActivity_RecoverHealth), minutes: 10);
    static public readonly EncounterDef Rest = new("Rest", typeof(OffmapActivity_Rest), minutes: 10);

    static EncounterDefOf()
    {
        Def.Register(typeof(EncounterDefOf));
    }
}
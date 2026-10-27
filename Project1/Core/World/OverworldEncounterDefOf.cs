using Project1.Framework;

#nullable enable

namespace Project1.Core.World;

[EnsureStaticCtorCall]
static public class OverworldEncounterDefOf
{
    static public readonly OverworldEncounterDef FarmLoot = new("FarmLoot", typeof(OffmapActivity_FarmLoot));
    static public readonly OverworldEncounterDef FindTreasure = new("FindTreasure", typeof(OffmapActivity_FindTreasure));
    static public readonly OverworldEncounterDef ActorEncounter = new("ActorEncounter", typeof(OffmapActivity_ActorEncounter));
    static public readonly OverworldEncounterDef Injury = new("Injury", typeof(OffmapActivity_Injury));
    static public readonly OverworldEncounterDef RecoverHealth = new("RecoverHealth", typeof(OffmapActivity_RecoverHealth));
    static public readonly OverworldEncounterDef Rest = new("Rest", typeof(OffmapActivity_Rest));

    static OverworldEncounterDefOf()
    {
        Def.Register(typeof(OverworldEncounterDefOf));
    }
}
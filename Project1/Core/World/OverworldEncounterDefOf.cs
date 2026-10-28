using Project1.Framework;

#nullable enable

namespace Project1.Core.World;

[EnsureStaticCtorCall]
static public class OverworldEncounterDefOf
{
    static public readonly EncounterDef FarmLoot = new("FarmLoot", typeof(OffmapActivity_FarmLoot));
    static public readonly EncounterDef FindTreasure = new("FindTreasure", typeof(OffmapActivity_FindTreasure));
    static public readonly EncounterDef ActorEncounter = new("ActorEncounter", typeof(OffmapActivity_ActorEncounter));
    static public readonly EncounterDef Injury = new("Injury", typeof(OffmapActivity_Injury));
    static public readonly EncounterDef RecoverHealth = new("RecoverHealth", typeof(OffmapActivity_RecoverHealth));
    static public readonly EncounterDef Rest = new("Rest", typeof(OffmapActivity_Rest));

    static OverworldEncounterDefOf()
    {
        Def.Register(typeof(OverworldEncounterDefOf));
    }
}
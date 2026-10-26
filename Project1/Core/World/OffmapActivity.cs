using Project1.Core.Entities.Actors;
using Project1.Core.World.WorldAreas;
using Project1.Framework;
using Project1.Framework.Helpers;
using System;

#nullable enable

namespace Project1.Core.World;

internal abstract class OffmapActivity
{
    internal abstract void Tick(FrontierRuntime frontier, Actor actor);
    internal abstract int GetWeight(FrontierRuntime frontier, Actor actor);
    internal virtual OverworldEncounterRuntime? TriggerFor(FrontierRuntime frontier, Actor actor) => null;
}

internal sealed class OverworldEncounterRuntime
{
    internal required OverworldEncounterDef Def;
    internal required ProgressInt Progress;
    //public enum States { InProgress, Paused, Finished };
    //public States State;
    //public float? PercentageComplete => null;
}

public sealed class OverworldEncounterDef(string name, Type workerType, int ticks = 0/*, Type runtimeType*/) : Def(name)
{
    //public readonly Type RuntimeType = runtimeType;
    internal OffmapActivity Worker => field ??= ActivatorSafe<OffmapActivity>.CreateInstance(workerType);
    internal int Duration = ticks;
}

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
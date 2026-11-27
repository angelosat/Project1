using Project1.Core.Simulation;
using Project1.Core.Systems.Effects;
using System;
using System.Collections.Generic;

namespace Project1.Core.Systems.Magic;

[Flags]
public enum SpellSubject { Self = 0x1, Other = 0x2, Any = Self | Other }
public enum SpellCategory { Neutral, Offensive, Defensive }

public sealed class SpellDef(string name,
                             TargetType targetType,
                             SpellSubject subject,
                             SpellCategory category,
                             SpellSchoolDef school,
                             int manaCost,
                             int castTimeInSecs//,
                             //EntityEffectWorker workerOnStart,
                             //EntityEffectWorker[] workers
    //,
    //                         EntityEffectSpec[] effects = null
                             )
    : Def(name)
{
    public readonly SpellSchoolDef School = school;

    public readonly int CastTimeSeconds = castTimeInSecs;

    public readonly int ManaCost = manaCost;

    public readonly OrderedDictionary<EntityEffectKey, int> Effects = [];
    public (EffectConfig Config, EffectSchedule Schedule)[] EffectsNew { get; init; }
    //new(effects.Select(e =>
    //    KeyValuePair.Create(
    //        new EntityEffectKey(e.Def, e.Target),
    //        e.Magnitude)));

    //public EffectConfig Instant { get; init; }

    //public EffectConfig[] Periodic { get; init; }

    public readonly TargetType TargetType = targetType;

    public readonly SpellCategory Category = category;

    public readonly SpellSubject Subject = subject;

    public SimulationDuration? Duration { get; init; }
    public int TickCount { get; init; }
    public SimulationDuration TickPeriod { get; init; }

}

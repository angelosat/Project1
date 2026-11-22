using Project1.Core.Entities.Stats;
using Project1.Core.Systems.Magic;
using System;
using System.Collections.Generic;

namespace Project1.Core.Systems.Effects;

public sealed class EffectDef(string name, string verb, EntityEffectWorker worker, Type targetDefType, int baseDuration = 0, int baseMagnitude = 0) : Def(name)
{
    internal Dictionary<Def, float> StatMods = [];
    internal string Verb = verb;
    internal EntityEffectWorker Worker = worker;
    internal Type TargetDefType = targetDefType;
    internal SpellSchoolDef School;
    internal int BaseDuration = baseDuration;
    internal int BaseMagnitude = baseMagnitude;
}
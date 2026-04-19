using System;

namespace Project1.Core.Systems.Effects;

public sealed class EffectDef(string name, string verb, EntityEffectWorker worker, Type targetDefType, int baseDuration = 0, int baseMagnitude = 0) : Def(name)
{
    internal string Verb = verb;
    internal EntityEffectWorker Worker = worker;
    internal Type TargetDefType = targetDefType;
    internal int BaseDuration = baseDuration;
    internal int BaseMagnitude = baseMagnitude;
}
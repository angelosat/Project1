using System;

namespace Project1.Core.Effects;

public class EffectDef(string name, string verb, EntityEffectWorker worker, Type targetDefType) : Def(name)
{
    internal string Verb = verb;
    internal EntityEffectWorker Worker = worker;
    internal Type TargetDefType = targetDefType;
}
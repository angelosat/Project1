namespace Project1.Core.Effects;

public class EffectDef(string name, string verb, EntityEffectWorker worker) : Def(name)
{
    internal string Verb = verb;
    internal EntityEffectWorker Worker = worker;
}
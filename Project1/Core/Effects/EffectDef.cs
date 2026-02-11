namespace Project1.Core.Effects
{
    public class EffectDef(string name, EntityEffectWorker worker) : Def(name)
    {
        internal EntityEffectWorker Worker = worker;
    }
}
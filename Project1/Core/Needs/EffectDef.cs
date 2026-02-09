using Project1.Core.Effects;

namespace Project1.Core.Needs
{
    public class EffectDef(string name, EntityEffectWorker worker) : Def(name)
    {
        internal EntityEffectWorker Worker = worker;
    }
}
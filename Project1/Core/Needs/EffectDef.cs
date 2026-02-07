using Project1.Core.Effects;
using Project1.Core.Base;

namespace Project1.Core.Needs
{
    public class EffectDef(string name, EntityEffectWorker worker) : Def(name)
    {
        internal EntityEffectWorker Worker = worker;
    }
}
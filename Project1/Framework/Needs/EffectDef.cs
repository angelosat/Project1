using Project1.Framework.Base;
using Project1.Framework.Effects;
using System;

namespace Project1.Framework.Needs
{
    public class EffectDef(string name, EntityEffectWorker worker) : Def(name)
    {
        internal EntityEffectWorker Worker = worker;
    }
}

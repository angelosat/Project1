using System;

namespace Start_a_Town_
{
    public class EffectDef(string name, EntityEffectWorker worker) : Def(name)
    {
        internal EntityEffectWorker Worker = worker;
    }
}

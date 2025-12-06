using System;

namespace Start_a_Town_
{
    public class EffectDef : Def
    {
        internal EntityEffectWorker Worker;

        public EffectDef(string name, EntityEffectWorker worker):base(name)
        {
            this.Worker = worker;
        }

        static public void Init()
        {
            Register(EffectDefOf.Sleeping);
        }
    }
}

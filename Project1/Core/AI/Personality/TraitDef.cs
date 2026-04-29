using Project1.Framework.Helpers;
using System;

namespace Project1.Core.AI.Personality
{
    public class TraitDef : Def
    {
        public string NameNegative, NamePositive, Description;
        public TraitWorker Worker;
        public TraitDef(string name) : base(name)
        {
        }
        public TraitDef(string name, Type traitWorker) : base(name)
        {
            this.Worker = ActivatorSafe<TraitWorker>.CreateInstance(traitWorker);
        }
    }
}

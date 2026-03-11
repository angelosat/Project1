using System;
using System.Collections.Generic;
using System.Text;

namespace Project1.Core.Systems.Consumables
{
    public class ConsumableDef(string name, string verb) : Def(name)
    {
        public string Verb = verb;
    }
}

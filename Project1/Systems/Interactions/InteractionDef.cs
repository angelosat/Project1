using System;

namespace Start_a_Town_
{
    public class InteractionDef : Def
    {
        public readonly Type InteractionClass;

        public InteractionDef(string name, Type interactionClass) : base(name)
        {
            this.InteractionClass = interactionClass;
        }
    }
}

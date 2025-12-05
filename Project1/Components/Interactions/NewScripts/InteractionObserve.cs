using System;

namespace Start_a_Town_.Components.Interactions
{
    class InteractionObserve : Interaction
    {
        public InteractionObserve():base("Observe", 4)
        {
            this.Animation = null;
        }

        public override void Perform()
        {
            throw new NotImplementedException();
        }

        public override object Clone()
        {
            return new InteractionObserve();
        }
    }
}

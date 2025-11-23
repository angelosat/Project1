using System;
using System.Collections.Generic;

namespace Start_a_Town_.Components.Interactions
{
    class InteractionObserve : Interaction
    {
        public InteractionObserve():base("Observe", 4)
        {
            this.Animation = null;
        }

        static readonly Dictionary<Need.Types, float> satisfyneed = new Dictionary<Need.Types, float>() { { Need.Types.Curiosity, 50 } };
        public override Dictionary<Need.Types, float> NeedSatisfaction => satisfyneed;
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

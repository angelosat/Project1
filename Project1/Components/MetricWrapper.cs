using System;
using System.Collections.Generic;
using System.Text;

namespace Start_a_Town_
{
    public abstract class MetricWrapper : Inspectable
    {
        private Actor Parent;
        public float DecayDelay, DecayDelayMax = 3;
        public int _value, _min, _max;
        public abstract void Tick();
    }
}

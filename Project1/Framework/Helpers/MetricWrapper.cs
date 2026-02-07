using Project1.Core.Entities;
using Project1.Core.Base;

namespace Project1.Core.Helpers
{
    public abstract class MetricWrapper: Inspectable
    {
        public Entity Owner;
        //public float DecayDelay, DecayDelayMax = 3;
        public int _value;//, Minn, Maxx;
        //public int Deficit => this.Maxx - this._value;
        public abstract void Tick();
    }
}

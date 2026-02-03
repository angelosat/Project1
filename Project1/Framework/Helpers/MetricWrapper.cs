using Start_a_Town_;

namespace Project1.Framework.Helpers
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

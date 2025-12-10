namespace Start_a_Town_
{
    public abstract class MetricWrapper: Inspectable
    {
        public Actor Parent;
        //public float DecayDelay, DecayDelayMax = 3;
        public int _value, Minn, Maxx;
        public abstract void Tick();
    }
}

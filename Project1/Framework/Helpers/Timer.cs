namespace Project1.Core.Helpers
{
    public class Timer
    {
        public int TickCount { get; private set; }
        public int Delay { get; private set; }

        public Timer(int delay)
        {
            Delay = delay;
            TickCount = 0;
        }

        public void Tick()
        {
            TickCount++;
        }

        public bool Fired => TickCount >= Delay;

        public void Reset()
        {
            TickCount = 0;
        }

        public void SetDelay(int newDelay)
        {
            Delay = newDelay;
        }
    }
}

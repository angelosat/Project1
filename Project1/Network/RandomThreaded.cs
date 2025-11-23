using System;

namespace Start_a_Town_.Net
{
    public class RandomThreaded
    {
        static Random Random;
        [ThreadStatic]
        static Random _Local;
        Random Local
        {
            get
            {
                if (_Local == null)
                {
                    int seed;
                    lock (Random) seed = Random.Next();
                    _Local = new Random(seed);
                    return _Local;
                }
                else
                {
                    Random old = _Local;
                    _Local = new Random(_Local.Next());
                    return old;
                }
            }
        }

        public RandomThreaded(Random random)
        {
            Random = random;
        }
        public double NextDouble()
        {
            return Local.NextDouble();
        }
        public int Next()
        {
            return Local.Next();
        }
        public int Next(int maxValue)
        {
            return Local.Next(maxValue);
        }
        public int Next(int minValue, int maxValue)
        {
            return Local.Next(minValue, maxValue);
        }
    }
}

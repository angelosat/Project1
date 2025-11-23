using System;
using System.Collections.Generic;

namespace Start_a_Town_.Net
{
    public class WorldSnapshot
    {
        public TimeSpan Time;
        public List<ObjectSnapshot> ObjectSnapshots = new();
        public WorldSnapshot(TimeSpan time)
        {
            this.Time = time;
        }
        public override string ToString()
        {
            return this.Time.ToString() + " States:" + ObjectSnapshots.Count;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Start_a_Town_.Net
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="time"></param>
    /// <param name="capacity">for initial array allocation</param>
    public class WorldSnapshot
    {
        public readonly TimeSpan Time;
        public IEnumerable<ObjectSnapshot> ObjectSnapshots => this.Dictionary.Values;
        private readonly Dictionary<int, ObjectSnapshot> _dic;
        public IReadOnlyDictionary<int, ObjectSnapshot> Dictionary => this._dic;
        //public WorldSnapshot(TimeSpan time, BinaryReader r)
        public WorldSnapshot(TimeSpan time, IDataReader r)
        {
            this.Time = time;
            var count = r.ReadInt32();
            this._dic = new(count);
            for (int i = 0; i < count; i++)
            {
                int redId = r.ReadInt32();
                var snap = new ObjectSnapshot(redId).Read(r);
                this._dic[snap.RefID] = snap;
            }
        }

        public override string ToString()
        {
            return this.Time.ToString() + " Snapshot Count:" + this.Dictionary.Count;
        }
    }
}

using Project1.Framework.Serialization;
using System.Collections.Generic;

namespace Project1.Core.Networking.Entities
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="time"></param>
    /// <param name="capacity">for initial array allocation</param>
    public class WorldSnapshot
    {
        //public readonly TimeSpan Time;
        public readonly Tick Tick;
        public IEnumerable<EntitySnapshot> ObjectSnapshots => this.Dictionary.Values;
        private readonly Dictionary<EntityRefId, EntitySnapshot> _dic;
        public IReadOnlyDictionary<EntityRefId, EntitySnapshot> Dictionary => this._dic;
        public WorldSnapshot(Tick time, IDataReader r)
        {
            this.Tick = time;
            var count = r.ReadInt32();
            this._dic = new(count);
            for (int i = 0; i < count; i++)
            {
                int redId = r.ReadInt32();
                var snap = new EntitySnapshot(redId).Read(r);
                this._dic[snap.RefID] = snap;
            }
        }

        public override string ToString()
            => $"{this.Tick} Snapshot Count: {this.Dictionary.Count}";
    }
}

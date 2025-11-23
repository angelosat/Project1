using System.Collections.Generic;

namespace Start_a_Town_
{
    public class ItemCategory : Def
    {
        public List<StorageFilter> Filters = new();
        public List<StatDef> Stats = new();
        public ItemCategory(string name) : base(name)
        {
        }
        public ItemCategory AddStats(params StatDef[] stats)
        {
            this.Stats.AddRange(stats);
            return this;
        }
        
        public override string ToString()
        {
            return this.Name;
        }
    }
}

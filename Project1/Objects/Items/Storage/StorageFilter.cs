using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public class StorageFilter : Def, ILabeled
    {
        Func<Entity, bool> _Condition;
        public StorageFilter(string name):base(name)
        {

        }
        public StorageFilter(string name, Func<Entity, bool> cond) : base(name)
        {
               this._Condition = cond;
        }
        public virtual bool Condition(Entity obj)
        {
            return this._Condition(obj);
        }
        public override string ToString()
        {
            return this.Name;
        }

        static public StorageFilter CreateFromItemDef(ItemDef def)
        {
            return new StorageFilter("StorageFilterFromItemDef:" + def.Name, item => item.Def == def);// { Label = def.Name };
        }
        static public HashSet<StorageFilter> CreateFilterSet()
        {
            var filters = Def.Database.Values.OfType<StorageFilter>();
            return new HashSet<StorageFilter>(filters);
        }
        static StorageFilter()
        {
            var itemByDef = Def.Database.Values.OfType<ItemDef>().GroupBy(d => d.Category);
            foreach(var group in itemByDef)
            {
                if (group.Key == null)
                    continue;
                foreach (var def in group)
                {
                    var filter = CreateFromItemDef(def);
                    Register(filter);
                    group.Key.Filters.Add(filter);
                }
            }
        }
    }
}

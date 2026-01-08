using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    internal class StockpileSettings(Stockpile stockpile)
    {
        readonly Stockpile Stockpile = stockpile;
        
        public readonly HashSet<(Def Profile, MaterialDef Material)> FiltersNew = [];
        public readonly HashSet<ItemDef> FiltersBase = [];

        public void Toggle(ItemDef item, Def profile, MaterialDef material)
        {
            if (profile is null && material is null)
            {
                if (!this.FiltersBase.Remove(item))
                    this.FiltersBase.Add(item);
            }
            else if (material is null)
            {
                if (!this.FiltersNew.Remove((profile, null)))
                    this.FiltersNew.Add((profile, null));
            }
            else
            {
                var tuple = (profile, material);
                if (!this.FiltersNew.Remove(tuple))
                    this.FiltersNew.Add(tuple);
            }
            this.Stockpile.Map.Events.Post(new StockpileUpdatedEvent(this.Stockpile));
        }
        public bool IsAllowed(ItemDef itemDef) => !this.FiltersBase.Contains(itemDef);

        public bool IsAllowed(Def def) => def switch
        {
            ItemDef itemDef => !this.FiltersBase.Contains(itemDef),
            Def profileDef => !this.FiltersNew.Contains((profileDef, null)),
            _ => throw new ArgumentException()
        };
        public bool IsAllowed(Def profile, MaterialDef material) => !this.FiltersNew.Contains((profile, null)) && !this.FiltersNew.Contains((profile, material));
    }
}

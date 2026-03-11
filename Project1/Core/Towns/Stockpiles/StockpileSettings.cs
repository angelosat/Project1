using Project1.Core.Entities;
using Project1.Core.Systems.Materials;
using System;
using System.Collections.Generic;

namespace Project1.Core.Towns.Stockpiles
{
    internal class StockpileSettings()
    {
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
        }
        public bool IsAllowed(ItemDef itemDef) => !this.FiltersBase.Contains(itemDef);

        public bool IsAllowed(Def def) => def switch
        {
            ItemDef itemDef => !this.FiltersBase.Contains(itemDef),
            Def profileDef => !this.FiltersNew.Contains((profileDef, null)),
            _ => throw new ArgumentException()
        };
        public bool IsAllowed(Def profile, MaterialDef material) => !this.FiltersNew.Contains((profile, null)) && !this.FiltersNew.Contains((profile, material));
        public bool Accepts(Entity entity) => 
            !this.FiltersBase.Contains(entity.Def) && 
            !this.FiltersNew.Contains((entity.Profile, null)) && 
            !this.FiltersNew.Contains((entity.Profile, entity.PrimaryMaterial));
    }
}

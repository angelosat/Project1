using Start_a_Town_;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Start_a_Town_
{
    public class StorageManager : TownComponent
    {
        readonly string _name = "Storage";
        public override string Name => this._name;

        readonly ObservableCollection<ItemMaterialAmount> CacheObservable = new();
        readonly Dictionary<ItemDef, Dictionary<MaterialDef, ItemMaterialAmount>> Cache = new();
        readonly StorageItemBranch RootNode = new();
        static readonly int TicksPerUpdate = Ticks.PerSecond;
        int UpdateTimer;

        public StorageManager(Town town) : base(town)
        {
        }

        public override void Update()
        {
            if (this.UpdateTimer-- <= 0)
            {
                this.UpdateTimer = TicksPerUpdate;
                foreach (var s in this.Town.ZoneManager.GetZones<Stockpile>())
                    s.CacheContentsNew();
                this.Refresh();
            }
        }

        internal IEnumerable<(Entity item, int amount)> FindItems(Func<Entity, bool> filter, int amount)
        {
            throw new System.NotImplementedException();
        }

        private void Refresh()
        {
            var stockpiles = this.Town.ZoneManager.GetZones<Stockpile>();
            var sum = stockpiles
                .SelectMany(s => s.CacheObservable)
                .GroupBy(i => i.Def)
                .Select(o => new
                {
                    def = o.Key,
                    matSums = o
                    .GroupBy(oo => oo.PrimaryMaterial)
                    .ToDictionary(m => m.Key, m => m.Sum(o => o.StackSize))
                })
                .ToDictionary(o => o.def, o => o.matSums);


            foreach (var def in this.Cache.ToList())
            {
                if (!sum.ContainsKey(def.Key))
                {
                    foreach (var i in this.Cache[def.Key])
                    {
                        this.CacheObservable.Remove(i.Value);
                        this.RootNode.Remove(def.Key);
                    }
                    this.Cache.Remove(def.Key);
                }
                else
                {
                    var mats = sum[def.Key];
                    foreach (var mat in def.Value.ToList())
                    {
                        var itemAmounts = this.Cache[def.Key];
                        if (!mats.TryGetValue(mat.Key, out var amount))
                        {
                            var cached = itemAmounts[mat.Key];
                            this.CacheObservable.Remove(cached);
                            this.RootNode[def.Key].Remove(mat.Key);
                            itemAmounts.Remove(mat.Key);
                        }
                    }
                }
            }

            foreach (var iDef in sum)
            {
                if (!this.Cache.TryGetValue(iDef.Key, out var mats))
                {
                    mats = new();
                    this.Cache.Add(iDef.Key, mats);
                    this.RootNode.Add(iDef.Key);
                }
                foreach (var mat in iDef.Value)
                {
                    if (!mats.TryGetValue(mat.Key, out var itemAmount))
                    {
                        itemAmount = new(iDef.Key, mat.Key, mat.Value);
                        mats.Add(mat.Key, itemAmount);
                        this.CacheObservable.Add(itemAmount);
                        this.RootNode[iDef.Key].Add(itemAmount);
                    }
                    else
                    {
                        itemAmount.Amount = mat.Value;
                        this.RootNode[iDef.Key][mat.Key].Item.Amount = mat.Value;
                    }
                }
            }
            this.RootNode.UpdateSum();
        }
        internal override void OnHudCreated(Hud hud)
        {
            hud.AddControls(this.getGui());
        }
        Control getGui()
        {
            var list = new ListCollapsibleObservable(this.RootNode, true);
            return list;
        }
        class StorageItemBranch : IListCollapsibleDataSourceObservable
        {
            int Sum;

            public StorageItemBranch this[ItemDef branch] => (StorageItemBranch)this.Branches.First(c => ((StorageItemBranch)c).ItemDef == branch);
            public StorageItemLeaf this[MaterialDef leaf] => (StorageItemLeaf)this.Leafs.First(c => ((StorageItemLeaf)c).Key == leaf);

            ItemDef ItemDef;
            readonly HashSet<StorageItemBranch> _branches = new();
            readonly HashSet<StorageItemLeaf> _leafs = new();

            readonly ObservableCollection<IListCollapsibleDataSourceObservable> Branches = new();
            readonly ObservableCollection<IListable> Leafs = new();

            public string Label => this.ItemDef.Label;

            public ObservableCollection<IListCollapsibleDataSourceObservable> ListBranches => this.Branches;// new(this.Branches.Cast<IListCollapsibleDataSourceObservable>());
            public ObservableCollection<IListable> ListLeafs => this.Leafs;// new(this.Leafs.Cast<IListable>());

            public bool Remove(ItemDef item)
            {
                var b = this.Branches.First(i => ((StorageItemBranch)i).ItemDef == item);
                this._branches.Remove(b as StorageItemBranch);
                return this.Branches.Remove(b);
            }
            public void Add(ItemDef item)
            {
                var b = new StorageItemBranch() { ItemDef = item };
                this._branches.Add(b);
                this.Branches.Add(b);
            }
            public bool Remove(MaterialDef item)
            {
                var leaf = this.Leafs.First(i => ((StorageItemLeaf)i).Key == item);
                this._leafs.Remove(leaf as StorageItemLeaf);
                return this.Leafs.Remove(leaf);
            }
            public void Add(ItemMaterialAmount item)
            {
                var leaf = new StorageItemLeaf() { Key = item.Material, Item = item };
                this._leafs.Add(leaf);
                this.Leafs.Add(leaf);
            }
            public Control GetGui()
            {
                return new ListCollapsibleObservable(this, true);
            }
            internal void UpdateSum()
            {
                this.Sum = this._leafs.Sum(l => l.Item.Amount);
                foreach (var b in this._branches)
                    b.UpdateSum();
            }
            public Control GetListControlGui()
            {
                // TODO instead of calculating sum every frame, store it in a field update it only when refreshing storemanager cache
                //return new Label(() => $"{this.InnerItems.Sum(l => l.Item.Amount)}x {this.Label}");
                return new Label(() => $"{this.Sum}x {this.Label}");

            }
            public override string ToString()
            {
                return this.ItemDef is ItemDef def ? $"{def.Label}: {this.Leafs.Count}" : $"Root: {this.Branches.Count}";
            }
        }

        class StorageItemLeaf : IListable
        {
            public MaterialDef Key;
            public ItemMaterialAmount Item;

            public string Label => this.Item.Label;

            public Control GetListControlGui()
            {
                return this.Item.GetListControlGui();
            }
            public override string ToString()
            {
                return this.Item.ToString();
            }
        }
    }
}

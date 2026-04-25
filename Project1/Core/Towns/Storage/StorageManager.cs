using Project1.Core.Entities;
using Project1.Core.Legacy;
using Project1.Core.Systems.Materials;
using Project1.Core.UI;
using Project1.Core.UI.Hud;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Project1.Core.Towns.Storage;

public class StorageManager : TownComp
{
    readonly string _name = "Storage";
    public override string Name => this._name;

    readonly ObservableCollection<ItemMaterialAmount> CacheObservable = new();
    readonly Dictionary<ItemDef, Dictionary<MaterialDef, ItemMaterialAmount>> Cache = new();
    readonly StorageItemBranch RootNode = new();

    public StorageManager(Town town) : base(town)
    {
    }

    internal IEnumerable<(Entity item, int amount)> FindItems(Func<Entity, bool> filter, int amount)
    {
        throw new System.NotImplementedException();
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

        public string LabelReadable => this.ItemDef.LabelReadable;

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
            return new Label(() => $"{this.Sum}x {this.LabelReadable}");

        }
        public override string ToString()
        {
            return this.ItemDef is ItemDef def ? $"{def.LabelReadable}: {this.Leafs.Count}" : $"Root: {this.Branches.Count}";
        }
    }

    class StorageItemLeaf : IListable
    {
        public MaterialDef Key;
        public ItemMaterialAmount Item;

        public string LabelReadable => this.Item.LabelReadable;

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

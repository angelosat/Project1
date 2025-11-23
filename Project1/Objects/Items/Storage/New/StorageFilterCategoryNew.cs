using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class StorageFilterCategoryNewNew : IListCollapsibleDataSource, IListable, ILabeled
    {
        public string Label { get; set; }
        IStorageNew _owner;
        public IStorageNew Owner
        {
            get => this.Root._owner;
            set
            {
                if (this.Root != this)
                    throw new Exception();
                this._owner = value;
            }
        }
        public ItemCategory Category;
        public ItemDef Item;
        public StorageFilterCategoryNewNew Parent;
        public StorageFilterCategoryNewNew Root => this.Parent?.Root ?? this;
        public List<StorageFilterCategoryNewNew> Branches = new();
        public List<StorageFilterNewNew> Leaves = new();
        public IEnumerable<IListCollapsibleDataSource> ListBranches => this.Branches;
        public IEnumerable<IListable> ListLeafs => this.Leaves;
        public StorageFilterCategoryNewNew(ItemDef def)
        {
            this.Item = def;
            this.Label = def.Label;
        }
        public StorageFilterCategoryNewNew(string label)
        {
            this.Label = label;
        }
        public StorageFilterCategoryNewNew AddChildren(IEnumerable<StorageFilterCategoryNewNew> cats)
        {
            return this.AddChildren(cats.ToArray());
        }

        public StorageFilterCategoryNewNew AddChildren(params StorageFilterCategoryNewNew[] cats)
        {
            foreach (var c in cats)
            {
                c.Parent = this;
                //c.Owner = this.Owner;
            }
            this.Branches.AddRange(cats);
            return this;
        }
        public StorageFilterCategoryNewNew AddLeafs(IEnumerable<StorageFilterNewNew> filters)
        {
            return this.AddLeafs(filters.ToArray());
        }
        public StorageFilterCategoryNewNew AddLeafs(params StorageFilterNewNew[] filters)
        {
            foreach (var c in filters)
            {
                c.Parent = this;
                //c.Owner = this.Owner;
            }
            this.Leaves.AddRange(filters);
            return this;
        }
        internal bool FindNodeIndex(StorageFilterCategoryNewNew c, out int i)
        {
            i = 0;
            foreach (var item in this.GetAllDescendantNodes())
            {
                if (c == item)
                    return true;
                i++;
            }
            return false;
        }
        internal bool FindLeafIndex(StorageFilterNewNew c, out int i)
        {
            i = 0;
            foreach (var item in this.GetAllDescendantLeaves())
            {
                if (c == item)
                    return true;
                i++;
            }
            return false;
        }

        internal StorageFilterNewNew GetLeafByIndex(int i)
        {
            var n = 0;
            var enumerator = this.GetAllDescendantLeaves().GetEnumerator();
            do { enumerator.MoveNext(); } while (n++ != i);
            return enumerator.Current;
        }

        internal StorageFilterCategoryNewNew FindNode(ItemCategory cat)
        {
            return GetAllDescendantNodes().First(n => n.Category == cat);
        }

        internal StorageFilterCategoryNewNew GetNodeByIndex(int i)
        {
            var n = 0;
            var enumerator = this.GetAllDescendantNodes().GetEnumerator();
            do { enumerator.MoveNext(); } while (n++ != i);
            return enumerator.Current;
        }
        internal IEnumerable<StorageFilterNewNew> GetAllDescendantLeaves()
        {
            var queue = new Queue<StorageFilterCategoryNewNew>();
            queue.Enqueue(this);
            while (queue.Any())
            {
                var current = queue.Dequeue();
                foreach (var leaf in current.Leaves)
                    yield return leaf;
                foreach (var child in current.Branches)
                    queue.Enqueue(child);
            }
        }
        internal IEnumerable<StorageFilterCategoryNewNew> GetAllDescendantNodes()
        {
            var queue = new Queue<StorageFilterCategoryNewNew>();
            queue.Enqueue(this);
            while (queue.Any())
            {
                var current = queue.Dequeue();
                yield return current;
                foreach (var child in current.Branches)
                    queue.Enqueue(child);
            }
        }
        public Control GetGui()
        {
            var box = new ScrollableBoxTest(300, 400, ScrollModes.Vertical);
            var listcollapsible = new ListCollapsibleNew(this);
            box.AddControls(listcollapsible);
            return box;
        }
        //public Control GetGui(Action<int[], int[]> callback)
        //{
        //    var box = new ScrollableBoxTest(300, 400, ScrollModes.Vertical);
        //    var listcollapsible = new ListCollapsibleNew();
        //    listcollapsible.AddNode(createNode(this));
        //    listcollapsible.Build();
        //    box.AddControls(listcollapsible);
        //    return box;

        //    ListBoxCollapsibleNode createNode(StorageFilterCategoryNewNew cat)
        //    {
        //        var node = new ListBoxCollapsibleNode(cat.Label, n => new CheckBoxNew()
        //        {
        //            TickedFunc = () => isEnabled(cat),
        //            LeftClickAction = () =>
        //            {
        //                this.FindNodeIndex(cat, out var k);
        //                callback(new int[] { k }, null);
        //            }
        //        });
        //        foreach (var child in cat.Branches)
        //            node.AddNode(createNode(child));
        //        foreach (var leaf in cat.Leaves)
        //            node.AddLeaf(new CheckBoxNew(leaf.Label)
        //            {
        //                TickedFunc = () => leaf.Enabled,
        //                LeftClickAction = () => {
        //                    this.FindLeafIndex(leaf, out var i);
        //                    callback(null, new int[] { i });
        //                }
        //            });
        //        return node;
        //    }
        //    bool isEnabled(StorageFilterCategoryNewNew cat) => cat.Leaves.All(l => l.Enabled) && cat.Branches.All(isEnabled);
        //}

        public Control GetListControlGui()
        {
            return new CheckBoxNew()
            {
                TickedFunc = () => this.IsEnabled,
                LeftClickAction = () =>
                {
                    this.Root.FindNodeIndex(this, out var d);
                    if (this.Category is not null)
                        this.Owner.FiltersGuiCallback(this.Category);
                    else if (this.Item is not null)
                        this.Owner.FiltersGuiCallback(this.Item, null);
                    else
                        this.Owner.FiltersGuiCallback(null);
                }
            };
        }

        bool IsEnabled => this.Leaves.All(l => l.Enabled) && this.Branches.All(c => c.IsEnabled);

        public void SetOwner(IStorageNew owner)
        {
            this.Owner = owner;
        }
        public override string ToString()
        {
            return this.Category?.Name ?? this.Item.Name;
        }
    }
    [Obsolete]
    class StorageFilterCategoryNew : IListCollapsibleDataSource, IListable, ILabeled
    {
        public string Label { get; set; }
        Stockpile _owner;
        public Stockpile Owner
        {
            get => this.Root._owner;
            set
            {
                if (this.Root != this)
                    throw new Exception();
                this._owner = value;
            }
        }
        public StorageFilterCategoryNew Parent;
        public StorageFilterCategoryNew Root => this.Parent?.Root ?? this;
        public List<StorageFilterCategoryNew> Branches = new();
        public List<StorageFilterNew> Leaves = new();
        public IEnumerable<IListCollapsibleDataSource> ListBranches => this.Branches;
        public IEnumerable<IListable> ListLeafs => this.Leaves;

        public StorageFilterCategoryNew(string label)
        {
            this.Label = label;
        }
        public StorageFilterCategoryNew AddChildren(IEnumerable<StorageFilterCategoryNew> cats)
        {
            return this.AddChildren(cats.ToArray());
        }

        public StorageFilterCategoryNew AddChildren(params StorageFilterCategoryNew[] cats)
        {
            foreach (var c in cats)
            {
                c.Parent = this;
                //c.Owner = this.Owner;
            }
            this.Branches.AddRange(cats);
            return this;
        }
        public StorageFilterCategoryNew AddLeafs(IEnumerable<StorageFilterNew> filters)
        {
            return this.AddLeafs(filters.ToArray());
        }
        public StorageFilterCategoryNew AddLeafs(params StorageFilterNew[] filters)
        {
            foreach (var c in filters)
            {
                c.Parent = this;
                //c.Owner = this.Owner;
            }
            this.Leaves.AddRange(filters);
            return this;
        }
        internal bool FindNodeIndex(StorageFilterCategoryNew c, out int i)
        {
            i = 0;
            foreach (var item in this.GetAllDescendantNodes())
            {
                if (c == item)
                    return true;
                i++;
            }
            return false;
        }
        internal bool FindLeafIndex(StorageFilterNew c, out int i)
        {
            i = 0;
            foreach (var item in this.GetAllDescendantLeaves())
            {
                if (c == item)
                    return true;
                i++;
            }
            return false;
        }

        internal StorageFilterNew GetLeafByIndex(int i)
        {
            var n = 0;
            var enumerator = this.GetAllDescendantLeaves().GetEnumerator();
            do { enumerator.MoveNext(); } while (n++ != i);
            return enumerator.Current;
        }
        internal StorageFilterCategoryNew GetNodeByIndex(int i)
        {
            var n = 0;
            var enumerator = this.GetAllDescendantNodes().GetEnumerator();
            do { enumerator.MoveNext(); } while (n++ != i);
            return enumerator.Current;
        }
        internal IEnumerable<StorageFilterNew> GetAllDescendantLeaves()
        {
            var queue = new Queue<StorageFilterCategoryNew>();
            queue.Enqueue(this);
            while (queue.Any())
            {
                var current = queue.Dequeue();
                foreach (var leaf in current.Leaves)
                    yield return leaf;
                foreach (var child in current.Branches)
                    queue.Enqueue(child);
            }
        }
        internal IEnumerable<StorageFilterCategoryNew> GetAllDescendantNodes()
        {
            var queue = new Queue<StorageFilterCategoryNew>();
            queue.Enqueue(this);
            while (queue.Any())
            {
                var current = queue.Dequeue();
                yield return current;
                foreach (var child in current.Branches)
                    queue.Enqueue(child);
            }
        }
        public Control GetGui()
        {
            var box = new ScrollableBoxNewNew(300, 400, ScrollModes.Vertical);
            var listcollapsible = new ListCollapsibleNew(this);
            box.AddControls(listcollapsible);
            return box;
        }
        public Control GetGui(Action<int[], int[]> callback)
        {
            var box = new ScrollableBoxNewNew(300, 400, ScrollModes.Vertical);
            var listcollapsible = new ListCollapsibleNew();
            listcollapsible.AddNode(createNode(this));
            listcollapsible.Build();
            box.AddControls(listcollapsible);
            return box;

            ListBoxCollapsibleNode createNode(StorageFilterCategoryNew cat)
            {
                var node = new ListBoxCollapsibleNode(cat.Label, n => new CheckBoxNew()
                {
                    TickedFunc = () => isEnabled(cat),
                    LeftClickAction = () =>
                    {
                        this.FindNodeIndex(cat, out var k);
                        callback(new int[] { k }, null);
                    }
                });
                foreach (var child in cat.Branches)
                    node.AddNode(createNode(child));
                foreach (var leaf in cat.Leaves)
                    node.AddLeaf(new CheckBoxNew(leaf.Label)
                    {
                        TickedFunc = () => leaf.Enabled,
                        LeftClickAction = () => {
                            this.FindLeafIndex(leaf, out var i);
                            callback(null, new int[] { i });
                        }
                    });
                return node;
            }
            bool isEnabled(StorageFilterCategoryNew cat) => cat.Leaves.All(l => l.Enabled) && cat.Branches.All(isEnabled);
        }

        public bool Filter(Entity obj)
        {
            return this.Leaves.Any(f => f.Enabled && f.Condition(obj)) || this.Branches.Any(c => c.Filter(obj));
        }

        public Control GetListControlGui()
        {
            return new CheckBoxNew()
            {
                TickedFunc = () => this.IsEnabled,
                LeftClickAction = () =>
                {
                    this.Root.FindNodeIndex(this, out var d);
                    PacketStorageFiltersNew.Send(this.Owner, new int[] { d }, null);
                }
            };
        }
        
        bool IsEnabled => this.Leaves.All(l => l.Enabled) && this.Branches.All(c => c.IsEnabled);

        public void UpdateOwner(Stockpile newOwner)
        {

        }

        public void SetOwner(Stockpile owner)
        {
            throw new NotImplementedException();
        }
    }
}

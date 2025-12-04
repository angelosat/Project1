using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Start_a_Town_
{
    public class StorageSettings : ISerializable, ISaveable
    {
        public readonly Dictionary<ItemDef, ItemFilter> Allowed = new();
        public StoragePriority Priority = StoragePriority.Normal;
        public StorageFilterCategoryNewNew FiltersView;
        
        public StorageSettings Initialize(StorageFilterCategoryNewNew root)
        {
            this.FiltersView = root;
            // populate the allowed list with all item variations
            foreach (var c in root.GetAllDescendantLeaves().GroupBy(l => l.Item))
                this.Allowed.Add(c.Key, new(c.Key));
            return this;
        }

        internal void Toggle(ItemDef item, Def variator)
        {
            if (variator is not IItemDefVariator)
                throw new Exception();
            var record = this.Allowed[item];
            record.Toggle(variator);
        }
        internal void Toggle(ItemDef item, MaterialDef mat)
        {
            var record = this.Allowed[item];
            record.Toggle(mat);
        }
        internal void Toggle(ItemDef item)
        {
            var record = this.Allowed[item];
            if (item.StorageFilterVariations is not null)
            {
                var minor = item.StorageFilterVariations.GroupBy(a => record.IsAllowed(a as Def)).OrderBy(a => a.Count()).First();
                foreach (var m in minor)
                    record.Toggle(m as Def);
            }
            else if (item.DefaultMaterialType is not null)
            {
                var minor = item.DefaultMaterialType.SubTypes.GroupBy(a => record.IsAllowed(a)).OrderBy(a => a.Count()).First();
                foreach (var m in minor)
                    record.Toggle(m);
            }
            else
                record.Toggle();
        }
        internal void Toggle(ItemCategory cat)
        {
            IEnumerable<ItemFilter> records = null;
            if (cat is null)
                records = this.Allowed.Values;
            else
            {
                var byCategory = this.Allowed.Values.ToLookup(r => r.Item.Category);
                records = byCategory[cat];
            }
            var catNode = FiltersView.FindNode(cat);
            var leafs = catNode.GetAllDescendantLeaves();
            var leafsMinor = leafs.GroupBy(l => l.Enabled).OrderBy(l => l.Count()).First();
            foreach (var l in leafsMinor)
            {
                if (l.Variation is not null)
                    this.Toggle(l.Item, l.Variation);
                else if (l.Material is not null)
                    this.Toggle(l.Item, l.Material);
                else
                    this.Toggle(l.Item);
            }
        }

        public bool Accepts(ItemDef item, MaterialDef mat, Def variation)
        {
            var record = this.Allowed[item];
            return record.IsAllowed(mat) && record.IsAllowed(variation);
        }
        public bool Accepts(Entity obj)
        {
            if (!this.Allowed.TryGetValue(obj.Def, out ItemFilter filter))
                return false;
            return filter.IsAllowed(obj.PrimaryMaterial);
        }

        public void Write(BinaryWriter w)
        {
            w.Write((byte)this.Priority);
            this.Allowed.Values.Sync(w);
        }

        public ISerializable Read(IDataReader r)
        {
            this.Priority = (StoragePriority)r.ReadByte();
            this.Allowed.Values.Sync(r);
            return this;
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.Allowed.Values.SaveNewBEST(tag, "Filters");
            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            tag.TryGetTag("Filters", t =>
            {
                var list = t.LoadList<ItemFilter>();
                foreach (var r in list)
                {
                    if (r is null) // in case an itemdef has been changed/removed
                        continue;
                    this.Allowed[r.Item].CopyFrom(r);
                }
            });
            return this;
        }

        internal void SetAllowed(ItemCategory cat, bool value)
        {
            foreach (var item in Allowed.Where(d => d.Key.Category == cat))
            {
                item.Value.Enabled = value;
            }
        }
    }
}

using System.Collections.Generic;
using System.IO;

namespace Start_a_Town_
{
    public record ItemFilter : ISaveable, ISerializable, ISyncable
    {
        public ItemDef Item { get; private set; }
        public bool Enabled = true;
        readonly HashSet<MaterialDef> DisallowedMaterials = new(); // TODO make materials implement iitemdefvariator too?
        readonly HashSet<Def> DisallowedVariations = new();
        public ItemFilter()
        {

        }
        public ItemFilter(ItemDef item, bool enabled = true)
        {
            this.Item = item;
            this.Enabled = enabled;
        }
        public bool IsAllowed(Entity item)
        {
            if (item.Def.DefaultMaterialType is not null)
                return !this.DisallowedMaterials.Contains(item.PrimaryMaterial);
            else if (item.Def.StorageFilterVariations is not null)
                return !this.DisallowedVariations.Contains(item.Def.VariationGetter(item));
            else
                return this.Enabled;
        }
        public bool IsAllowed(MaterialDef mat)
        {
            return this.Enabled && !this.DisallowedMaterials.Contains(mat);
        }
        public bool IsAllowed(Def v)
        {
            return this.Enabled && !this.DisallowedVariations.Contains(v);
        }
        internal void Toggle()
        {
            this.Enabled = !this.Enabled;
        }
        internal void Toggle(MaterialDef mat)
        {
            if (this.DisallowedMaterials.Contains(mat))
                this.DisallowedMaterials.Remove(mat);
            else
                this.DisallowedMaterials.Add(mat);
        }
        internal void Toggle(Def v)
        {
            if (this.DisallowedVariations.Contains(v))
                this.DisallowedVariations.Remove(v);
            else
                this.DisallowedVariations.Add(v);
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.Item.Save(tag, "Item");
            this.Enabled.Save(tag, "Enabled");
            this.DisallowedMaterials.SaveDefs(tag, "Materials");
            this.DisallowedVariations.SaveDefs(tag, "Variations");
            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            this.Item = tag.LoadDef<ItemDef>("Item");
            if (this.Item is null)
                return null;
            this.Enabled = (bool)tag["Enabled"].Value;
            this.Enabled |= this.Item.DefaultMaterialType is not null || this.Item.StorageFilterVariations is not null; // FAILSAFE because 'enabled' can only be set to false for itemdefs that are leafs (don't have any sub-variations)
            this.DisallowedMaterials.TryLoadDefs(tag, "Materials");
            this.DisallowedVariations.TryLoadDefs(tag, "Variations");
            return this;
        }

        public void Write(BinaryWriter w)
        {
            this.Item.Write(w);
            this.Sync(w);
        }

        public ISerializable Read(IDataReader r)
        {
            this.Item = Def.GetDef<ItemDef>(r.ReadString());
            this.Sync(r);
            return this;
        }

        public ISyncable Sync(BinaryWriter w)
        {
            w.Write(this.Enabled);
            this.DisallowedMaterials.WriteDefs(w);
            this.DisallowedVariations.WriteDefs(w);
            return this;
        }

        public ISyncable Sync(IDataReader r)
        {
            this.Enabled = r.ReadBoolean();
            this.DisallowedMaterials.ReadDefs(r);
            this.DisallowedVariations.ReadDefs(r);
            return this;
        }

        public void CopyFrom(ItemFilter toCopy)
        {
            this.Enabled = toCopy.Enabled;
            foreach (var m in toCopy.DisallowedMaterials)
                this.DisallowedMaterials.Add(m);
            foreach (var m in toCopy.DisallowedVariations)
                this.DisallowedVariations.Add(m);
        }
    }
}

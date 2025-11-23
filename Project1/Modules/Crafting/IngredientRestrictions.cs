using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Start_a_Town_
{
    public class IngredientRestrictions : ISaveable, ISerializable
    {
        public HashSet<MaterialDef> Material = new();
        public HashSet<MaterialTypeDef> MaterialType = new();
        public HashSet<ItemDef> ItemDef = new();
        public IngredientRestrictions()
        {

        }
        public IngredientRestrictions(IngredientRestrictions source)
        {
            this.Material = new(source.Material);
            this.ItemDef = new(source.ItemDef);
            this.MaterialType = new(source.MaterialType);

        }
        public bool IsRestricted(MaterialDef item)
        {
            return this.Material.Contains(item);
        }
        public bool IsRestricted(MaterialTypeDef item)
        {
            return this.MaterialType.Contains(item);
        }
        public bool IsRestricted(ItemDef item)
        {
            return this.ItemDef.Contains(item);
        }

        public IngredientRestrictions Restrict(MaterialDef mat)
        {
            this.Material.Add(mat);
            return this;
        }
        public IngredientRestrictions Restrict(MaterialTypeDef mattype)
        {
            foreach (var m in mattype.SubTypes)
                this.Restrict(m);
            return this;
        }
        public IngredientRestrictions Restrict(ItemDef item)
        {
            this.ItemDef.Add(item);
            return this;
        }

        public void Toggle(MaterialDef item)
        {
            if (!this.Material.Contains(item))
                this.Material.Add(item);
            else
                this.Material.Remove(item);
        }
        public void Toggle(MaterialTypeDef item)
        {
            if (!this.MaterialType.Contains(item))
                this.MaterialType.Add(item);
            else
                this.MaterialType.Remove(item);
        }
        public void Toggle(ItemDef item)
        {
            if (!this.ItemDef.Contains(item))
                this.ItemDef.Add(item);
            else
                this.ItemDef.Remove(item);
        }

        internal void ToggleRestrictions(ItemDef[] defs, MaterialDef[] mats, MaterialTypeDef[] matTypes)
        {
            for (int i = 0; i < defs.Length; i++)
            {
                this.Toggle(defs[i]);
            }
            for (int i = 0; i < mats.Length; i++)
            {
                this.Toggle(mats[i]);
            }
            for (int i = 0; i < matTypes.Length; i++)
            {
                this.Toggle(matTypes[i]);
            }
        }

        internal bool IsRestricted(Entity item)
        {
            return
                this.ItemDef.Contains(item.Def) ||
                this.Material.Contains(item.Body.Material) || //will i have cases with multiple bones of different materials? find dominant material? or use the first bone's material?
                this.MaterialType.Contains(item.Body.Material.Type);
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(this.ItemDef.Select(d => d.Name).Save("ItemDef"));
            tag.Add(this.Material.Select(d => d.Name).Save("Material"));
            tag.Add(this.MaterialType.Select(d => d.Name).Save("MaterialType"));
            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            this.ItemDef.TryLoadDefs(tag, "Material");
            this.Material.TryLoadDefs(tag, "Material");
            this.MaterialType.TryLoadDefs(tag, "MaterialType");
            return this;
        }


        public void Write(BinaryWriter w)
        {
            w.Write(this.ItemDef.Select(d => d.Name).ToArray());
            w.Write(this.Material.Select(d => d.Name).ToArray());
            w.Write(this.MaterialType.Select(d => d.Name).ToArray());
        }

        public ISerializable Read(BinaryReader r)
        {
            this.ItemDef = new HashSet<ItemDef>(r.ReadStringArray().Select(Def.GetDef<ItemDef>));
            this.Material = new HashSet<MaterialDef>(r.ReadStringArray().Select(Def.GetDef<MaterialDef>));
            this.MaterialType = new HashSet<MaterialTypeDef>(r.ReadStringArray().Select(Def.GetDef<MaterialTypeDef>));
            return this;
        }
    }
}

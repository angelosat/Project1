using Start_a_Town_.UI;
using System.IO;

namespace Start_a_Town_
{
    public class ItemMaterialAmount : Inspectable, ISerializable, ISaveable, IListable
    {
        public ItemDef Item;
        public MaterialDef Material;
        public int Amount;

        public override string Label => $"{this.Amount}x {this.Material.Label} {this.Item.Label}";

        public ItemMaterialAmount()
        {

        }
        public ItemMaterialAmount(ItemDef def, MaterialDef material, int amount)
        {
            Item = def;
            Material = material;
            this.Amount = amount;
        }

        public ItemMaterialAmount(IDataReader r)
        {
            this.Read(r);
        }
        public ItemMaterialAmount(SaveTag s)
        {
            this.Load(s);
        }

        internal Entity Create()
        {
            return ItemFactory.CreateFrom(this.Item, this.Material).SetStackSize(this.Amount) as Entity;
        }

        public override string ToString()
        {
            return this.GetText();// GetText(this.Def, this.Material, this.Amount);
        }
        static public string GetText(ItemDef def, MaterialDef material, int amount)
        {
            return $"{amount}x {material.Label} {def.Label}"; // TODO add a method to itemdefs that return the final name of the item depending on materials etc
        }
        public string GetText()
        {
            return $"{this.Amount}x {this.Material.Label} {this.Item.Label}"; // TODO add a method to itemdefs that return the final name of the item depending on materials etc
        }
        public void Save(SaveTag save, string name)
        {
            save.Add(this.Save(name));
        }
        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(this.Item.Name.Save("DefName"));
            this.Material.Save(tag, "Material");
            tag.Add(this.Amount.Save("Amount"));
            return tag;
        }
        public ISaveable Load(SaveTag tag)
        {
            this.Item = tag.LoadDef<ItemDef>("DefName");
            this.Material = tag.LoadDef<MaterialDef>("Material");
            this.Amount = tag.GetValue<int>("Amount");
            return this;
        }
        public void Write(BinaryWriter w)
        {
            w.Write(this.Item.Name);
            w.Write(this.Material.Name);
            w.Write(this.Amount);
        }
        public ISerializable Read(IDataReader r)
        {
            this.Item = Start_a_Town_.Def.GetDef<ItemDef>(r.ReadString());
            this.Material = Start_a_Town_.Def.GetDef<MaterialDef>(r.ReadString());
            this.Amount = r.ReadInt32();
            return this;
        }

        public Control GetListControlGui()
        {
            return new Label(this.GetText);
        }
    }
}

using Project1.Core.Entities;
using Project1.Core.Base;
using Project1.Core.Helpers;
using Project1.Core.Interfaces;
using Project1.Framework.IO;

namespace Project1.Core.Legacy
{
    public class ItemDefAmount : ISaveable, ISerializableNew<ItemDefAmount>
    {
        public ItemDef Def;
        public int Amount;
        public ItemDefAmount()
        {

        }
        public ItemDefAmount(ItemDef def, int amount)
        {
            this.Def = def;
            this.Amount = amount;
        }

        public override string ToString()
        {
            return GetText(this.Def, this.Amount);
        }
        static public string GetText(ItemDef def, int amount)
        {
            return string.Format("{0}x {1}", amount, def.LabelReadable); // TODO add a method to itemdefs that return the final name of the item depending on materials etc
        }
        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(this.Def.Name.Save("Def"));
            tag.Add(this.Amount.Save("Amount"));
            return tag;
        }
        public ISaveable Load(SaveTag tag)
        {
            this.Def = Project1.Core.Base.Def.GetDef<ItemDef>(tag.GetValue<string>("Def"));
            this.Amount = tag.GetValue<int>("Amount");
            return this;
        }
        public void Write(IDataWriter w)
        {
            w.Write(this.Def.Name);
            w.Write(this.Amount);
        }
        public ItemDefAmount Read(IDataReader r)
        {
            this.Def = Project1.Core.Base.Def.GetDef<ItemDef>(r.ReadString());
            this.Amount = r.ReadInt32();
            return this;
        }

        public static ItemDefAmount Create(IDataReader r) => new ItemDefAmount().Read(r);
    }
}

using System.IO;

namespace Start_a_Town_
{
    public struct Transaction
    {
        public enum Types { Buy, Sell }
        public int Customer;
        public int Item;
        public int Cost;
        public Types Type;

        public Transaction(Actor customer, Types type, Entity item, int cost)
        {
            Customer = customer.RefID;
            Type = type;
            Item = item.RefID;
            Cost = cost;
        }

        public Transaction(int customer, int item, int cost, Types type)
        {
            Customer = customer;
            Item = item;
            Cost = cost;
            Type = type;
        }
        public Transaction(BinaryReader r)
        {
            this.Type = (Types)r.ReadInt32();
            this.Customer = r.ReadInt32();
            this.Item = r.ReadInt32();
            this.Cost = r.ReadInt32();
        }
        public Transaction(SaveTag load)
        {
            this.Type = (Types)load.GetValue<int>("Type");
            this.Customer = load.GetValue<int>("Customer");
            this.Item = load.GetValue<int>("Item");
            this.Cost = load.GetValue<int>("Cost");
        }
        public void Write(BinaryWriter w)
        {
            w.Write((int)this.Type);
            w.Write(this.Customer);
            w.Write(this.Item);
            w.Write(this.Cost);
        }
        public SaveTag Sate(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(new SaveTag(SaveTag.Types.Int, "Type", (int)this.Type));
            this.Customer.Save(tag, "Customer");
            this.Item.Save(tag, "Item");
            this.Cost.Save(tag, "Cost");
            return tag;
        }
        public void Save(SaveTag save, string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(new SaveTag(SaveTag.Types.Int, "Type", (int)this.Type));
            this.Customer.Save(tag, "Customer");
            this.Item.Save(tag, "Item");
            this.Cost.Save(tag, "Cost");
            save.Add(tag);
        }
    }
}

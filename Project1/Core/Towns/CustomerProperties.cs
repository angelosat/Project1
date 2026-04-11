using Project1.Framework;
using Project1.Framework.Serialization;

namespace Project1.Core.Towns
{
    public abstract class CustomerProperties : ISaveable, ISerializable
    {
        public EntityRefId CustomerID;
        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            //this.CustomerID.Save(tag, "CustomerID");
            tag.Save("CustomerId", this.CustomerID);
            this.SaveExtra(tag);
            return tag;
        }
        protected virtual void SaveExtra(SaveTag save) { }
        public ISaveable Load(SaveTag tag)
        {
            this.CustomerID = tag.GetValue<int>("CustomerID");
            this.LoadExtra(tag);
            return this;
        }
        protected virtual void LoadExtra(SaveTag save) { }

        public void Write(IDataWriter w)
        {
            w.Write(this.CustomerID);
            this.WriteExtra(w);
        }
        protected virtual void WriteExtra(IDataWriter w) { }

        public ISerializable Read(IDataReader r)
        {
            this.CustomerID = r.ReadInt32();
            this.ReadExtra(r);
            return this;
        }
        protected virtual void ReadExtra(IDataReader r) { }
    }
}

using System.IO;

namespace Start_a_Town_
{
    public abstract class CustomerProperties : ISaveable, ISerializable
    {
        public int CustomerID;
        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.CustomerID.Save(tag, "CustomerID");
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

        public void Write(BinaryWriter w)
        {
            w.Write(this.CustomerID);
            this.WriteExtra(w);
        }
        protected virtual void WriteExtra(BinaryWriter w) { }

        public ISerializable Read(IDataReader r)
        {
            this.CustomerID = r.ReadInt32();
            this.ReadExtra(r);
            return this;
        }
        protected virtual void ReadExtra(IDataReader r) { }
    }
}

using System.IO;

namespace Start_a_Town_
{
    public class ObjectRefIDsAmount : ISaveable, ISerializableNew
    {
        public int Object;
        public int Amount;
        
        public ObjectRefIDsAmount()
        {

        }
        public ObjectRefIDsAmount(GameObject obj)
        {
            this.Object = obj.RefId;
            this.Amount = obj.StackSize;
        }
        public ObjectRefIDsAmount(GameObject obj, int amount)
        {
            this.Object = obj.RefId;
            this.Amount = amount;
        }
        public SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(this.Object.Save("Object"));
            tag.Add(this.Amount.Save("Amount"));
            return tag;
        }
        public ISaveable Load(SaveTag tag)
        {
            tag.TryGetTagValueOrDefault("Object", out this.Object);
            tag.TryGetTagValueOrDefault("Amount", out this.Amount);
            return this;
        }

        public void Write(BinaryWriter w)
        {
            w.Write(this.Object);
            w.Write(this.Amount);
        }
        public void Write(IDataWriter w)
        {
            w.Write(this.Object);
            w.Write(this.Amount);
        }
        public ISerializableNew Read(IDataReader r)
        {
            this.Object = r.ReadInt32();
            this.Amount = r.ReadInt32();
            return this;
        }

        public static ISerializableNew Create(IDataReader r) => new ObjectRefIDsAmount().Read(r);
        

    }
}

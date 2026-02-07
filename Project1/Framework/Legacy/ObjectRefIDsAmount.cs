using Project1.Core.Entities;
using Project1.Core.Base;
using Project1.Core.Helpers;
using Project1.Core.Interfaces;

namespace Project1.Core.Legacy
{
    public class ObjectRefIDsAmount : ISaveable, ISerializableNew<ObjectRefIDsAmount>
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

        public void Write(IDataWriter w)
        {
            w.Write(this.Object);
            w.Write(this.Amount);
        }
        public ObjectRefIDsAmount Read(IDataReader r)
        {
            this.Object = r.ReadInt32();
            this.Amount = r.ReadInt32();
            return this;
        }

        public static ObjectRefIDsAmount Create(IDataReader r) => new ObjectRefIDsAmount().Read(r);
        

    }
}

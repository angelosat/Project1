using System;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    public class ObjectAmount : ISaveable, ISerializable
    {
        TargetArgs ObjectTarget;
        public GameObject Object
        {
            get { return this.ObjectTarget.Object; }
            set { this.ObjectTarget = new TargetArgs(value); }
        }
        int _amount;
        public int Amount
        {
            get
            {
                return this._amount;
            }
            set
            {
                if (this.Object == null)
                    throw new Exception();
                if (value > this.Object.StackMax)
                    throw new Exception();
                this._amount = Math.Max(0, value);
            }
        }
        public ObjectAmount()
        {

        }
        public ObjectAmount(GameObject obj)
        {
            this.Object = obj;
            this.Amount = obj.StackSize;
        }
        public ObjectAmount(GameObject obj, int amount)
        {
            this.Object = obj;
            this.Amount = amount;
        }
        public ObjectAmount((Entity i, int amount) tuple)
        {
            this.Object = tuple.i;
            this.Amount = tuple.amount;
        }
        public SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(this.ObjectTarget.Save("Object"));
            tag.Add(this.Amount.Save("Amount"));
            return tag;
        }
        public ISaveable Load(SaveTag tag)
        {
            tag.TryGetTag("Object", t => this.ObjectTarget = new TargetArgs(t));
            tag.TryGetTagValue<int>("Amount", t => this._amount = t);
            return this;
        }
        public override string ToString()
        {
            return this.Object.Name + ": " + this.Amount.ToString();
        }

        internal void ResolveReferences(INetPeer net)
        {
            this.ObjectTarget.Map = net.Map;
        }

        public void Write(BinaryWriter w)
        {
            this.ObjectTarget.Write(w);
            w.Write(this._amount);
        }

        public ISerializable Read(BinaryReader r)
        {
            this.ObjectTarget = TargetArgs.Read(Network.CurrentNetwork, r);
            this._amount = r.ReadInt32();
            return this;
        }
    }
}

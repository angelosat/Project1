using Project1.Core.Entities;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Serialization;
using System;

namespace Project1.Core.Legacy
{
    public class ObjectAmount : ISaveable, ISerializableNew<ObjectAmount>
    {
        TargetArgs ObjectTarget;
        public Entity Object
        {
            get { return this.ObjectTarget.Entity; }
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
        public ObjectAmount(Entity obj)
        {
            this.Object = obj;
            this.Amount = obj.StackSize;
        }
        public ObjectAmount(Entity obj, int amount)
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

        internal void ResolveReferences(WorldBase world)
        {
            this.ObjectTarget.InitializeProvider(world);
        }

        public void Write(IDataWriter w)
        {
            this.ObjectTarget.Write(w);
            w.Write(this._amount);
        }

        public ObjectAmount Read(IDataReader r)
        {
            this.ObjectTarget = TargetArgs.Read(Network.CurrentEndpoint, r);
            this._amount = r.ReadInt32();
            return this;
        }

        public static ObjectAmount Create(IDataReader r)
        {
            return new ObjectAmount().Read(r);
        }
    }
}

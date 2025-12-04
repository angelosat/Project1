using System;
using System.Collections.Generic;
using System.IO;

namespace Start_a_Town_
{
    public abstract class QuestObjective : ISerializable, ISaveable
    {
        int _Count = 1;
        public int Count
        {
            get => this._Count; set
            {
                this._Count = Math.Max(0, value);
                this.Parent.Manager.QuestModified(this.Parent);
            }
        }
        public abstract string Text { get; }
        public QuestDef Parent;
        public QuestObjective(QuestDef parent)
        {
            this.Parent = parent;
        }
        public abstract int GetValue();
        public abstract void Write(BinaryWriter w);
        public abstract ISerializable Read(IDataReader r);
        public abstract bool IsCompleted(Actor actor);
        protected virtual void AddSaveData(SaveTag save) { }
        void Save(SaveTag save)
        {
            this.GetType().FullName.Save(save, "Type");
            this.Count.Save(save, "Count");
            this.AddSaveData(save);
        }
        
        protected virtual void Load(SaveTag load) { }
       
        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.Save(tag);
            return tag;
        }

        ISaveable ISaveable.Load(SaveTag tag)
        {
            this.Count = tag.GetValue<int>("Count");
            this.Load(tag);
            return this;
        }

        internal void Remove()
        {
            this.Parent.RemoveObjective(this);
        }

        internal virtual void TryComplete(Actor actor, OffsiteAreaDef area)
        {
        }

        internal virtual IEnumerable<ObjectAmount> GetQuestItemsInInventory(Actor actor)
        {
            yield break;
        }
    }
}

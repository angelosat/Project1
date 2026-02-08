using Project1.Core.World.WorldAreas;
using Project1.Core.Base;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Interfaces;
using Project1.Core.Legacy;
using System;
using System.Collections.Generic;
using System.IO;
using Project1.Framework.IO;

namespace Project1.Core.Quests
{
    public abstract class QuestObjective : ISaveable, ISerializable
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
        public abstract void Write(IDataWriter w);
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

        internal virtual void TryComplete(Actor actor, FrontierDef area)
        {
        }

        internal virtual IEnumerable<ObjectAmount> GetQuestItemsInInventory(Actor actor)
        {
            yield break;
        }
    }
}

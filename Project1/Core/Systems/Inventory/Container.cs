using Project1.Core.Entities;
using Project1.Framework;
using Project1.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Inventory
{
    public class Container
    {
        public int ID;
        public string Name = "";
        public Func<GameObject, bool> Filter = o => true;
        GameObject _Parent;
        public GameObject Parent
        {
            get => this._Parent;
            set
            {
                this._Parent = value;
                foreach (var slot in this.Slots)
                    slot.Owner = value;
            }
        }
        public List<GameObjectSlot> Slots = new();
        public Container()
        {

        }
        public Container(int capacity)
        {
            this.Initialize(capacity);
        }
        private void Initialize(int capacity)
        {
            for (int i = 0; i < capacity; i++)
                this.Slots.Add(new GameObjectSlot((byte)i) { ContainerNew = this });
        }
        public GameObjectSlot GetSlot(int id)
        {
            return this.Slots.FirstOrDefault(f => f.ID == id);
        }
        public List<GameObjectSlot> GetNonEmpty()
        {
            return (from slot in this.Slots where slot.Object != null select slot).ToList();
        }
        public void Write(IDataWriter writer)
        {
            var haveObjects = from slot in this.Slots where slot.Object != null select slot;
            writer.Write(haveObjects.Count());
            foreach (var slot in haveObjects)
            {
                writer.Write(slot.ID);
                slot.Write(writer);
            }
        }
        public void Read(IDataReader reader)
        {
            int haveObjects = reader.ReadInt32();
            for (int i = 0; i < haveObjects; i++)
            {
                var id = reader.ReadByte();
                var slot = this.Slots.FirstOrDefault(f => f.ID == id);
                slot.Read(reader);
            }
        }
        public override string ToString()
        {
            return this.ID.ToString() + ":" + this.Name + ":" + this.Slots.Count.ToString();
        }
        public string ToStringFull()
        {
            var text = this.ToString();
            foreach (var slot in this.Slots)
                text += '\n' + slot.ToString();
            return text;
        }
        public List<SaveTag> Save()
        {
            var containerTag = new List<SaveTag>();
            containerTag.Add(new SaveTag(SaveTag.Types.Int, "ID", this.ID));
            var items = new SaveTag(SaveTag.Types.Compound, "Items");
            for (int i = 0; i < this.Slots.Count; i++)
            {
                var objSlot = this.Slots[i];
                if (objSlot.Object != null)
                    items.Add(new SaveTag(SaveTag.Types.Compound, i.ToString(), objSlot.Save()));
            }
            containerTag.Add(items);
            return containerTag;
        }
        public Container Load(SaveTag containerTag)
        {
            var dic = containerTag.Value as Dictionary<string, SaveTag>;
            containerTag.TryGetTagValueOrDefault<int>("ID", out this.ID);
            var itemList = dic["Items"].Value as Dictionary<string, SaveTag>;
            foreach (var itemTag in itemList.Values)
            {
                if (itemTag.Value is null)
                    continue;
                int index = byte.Parse(itemTag.Name);
                var slot = GameObjectSlot.Create(itemTag);
                slot.ContainerNew = this;
                slot.ID = (byte)index;
                this.Slots[index].Assign(slot.Object);
            }
            return this;
        }
    }
}

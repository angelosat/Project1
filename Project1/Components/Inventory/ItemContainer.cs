using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Start_a_Town_.Components
{
    public class ItemContainer : GameObjectSlotCollection
    {
        public Func<GameObject, bool> Filter = o => true;
        GameObject _parent;
        public GameObject Parent
        {
            get { return this._parent; }
            set
            {
                this._parent = value;
                foreach (var slot in this)
                    slot.Parent = value;
            }
        }
        public List<ItemContainer> Children { get; set; }
        public byte ID;
        new public byte Capacity { get; set; }
        public ItemContainer(GameObject parent, byte size = 0) : this(parent, size, o => true) { }
        public ItemContainer(GameObject parent, byte size, Func<GameObject, bool> filter)
        {
            this.Parent = parent;
            this.Children = new List<ItemContainer>();
            this.Capacity = size;
            for (byte n = 0; n < size; n++)
            {
                var id = parent.ChildrenSequence;
                var sl = new GameObjectSlot(id) { Container = this };
                Add(sl);
            }
            this.Filter = filter;
        }
        public ItemContainer(byte size = 0)
        {
            this.Parent = null;
            this.Children = new List<ItemContainer>();
            this.Capacity = size;
        }
        private void Initialize(byte size)
        {
            this.Clear();
            this.Capacity = size;
            for (byte n = 0; n < size; n++)
                Add(new GameObjectSlot(n) { Container = this });
        }
        public ItemContainer(byte size, Func<byte> indexSequence) 
        {
            this.Children = new List<ItemContainer>();
            this.Clear();
            this.Capacity = size;
            for (byte n = 0; n < size; n++)
                Add(new GameObjectSlot(indexSequence()) { Container = this });
        }
        public ItemContainer(GameObject parent, byte size, Func<byte> indexSequence)
        {
             this.Parent = parent;
            this.Children = new List<ItemContainer>();
            this.Clear();
            this.Capacity = size;
            for (byte n = 0; n < size; n++)
                Add(new GameObjectSlot(indexSequence()) { Container = this });
        }
        public ItemContainer(BinaryReader reader)
        {
            this.ID = reader.ReadByte();
            this.Capacity = (byte)reader.ReadInt32();
            this.Children = new List<ItemContainer>();
            int occupiedslotCount = reader.ReadInt32();
            for (byte n = 0; n < this.Capacity; n++)
                Add(new GameObjectSlot(n) { Container = this });
            for (int i = 0; i < occupiedslotCount; i++)
            {
                int slotID = reader.ReadInt32();
                var slot = this[slotID];
                slot.Read(reader);
                slot.Container = this;
            }
        }
        
        public override string ToString()
        {
            string text = "";
            for (int i = 0; i < this.Count; i++)
                text += "[" + i + "]: " + this[i].ToString() + "\n";
            return text.TrimEnd('\n');
        }

        public void Write(System.IO.BinaryWriter writer)
        {
            writer.Write(this.ID);
            writer.Write(this.Count);
            var slots = from slot in this where slot.HasValue select slot;
            writer.Write(slots.Count());
            for (int i = 0; i < this.Count; i++)
            {
                var slot = this[i];
                if (!slot.HasValue)
                    continue;
                writer.Write(i);
                slot.Write(writer);
            }
        }
        public void Read(BinaryReader reader)
        {
            this.Clear();
            this.ID = reader.ReadByte();
            int size = reader.ReadInt32();
            for (byte n = 0; n < size; n++)
                Add(new GameObjectSlot(n) { Container = this });
            int occupiedslotCount = reader.ReadInt32();
            for (int i = 0; i < occupiedslotCount; i++)
            {
                int slotID = reader.ReadInt32();
                GameObjectSlot slot = this[slotID];
                slot.Container = this;
                slot.Read(reader);
            }
        }

        public List<GameObjectSlot> Empty()
        {
            var contents = new List<GameObjectSlot>();
            foreach (var slot in
                from slot in this
                where slot.HasValue
                select slot)
            {
                contents.Add(slot.Clone());
                slot.Clear();
            }
            return contents;
        }
        public List<GameObjectSlot> Clear(byte newCapacity)
        {
            var contents = new List<GameObjectSlot>();
            foreach (var slot in
                from slot in this
                where slot.HasValue
                select slot)
            {
                contents.Add(slot.Clone());
                slot.Clear();
            }

            Initialize(newCapacity);

            return contents;
        }
    }                        
}

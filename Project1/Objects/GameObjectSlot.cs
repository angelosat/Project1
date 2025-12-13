using SharpDX.Direct3D9;
using Start_a_Town_.Components;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public class GameObjectSlot : ITooltippable
    {
        Func<GameObject, bool> _Filter = o => true;
        public Func<GameObject, bool> Filter
        {
            get { return this.ContainerNew == null ? this._Filter : this.ContainerNew.Filter; }
            set { this._Filter = value; }
        }
        public Action<GameObject> ObjectChangedAction = o => { };
        public string Name = "";
        public ItemContainer Container { get; set; }
        public Container ContainerNew;
        GameObject _parent;
        public GameObject Parent
        {
            get => this.Container == null ? this._parent : this.Container.Parent;
            set
            {
                this._parent = value;
                if (this.HasValue)
                    this.Object.Parent = value;
            }
        }
        public byte ID { get; set; }
        public int StackSize
        {
            get { return this.HasValue ? this.Object.StackSize : 0; }
            set
            {
                if (value == 0)
                {
                    this.Object = null;
                    return;
                }
                if (this.HasValue)
                    this.Object.StackSize = value;
                
            }
        }
        public int StackMax => this.Object is null ? 1 : Object.StackMax;
        GameObject _link;
        public GameObject Link
        {
            get { return this._link; }
            set
            {
                var old = this._link;
                this._link = value;
                if (old != this._link)
                    ObjectChangedAction(this._link);
            }
        }
        GameObject _object;

        /// <summary>
        /// If the object is in another container, setting the slot will remove it from the other container.
        /// </summary>
        public virtual GameObject Object
        {
            get => this.Link ?? _object;
            [Obsolete]
            set
            {
                if (value is not null)
                    if (!this.Filter(value))
                        return;
                if (this._object != null)
                {
                    this._object.Slot = null;
                    this._object.Parent = null;
                }
                _object = value;
                ObjectChangedAction(value);
                OnObjectChanged();
                if (value != null)
                {
                    var _ = value.Container?.Remove(value) ?? value.Map?.Despawn(value);
                    if (value.Slot is not null && value.Slot != this)
                        value.Slot.Clear();
                    value.Slot = this;
                    value.Parent = this.Parent;
                }
            }
        }
        public bool HasValue => this.Object != null;
        public Func<Icon> GetIcon;

        public GameObjectSlot(byte id)
        {
            this.ID = id;
        }
        public GameObjectSlot(GameObject obj = null, int stackSize = 1)
        {
            Object = obj;
            StackSize = obj == null ? 0 : stackSize;
        }
        public GameObjectSlot(ItemContainer parent, GameObject obj = null, int stackSize = 1)
        {
            this.Container = parent;
            Object = obj;
            StackSize = obj == null ? 0 : stackSize;
        }
        
        void OnObjectChanged()
        {
            this.GetIcon = () => Object.GetIcon();
        }

        public bool Swap(GameObjectSlot otherSlot)
        {
            var otherobj = otherSlot.Object;
            otherSlot.Object = this.Object;
            this.Object = otherobj;
            return true;
        }
        static public bool Swap(GameObjectSlot slot1, GameObjectSlot slot2)
        {
            return slot1.Swap(slot2);
        }
        
        /// <summary>
        /// Copies the object and stack amount of one slot to another, and returns the old values of the target slot.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        static public GameObjectSlot Copy(GameObjectSlot source, GameObjectSlot target)
        {
            GameObjectSlot temp = target.Clone();
            target.Object = source.Object;
            target.StackSize = source.StackSize;
            return temp;
        }
        
        public GameObjectSlot SetObject(GameObject obj)
        {
            this.Object = obj;
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="newItem"></param>
        /// <returns>The previous item (or null) the slot held.</returns>
        public bool SetItem(GameObject newItem, out GameObject prevItem)
        {
            prevItem = this._object;
            if (newItem is not null && !this.Filter(newItem))
                    return false;
            if (prevItem != null)
            {
                prevItem.Slot = null;
                prevItem.Parent = null;
            }
            this._object = newItem;
            this.ObjectChangedAction(newItem);
            this.OnObjectChanged();
            if (newItem != null)
            {
                var _ = newItem.Container?.Remove(newItem) ?? newItem.Map?.Despawn(newItem);
                if (newItem.Slot is not null && newItem.Slot != this)
                    newItem.Slot.Clear();
                newItem.Slot = this;
                newItem.Parent = this.Parent;
            }
            return true;
        }
        public override string ToString()
        {
            return $"{this.ID}: {(!string.IsNullOrWhiteSpace(this.Name) ? this.Name + ":" : "")} + {(Object is not null ? Object.Name + $" ({StackSize})" : "<empty>")}";
        }

        public void Write(IDataWriter writer)
        {
            writer.Write(ID);
            writer.Write(this.Name);
            writer.Write(this.HasValue);
            if (this.HasValue)
                this.Object.Write(writer);
        }
        public void Read(IDataReader reader)
        {
            this.ID = reader.ReadByte();
            this.Name = reader.ReadString();
            if (!reader.ReadBoolean()) // if not having a value
                return;
            // set backing field instead of property so inventorychanged event isn't raised
            this._object = GameObject.Create(reader);
            this._object.Parent = this.Parent;
            this._object.Slot = this;
            // no need to set stacksize here since it's saved along with the object
            // PLUS i don't want to raise inventorychanged event that's raised in the property setter
        }

        public List<SaveTag> Save()
        {
            List<SaveTag> data = [new SaveTag(SaveTag.Types.Compound, "Object", Object.SaveInternal())];
            return data;
        }

        static public GameObjectSlot Create(SaveTag tag) 
        { 
            return Create(null, tag);
        }
        static public GameObjectSlot Create(ItemContainer parent, SaveTag tag)
        {
            GameObject obj = (SaveTag.Types)tag["Object"].Type switch
            {
                SaveTag.Types.Compound => GameObject.Load((SaveTag)tag["Object"]),
                _ => throw new ArgumentException("Invalid tag type"),
            };
            GameObjectSlot slot = new GameObjectSlot(parent, obj);
           
            return slot;
        }
        public GameObject Load(SaveTag tag)
        {
            GameObject obj = (SaveTag.Types)tag["Object"].Type switch
            {
                SaveTag.Types.Compound => GameObject.Load((SaveTag)tag["Object"]),
                _ => throw new ArgumentException("Invalid tag type"),
            };
            this.Object = obj;
            return obj;
        }

        public void GetTooltipInfo(Control tooltip)
        {
            if (Object is not null)
            {
                this.Object.GetTooltipInfo(tooltip);
            }
            if (this.ContainerNew is not null)
                tooltip.Controls.Add(new Label(tooltip.Controls.BottomLeft, this.ContainerNew.ToString()));
            tooltip.Controls.Add(new Label(tooltip.Controls.BottomLeft, this.ToString()));
        }

        public GameObjectSlot Clone()
        {
            // maybe return new object?
            return new GameObjectSlot(Object, StackSize) { Filter = this.Filter, Container = this.Container, Parent = this.Parent };
        }

        static public GameObjectSlot Empty
        {
            get { return new GameObjectSlot(); }
        }

        /// <summary>
        /// Sets Object to null and returns true if Object was non-null.
        /// </summary>
        /// <returns></returns>
        public bool Clear()
        {
            bool had = HasValue;
            Object = null;
            StackSize = 0; // WARNING! i had this commented out for some reason
            this.Link = null;
            return had;
        }
        internal void Consume(int amount)
        {
            if (this.Object is null)
                return;
            if (this.Object.StackSize > amount)
                this.Object.StackSize-=amount;
            else
            {
                this.Object.Net.DisposeObject(this.Object);
                this.Clear();
            }
        }
    }
}

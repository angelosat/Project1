using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Input;
using Project1.Core.Screens;
using Project1.Core.UI;
using Project1.Framework;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Inventory
{
    public sealed class ContainerList : Inspectable, IList<Entity>, ISerializableNew<ContainerList>, ISaveable
    {
        TableObservable<Entity> _gui;
        public Control Gui
        {
            get
            {
                var actor = this.Parent as Actor;
                _gui ??= new TableObservable<Entity>()
                        .AddColumn("name", 96, o => new Label(() => o.Name, () => Inspector.Refresh(o)) { TooltipFunc = o.GetInventoryTooltip })
                        .AddColumn("preference", 96, o => actor.ItemPreferences.GetListControl(o))
                        .AddColumn("weight", 32, o => new Label(() => o.TotalWeight.ToString("0.# kg")))
                        .AddColumn("drop", Icon.Cross.Width, o => IconButton.CreateSmall(Icon.Cross, () => drop(o), "Drop").ShowOnParentFocus(true));

                throw new Exception();
                return null;// _gui.Bind(this.Contents);
                void drop(Entity o)
                {
                    if (actor.IsSpawned && actor.IsTownMember)
                        Ingame.Instance.Events.Post(new PlayerForcedDropInventoryItemEvent(this.Parent, o, o.StackSize));
                }
            }
        }
        public event Action<Entity> ItemAdded, ItemRemoved; 
        readonly List<Entity> Contents = [];
        public Entity Parent;

        public int Count => ((ICollection<Entity>)this.Contents).Count;

        public bool IsReadOnly => ((ICollection<Entity>)this.Contents).IsReadOnly;

        public override string LabelReadable => this.ToString();

        public Entity this[int index] { get => ((IList<Entity>)this.Contents)[index]; set => ((IList<Entity>)this.Contents)[index] = value; }

        public int IndexOf(Entity item)
        {
            return ((IList<Entity>)this.Contents).IndexOf(item);
        }

        public void Insert(int index, Entity item)
        {
            if (item.Container == this)
                throw new Exception();
            ((IList<Entity>)this.Contents).Insert(index, item);
            item.Container?.Remove(item);
            item.Container = this;
        }

        public void RemoveAt(int index)
        {
            var item = this[index];
            if (item.Container != this)
                throw new Exception();
            item.Container = null;
            ((IList<Entity>)this.Contents).RemoveAt(index);
        }
        public void Add(Entity item)
        {
            if (item.Container == this)
                throw new Exception();

            if (this.Contents.FirstOrDefault(i => i.CanAbsorb(item)) is Entity existing)
            {
                //existing.StackSize += item.StackSize;
                if (item.StackSize > existing.StackAvailableSpace)
                    throw new NotImplementedException();
                existing.Add(item.StackSize);
                item.Consume(item.StackSize);
                return;
                //throw new NotImplementedException();
            }

            ((ICollection<Entity>)this.Contents).Add(item);
            item.World.Events.Post(new InventoryItemAddedEvent(this.Parent as Actor, item));
            item.Detach();
            item.Container = this;
            item.Owner = this.Parent;
            (this.Parent as Actor).Log.Write($"Stored {item} in inventory");
            this.ItemAdded?.Invoke(item);
        }
        internal void AddInternal(Entity item)
        {
            this.Contents.Add(item);
            this.ItemAdded?.Invoke(item);
        }
        internal void RemoveInternal(Entity item)
        {
            this.Contents.Remove(item);
            this.ItemRemoved ?.Invoke(item);
        }
        public void Clear()
        {
            foreach (var i in this.Contents)
                i.Container = null;
            ((ICollection<GameObject>)this.Contents).Clear();
        }
        public bool Contains(Entity item)
        {
            return ((ICollection<Entity>)this.Contents).Contains(item);
        }
        public void CopyTo(Entity[] array, int arrayIndex)
        {
            ((ICollection<Entity>)this.Contents).CopyTo(array, arrayIndex);
        }
        public bool Remove(Entity item)
        {
            if (item.Container != this)
                throw new Exception();
            item.Container = null;
            item.Owner = null;
            this.ItemRemoved?.Invoke(item);
            item.World.Events.Post(new InventoryItemRemovedEvent(this.Parent as Actor, item));
            return ((ICollection<Entity>)this.Contents).Remove(item);

        }

        public IEnumerator<Entity> GetEnumerator()
        {
            return ((IEnumerable<Entity>)this.Contents).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.Contents).GetEnumerator();
        }
        public void Write(IDataWriter w)
        {
            w.Write(this.Contents.Count);
            foreach (var o in this.Contents)
                o.Write(w);
        }
        public ContainerList Read(IDataReader r)
        {
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var obj = GameObject.Create(r) as Entity;
                this.Contents.Add(obj);
                obj.Container = this;
            }
            return this;
        }
        internal void Instantiate(Action<GameObject> instantiator)
        {
            foreach (var o in this.Contents)
                instantiator(o);
        }
        public SaveTag Save(string name = "")
        {
            var save = new SaveTag(SaveTag.Types.Compound, name);
            var listtag = new SaveTag(SaveTag.Types.List, "Contents", SaveTag.Types.Compound);
            foreach (var i in this.Contents)
                listtag.Add(i.Save());
            save.Add(listtag);
            return save;
        }
        public ISaveable Load(SaveTag tag)
        {
            //return this; // added this to reset inventory contents of every entity to do some work that will break existing items
            var itemList = tag["Contents"].Value as List<SaveTag>;
            foreach (var itemTag in itemList)
                if(GameObject.Load(itemTag) is Entity obj)
                {
                    this.Contents.Add(obj);
                    obj.Container = this;
                }
            return this;
        }
        public override IEnumerable<(string item, object value)> Inspect()
        {
            yield return (nameof(this.Parent), this.Parent);
            yield return (nameof(this.Contents), this.Contents);
        }
        public static ContainerList Create(IDataReader r) => new ContainerList().Read(r);
    }
}

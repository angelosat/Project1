using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Gear;
using Project1.Core.Legacy;
using Project1.Core.Networking;
using Project1.Core.Systems.Materials;
using Project1.Framework;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Inventory
{
    public sealed class InventoryComponent : EntityComp
    {
        public new class Spec(int size) : Spec<InventoryComponent> 
        {
            public readonly int Capacity = size;

            protected override void ApplyDefaultsTo(InventoryComponent comp)
            {
                comp.Capacity = this.Capacity;
            }
        }
        internal override void CopyFrom(EntityComp source)
        {
            var comp = (InventoryComponent)source;
            foreach (var i in comp.Contents)
                this.Contents.AddInternal(i.Clone() as Entity);
        }
        public override EntityCompDef CompDef => EntityCompDefOf.Inventory;

        public int Capacity = 16;
        public float PercentageEmpty => this.Contents.Count / (float)this.Capacity;
        public float PercentageFull => 1 - this.PercentageEmpty;
        public bool HasFreeSpace => this.PercentageEmpty < 1;
        readonly Container HaulContainer;
        public readonly GameObjectSlot HaulSlot;
        public ContainerList Contents = [];
        internal void Remove(Entity obj)
        {
            this.Contents.Remove(obj);
        }
        internal void Remove(GameObject obj)
        {
            this.Remove(obj as Entity);
        }
        [Obsolete]
        internal void SyncInsert(GameObject split)
        {
            var actor = this.Owner as Actor;
            var net = actor.Net;
            if (net is not Server server)
                throw new Exception();
            //Packets.SendSyncInsert(net, actor, split as Entity);
        }

        public override string Name { get; } = "PersonalInventory";


        public float Distance(GameObject obj1, GameObject obj2)
        {
            return obj1.Inventory.Contains(obj => obj == obj2) ? 0 : -1;
        }
        public Vector3? DistanceVector(GameObject obj1, GameObject obj2)
        {
            return obj1.Inventory.Contains(obj => obj == obj2) ? Vector3.Zero : null;
        }

        internal override void Resolve()
        {
            this.Contents.Parent = this.Owner;
            this.Owner.RegisterContainer(this.HaulContainer);
        }
        public void Throw(Vector3 velocity, int amount = -1)
        {
            var thrownItem = this.HaulSlot.Object as Entity;
            ArgumentNullException.ThrowIfNull(thrownItem);
            var parent = this.Owner;
            parent.Map.Spawn(thrownItem, parent.Global + (1 + parent.Height) * Vector3.UnitZ, this.Owner.Velocity + velocity);
        }
  
        public override IEnumerable<Entity> GetChildren()
        {
            if (this.HaulContainer.Slots[0].Object is Entity obj)
                yield return obj;
            foreach (var o in this.Contents)
                yield return o;
        }
        public override void GetContainers(List<Container> list)
        {
            list.Add(this.HaulContainer);
        }
        public InventoryComponent()
            : base()
        {
            this.Owner = null;
            this.HaulContainer = new Container(1) { Name = "Hauling" };
            this.HaulSlot = this.HaulContainer.Slots.First();
        }
        public InventoryComponent(byte capacity)
            : this()
        {
            this.Capacity = capacity;
        }
        public GameObject Drop(GameObject item, int amount)
        {
            var parent = this.Owner;
            var slot = this.Contents.First(i => i == item);
            // TODO instantiate new item if necessary
            if (amount < item.StackSize)
            {
            }
            parent.Map.Spawn(item as Entity, parent.Global + new Vector3(0, 0, parent.Physics.Height), Vector3.Zero);
            return item;
        }
        internal override IEnumerable<GameObjectSlot> GetSlots()
        {
            yield return this.HaulSlot;
        }
        public void HaulNew(Entity target, int amount)
        {
            Entity finalItem;
            var actor = this.Owner as Actor;
            if (target == actor.Hauled)
                throw new Exception();

            if (!target.IsHaulable)
                throw new Exception();
            if (amount == 0)
                throw new Exception("Amount must be specified");
            if (amount < 0)
                amount = target.StackSize;
            if (amount > target.StackSize)
                throw new Exception("Cannot take more than stack size");
            if (amount < target.StackSize)
            {
                //target.Consume(amount);
                finalItem = target.Split(amount); // this creates a new entity
            }
            else
                finalItem = target;

            // if currently hauling something else, it must be made sure that it's of the same type so we can increase its stacksize. otherwise there has been a bug earlier
            if (actor.Inventory.HaulSlot.Object is not GameObject existing)
            {
                actor.Inventory.HaulSlot.Assign(finalItem, out var _);
                return;
            }
            if (!existing.CanAbsorb(finalItem))
                throw new Exception();
            // if the amount specified to haul will make the existing hauled item exceed the stackmax, there's been a bug
            if (existing.StackSize + amount > existing.StackMax)
                throw new Exception();
            existing.Add(amount);
            finalItem.Consume(amount);
        }

        public void Drop(Entity item)
        {
            var parent = this.Owner;
            if (!this.Contents.Contains(item))
                throw new Exception();
            this.Contents.Remove(item);
            item.Container = null;
            parent.Map.Spawn(item, parent.Global + new Vector3(0, 0, parent.Physics.Height), Vector3.Zero);

        }
        public bool StoreHauled()
        {
            if (this.HaulSlot.Object is null)
                return false;
            this.Contents.Add(this.HaulSlot.Object as Entity);

            //NpcComponent.AddPossesion(parent, obj); // why was i adding the item as a possesion here? the item becomes a possesion during ownership assignment
            // BECAUSE i want npc to claim ownership when picking up and storing ie. food in their inventory
            // but other problems arise if i set ownership here

            return true;
        }
        public bool Insert(GameObject obj)
        {
            return this.Insert(obj as Entity);
        }
        public bool Insert(Entity obj)
        {
            if (obj == null)
                return false;
            this.Contents.Add(obj);

            return true;
        }

        public bool Unequip(GameObject item)
        {
            var slot = (this.Owner as Entity).Gear.GetSlot(item);
            return this.Receive(slot);
        }
        public bool Unequip(GearTypeDef gearDef)
        {
            return this.Receive(this.Owner.Gear.GetSlot(gearDef));
        }
        public bool Receive(GameObjectSlot objSlot, bool report = true)
        {
            // TODO: if can't receive, haul item instead or drop on ground?
            var obj = objSlot.Object as Entity;
            var parent = this.Owner;
            this.Contents.Add(obj);
            objSlot.Clear();
            return true;
            // TODO: drop object if can't receive? here? or let whoever called this method do something else if it fails?
        }

        public IEnumerable<Entity> GetItems()
        {
            foreach (var sl in this.Contents)
                yield return sl as Entity;
        }
        public IEnumerable<Entity> All => this.GetItems();

        public Entity First(Func<Entity, bool> filter)
        {
            foreach (var slot in this.Contents)
                if (filter(slot))
                    return slot;
            if (this.HaulSlot.Object != null && filter(this.HaulSlot.Object as Entity))
                return this.HaulSlot.Object as Entity;
            return null;
        }
        public int Count(ItemDef def)
        {
            return this.Count(e => e.Def == def);
        }
        public int Count(ItemDef def, MaterialDef mat)
        {
            return this.Count(e => e.Def == def && e.PrimaryMaterial == mat);
        }
        public int Count(Func<Entity, bool> filter)
        {
            return this.FindItems(filter).Sum(i => i.StackSize);

        }
        public bool Contains(GameObject item)
        {
            return this.Contents.FirstOrDefault(s => s == item) != null;
        }
        public bool Contains(Func<GameObject, bool> filter)// Predicate<GameObject> filter)
        {
            return (from slot in this.Contents
                    where filter(slot)
                    select slot).FirstOrDefault() != null;
        }
        public bool CheckWeight(GameObject obj)
        {
            return true;
        }

        public bool Haul(Entity obj)
        {
            if (obj is null)
                return true;
            var parent = this.Owner;
            var current = this.HaulSlot.Object;

            if (obj == current)
                return true;
            if (!this.CheckWeight(obj))
                return true;
            var net = parent.Net;
            // if currently hauling object of same type, increase held stacksize and dispose other object
            if (current != null)
                if (current.CanAbsorb(obj))
                {
                    current.Add(1);
                    obj.OnDespawn();
                    net.DisposeObject(obj);
                    return true;
                }

            this.Throw(Vector3.Zero, true); //or store carried object in backpack? (if available)

            this.HaulSlot.Assign(obj);
            return true;
        }

        public bool Throw(Vector3 direction, bool all = false)
        {
            var parent = this.Owner;
            var velocity = direction * 0.1f + parent.Velocity;
            // throws hauled object, if hauling nothing throws equipped object, make it so it only throws hauled object?
            var slot = this.HaulSlot;
            if (slot.Object == null)
                return false;
            Entity newobj;
            if (!all && slot.Object.StackSize > 1)
            {
                newobj = (slot.Object as Entity).Split(1);
            }
            else
                newobj = slot.Object as Entity;
            // TODO instantiate new obj as necessary
            newobj.Global = parent.Global + new Vector3(0, 0, parent.Physics.Height);
            newobj.Velocity = velocity;
            newobj.Physics.Enable();
            parent.Map.Spawn(newobj, newobj.Global, newobj.Velocity);
            //newobj.SyncSpawnNew(parent.Map);

            if (all)
                slot.Clear();
            return true;
        }

        public IEnumerable<ObjectAmount> Take(Func<Entity, bool> filter, int amount)
        {
            var remaining = amount;

            var e = this.FindItems(filter).GetEnumerator();
            while (e.MoveNext() && remaining > 0)
            {
                var i = e.Current;
                var amountToReturn = Math.Min(i.StackSize, remaining);
                remaining -= amountToReturn;
                yield return new ObjectAmount(i, amountToReturn);
            }
        }
        public override void Write(IDataWriter w)
        {
            this.Contents.Write(w);
            this.HaulSlot.Write(w);
        }
        public override void Read(IDataReader r)
        {
            this.Contents.Read(r);
            this.HaulSlot.Read(r);
        }

        internal override List<SaveTag> Save()
        {
            var data = new List<SaveTag>();
            data.Add(this.Contents.Save("Contents"));
            var isHauling = this.HaulSlot.Object != null;
            data.Add(new SaveTag(SaveTag.Types.Bool, "IsHauling", isHauling));
            if (isHauling)
                data.Add(new SaveTag(SaveTag.Types.Compound, "Hauling", this.HaulSlot.Save()));

            return data;
        }
        internal override void LoadExtra(SaveTag data)
        {
            var container = new Container(16);
            if (!data.TryGetTag("Contents", t => this.Contents.Load(t)))
            {
                var tmpslots = new Container(16);
                data.TryGetTag("Inventory", tag => tmpslots.Load(tag));

                /// temp
                foreach (var i in tmpslots.Slots.Where(s => s.HasValue).Select(s => s.Object as Entity))
                    this.Contents.Add(i);
            }
            if (data.TryGetTagValueOrDefault("IsHauling", out bool isHauling) && isHauling)
                data.TryGetTag("Hauling", tag => this.HaulSlot.Load(tag));
        }

        public override string ToString()
        {
            var text = base.ToString() +
                '\n' + this.HaulContainer.ToStringFull();
            ;
            return text;
        }

        readonly Label CachedGuiLabelCarrying = new();
        //internal override void GetSelectionInfo(IUISelection info, GameObject parent)
        //{
        //    info.AddInfo(this.CachedGuiLabelCarrying.SetTextFunc(() => $"Carrying: {this.HaulSlot.Object?.DebugName ?? "Nothing"}"));
        //}

        public IEnumerable<Entity> FindItems(Func<Entity, bool> p)
        {
            foreach (var s in this.Contents)
            {
                if (s is not Entity e)
                    continue;
                if (p(e))
                    yield return e;
            }
        }

        internal bool TryGet(Func<Entity, bool> predicate, out Entity found)
        {
            found = this.Contents.FirstOrDefault(predicate);
            return found is not null;
        }
        internal Entity Get(Func<Entity, bool> predicate)
            => this.Contents.FirstOrDefault(predicate);
    }
}

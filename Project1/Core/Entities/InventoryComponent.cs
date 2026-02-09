using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Project1.Framework;
using Project1.Framework.UI;
using Project1.Framework.Serialization;
using Project1.Core.Gear;
using Project1.Core.Inventory;
using Project1.Core.Base;
using Project1.Core.Components;
using Project1.Core.Entities.Actors;
using Project1.Core.Legacy;
using Project1.Core.Materials;
using Project1.Core.Net;

namespace Project1.Core.Entities
{
    public class InventoryComponent : EntityComp
    {
        public new class Spec : Spec<InventoryComponent> 
        {
            public readonly int Capacity;
            public Spec(int size)
            {
                this.Capacity = size;
            }
            protected override void ApplyDefaultsTo(InventoryComponent comp)
            {
                comp.Capacity = this.Capacity;
            }
        }

        class Packets
        {
            static int PacketSyncInsert, PacketSetHaulSlot;
            public static void Init()
            {
                PacketSyncInsert = Registry.PacketHandlers.Register(HandleSyncInsert);

                static void handleSetHaulSlot(NetEndpoint net, Packet pck)
                {
                    var r = pck.PacketReader;
                    var actor = net.World.GetEntity(r.ReadInt32()) as Actor;
                    var item = net.World.GetEntity(r.ReadInt32()) as Entity;
                    actor.Carry(item);
                }

                PacketSetHaulSlot = Registry.PacketHandlers.Register(handleSetHaulSlot);
            }

            public static void SendSyncInsert(INetEndpoint net, Actor actor, Entity item)
            {
                var server = net as Server;
                //if (net is Server server)
                actor.Inventory.Insert(item);
                server.BeginPacket(PacketSyncInsert)
                    .Write(actor.RefId)
                    .Write(item.RefId);
            }
            private static void HandleSyncInsert(NetEndpoint net, Packet pck)
            {
                var r = pck.PacketReader;
                var actor = net.World.GetEntity(r.ReadInt32()) as Actor;
                var item = net.World.GetEntity(r.ReadInt32()) as Entity;
                if (net is Server)
                    SendSyncInsert(net, actor, item);
                else
                    actor.Inventory.Insert(item);
            }

            //public static void SyncSetHaulSlot(INetEndpoint net, Actor actor, Entity item)
            //{
            //    var server = net as Server;
            //    var w = server.OutgoingStreamTimestamped;// .GetOutgoingStream();
            //    w.Write(PacketSetHaulSlot);
            //    w.Write(actor.RefId);
            //    w.Write(item.RefId);
            //}
        }
        static InventoryComponent()
        {
            Packets.Init();
        }

        public int Capacity = 16;//this.Slots.Slots.Count;
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

        internal void SyncInsert(GameObject split)
        {
            var actor = this.Owner as Actor;
            var net = actor.Net;
            if (net is not Server server)
                throw new Exception();
            Packets.SendSyncInsert(net, actor, split as Entity);
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
  
        public override IEnumerable<GameObject> GetChildren()
        {
            if (this.HaulContainer.Slots[0].Object is GameObject obj)
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

        //public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        //{
        //    switch (e.Type)
        //    {
        //        // TODO: maybe create a new message called InventoryInteraction that individual components can respond too?
        //        case Message.Types.SlotInteraction:
        //            this.SlotInteraction(parent, e.Parameters[0] as GameObject, e.Parameters[1] as GameObjectSlot);
        //            return true;

        //        default:
        //            return false;
        //    }
        //}
        public GameObject Drop(GameObject item, int amount)
        {
            var parent = this.Owner;
            var slot = this.Contents.First(i => i == item);
            // TODO instantiate new item if necessary
            if (amount < item.StackSize)
            {
                //obj = slot.Object.Clone();
                //obj.StackSize = amount;
            }
            //item.Spawn(parent.Map, parent.Global + new Vector3(0, 0, parent.Physics.Height));

            parent.Map.Spawn(item as Entity, parent.Global + new Vector3(0, 0, parent.Physics.Height), Vector3.Zero);

            //item.StackSize -= amount;
            return item;
        }
        internal override IEnumerable<GameObjectSlot> GetSlots()
        {
            yield return this.HaulSlot;
        }
        public void HaulNew(GameObject target, int amount)
        {
            //if (this.Owner.Net.IsClient)
            //    return;
            GameObject finalItem;
            var actor = this.Owner as Actor;
            if (target == actor.Hauled)
                throw new Exception();

            if (!target.IsHaulable)
                throw new Exception();
            if (amount == 0)
                throw new Exception("Amount must be initialized");
            if (amount < 0)
                amount = target.StackSize;
            if (amount > target.StackSize)
                throw new Exception("Cannot take more than stack size");
            if (amount < target.StackSize)
            {
                //target.Consume(amount);
                finalItem = target.Split(amount) as Entity; // this creates a new entity
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
            //PacketSyncStackSize.Send(finalItem);



            //PacketSetStackSize.Send(existing, existing.StackSize);
            //GameObject finalItem;
            //var actor = this.Owner as Actor;
            //if (!target.IsHaulable)
            //    throw new Exception();
            //if (amount == 0)
            //    throw new Exception("Amount must be initialized");
            //if (amount < 0)
            //    amount = target.StackSize;
            //if (amount > target.StackSize)
            //    throw new Exception("Cannot take more than stack size");
            //if (amount < target.StackSize)
            //{
            //    target.Consume(amount);
            //    finalItem = target.Split(amount) as Entity;
            //}
            //else
            //    finalItem = target;

            //// if currently hauling something else, it must be made sure that it's of the same type so we can increase its stacksize. otherwise there has been a bug earlier
            //if(actor.Inventory.HaulSlot.Object is not GameObject existing)
            //{
            //    actor.Inventory.HaulSlot.SetItem(finalItem, out var _);/// putting the item in the gameobjectslot, removes it from its current container or despawns it, so no need to send packetremoveinventoryitem
            //    //PacketActorHaulUpdate.Send(actor, finalItem as Entity);
            //    return;
            //}
            //if (!existing.CanAbsorb(finalItem))
            //    throw new Exception();
            //// if the amount specified to haul will make the existing hauled item exceed the stackmax, there's been a bug
            //if (existing.StackSize + amount > existing.StackMax)
            //    throw new Exception();
            //existing.Add(amount);
            //finalItem.Consume(amount);
            ////PacketSetStackSize.Send(existing, existing.StackSize);
        }

        public void Drop(Entity item)
        {
            var parent = this.Owner;
            if (!this.Contents.Contains(item))
                throw new Exception();
            this.Contents.Remove(item);
            item.Container = null;
            //item.Spawn(parent.Map, parent.Global + new Vector3(0, 0, parent.Physics.Height));
            parent.Map.Spawn(item as Entity, parent.Global + new Vector3(0, 0, parent.Physics.Height), Vector3.Zero);

        }

        //void SlotInteraction(GameObject parent, GameObject actor, GameObjectSlot slot)
        //{
        //    if (!slot.HasValue)
        //    {
        //        // if right clicking an empty slot, put player's hauled item in it
        //        var hauled = this.HaulSlot;// PersonalInventoryComponent.GetHauling(actor);
        //        if (hauled.HasValue)
        //            hauled.Swap(slot);
        //        return;
        //    }
        //    var obj = slot.Object;

        //    if (obj.HasComponent<EquipComponent>())
        //        (parent as Actor).Work.Perform(new Interactions.EquipFromInventory(), new TargetArgs(parent.World, slot));
        //    else
        //    {
        //        this.HaulSlot.Swap(slot);
        //    }
        //}

        public bool StoreHauled()
        {
            if (this.HaulSlot.Object is null)
                return false;
            this.Contents.Add(this.HaulSlot.Object as Entity);
            //{
            //    // throw? or return false and raise event so we can handle it and display a message : not enough space?
            //    //inv.Throw(parent, Vector3.Zero);
            //    this.Parent.Net.EventOccured(Message.Types.NotEnoughSpace, this.Parent);
            //    return false;
            //}

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

            //if (!this.Slots.Insert(obj))
            //{
            //    // throw? or return false and raise event so we can handle it and display a message : not enough space?
            //    //inv.Throw(parent, Vector3.Zero);
            //    actor.Net.EventOccured(Message.Types.NotEnoughSpace, actor);
            //    return false;
            //}
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
            if (report)
                parent.Net.EventOccured((int)Message.Types.ItemGot, parent, obj);
            return true;
            // TODO: drop object if can't receive? here? or let whoever called this method do something else if it fails?
        }

        public IEnumerable<Entity> GetItems()
        {
            foreach (var sl in this.Contents)
                yield return sl as Entity;
        }
        public IEnumerable<Entity> All => this.GetItems();

        public GameObject First(Func<GameObject, bool> filter)
        {
            foreach (var slot in this.Contents)
                if (filter(slot))
                    return slot;
            if (this.HaulSlot.Object != null && filter(this.HaulSlot.Object))
                return this.HaulSlot.Object;
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
        //public bool Equip(GameObject item)
        //{
        //    foreach (var slot in this.Contents)
        //        if (slot == item)
        //            return GearComponent.Equip(this.Owner, slot);
        //    return false;
        //}
        public bool CheckWeight(GameObject obj)
        {
            return true;
        }

        public bool Haul(GameObject obj)
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

            //obj.OnDespawn();
            //this.HaulSlot.Object = obj;
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
            GameObject newobj;
            if (!all && slot.Object.StackSize > 1)
            {
                newobj = slot.Object.Split(1);
            }
            else
                newobj = slot.Object;
            // TODO instantiate new obj as necessary
            newobj.Global = parent.Global + new Vector3(0, 0, parent.Physics.Height);
            newobj.Velocity = velocity;
            //newobj.Physics.Enabled = true;
            newobj.Physics.Enable();

            newobj.SyncSpawnNew(parent.Map);

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
        internal override void GetSelectionInfo(IUISelection info, GameObject parent)
        {
            info.AddInfo(this.CachedGuiLabelCarrying.SetTextFunc(() => $"Carrying: {this.HaulSlot.Object?.DebugName ?? "Nothing"}"));
        }

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
    }
}

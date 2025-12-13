using SharpDX.XAudio2;
using Start_a_Town_.Components;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    public class GearComponent : EntityComp
    {
        static GearComponent()
        {
            Registry.GameEvents.Register<ActorGearUpdatedEvent>(); 
        }
        public override string Name { get; } = "Gear";

        public override void OnObjectLoaded(GameObject parent)
        {
            base.OnObjectLoaded(parent);
        }
        
        public override void Resolve()
        {
            this.Owner.RegisterContainer(this.Equipment);
        }
      
        public Container Equipment = new() { Name = "Equipment" };
        public float ArmorTotal;
        public GearComponent()
        {
        }
        public GearComponent(ItemDef def)
        {
            foreach (var slot in def.ActorProperties.GearSlots)
                this.Equipment.Slots.Add(new GameObjectSlot((byte)slot.ID) { ContainerNew = this.Equipment, Name = slot.Name });
        }
        public GearComponent(params GearType[] types)
        {
            foreach (var slot in types)
                this.Equipment.Slots.Add(new GameObjectSlot((byte)slot.ID) { ContainerNew = this.Equipment, Name = slot.Name });
        }
        public override object Clone()
        {
            var types = from gear in this.Equipment.Slots select GearType.Dictionary[(GearType.Types)gear.ID];
            var comp = new GearComponent(types.ToArray());
            //using (var w = new BinaryWriter(new MemoryStream()))
            //{
            //    this.Write(w);
            //    w.BaseStream.Position = 0;
            //    //using var r = new BinaryReader(w.BaseStream);
            //    using var r = new DataReader(w.BaseStream);
            //    comp.Read(r);
            //}
            using var mem = new MemoryStream();
            var w = new DataWriter(mem);
            using var r = new DataReader(mem);
            //using (var w = new BinaryWriter(new MemoryStream()))
            //{
                this.Write(w);
                w.Position = 0;
                //using var r = new BinaryReader(w.BaseStream);
                //using var r = new DataReader(w.BaseStream);
                comp.Read(r);
            //}
            return comp;
        }
        public override IEnumerable<GameObject> GetChildren()
        {
            foreach (var o in this.Equipment.Slots.Where(s => s.HasValue).Select(s => s.Object))
                yield return o;
        }
        public override void GetContainers(List<Container> list)
        {
            list.Add(this.Equipment);
        }

        public override string ToString()
        {
            string text = "";
            foreach (var slot in this.Equipment.Slots)
                text += $"{slot.ID}: {(slot.HasValue ? slot.Object.Name : "<empty>")}\n";
            return text.TrimEnd('\n');
        }

        public override void Write(IDataWriter writer)
        {
            this.Equipment.Write(writer);
        }
        public override void Read(IDataReader reader)
        {
            this.Equipment.Read(reader);
        }

        internal override List<SaveTag> Save()
        {
            var save = new List<SaveTag>();
            save.Add(new SaveTag(SaveTag.Types.Compound, "Equipment", this.Equipment.Save()));
            return save;
        }
        internal override void LoadExtra(SaveTag compTag)
        {
            compTag.TryGetTag("Equipment", tag => this.Equipment.Load(tag));
        }
        public GameObject GetGear(GearType type)
        {
            return this.Equipment.GetSlot((int)type.ID).Object;
        }
        public GameObjectSlot GetSlot(GearType type)
        {
            var slot = this.Equipment.GetSlot((int)type.ID);
            return slot;
        }
        public GameObjectSlot GetSlot(GameObject item)
        {
            var slot = this.Equipment.Slots.FirstOrDefault(s => s.Object == item);
            return slot;
        }
        public static bool Equip(GameObject a, GameObject t)
        {
            if (t is null)
                return false;
            
            var geartype = (int)t.GetComponent<EquipComponent>().Type.ID;

            GameObjectSlot gearSlot = a.GetComponent<GearComponent>().Equipment.Slots[geartype];

            // despawn item's entity from world (if it's spawned in the world)
            if (t.IsSpawned)
                t.OnDespawn();

            // attempt to store current equipped item in inventory, otherwise drop it if inventory is full
            
            // equip new item
            gearSlot.Object = t;

            return true;
        }
        protected void Equip(Entity item)
        {
            if (!this.Owner.Inventory.Contains(item))
                throw new Exception();
            var slotType = item.Def.GearType;
            var slot = this.GetSlot(slotType);

            // the slot implictly removes the new item from the inventory or despawns it from the map and outputs the previous item that occupied the slot
            slot.SetItem(item, out var previousItem);

            // the previousItem is currently detached from a parent but still exists, so we have to explicitly insert it in the inventory
            if(previousItem != null)
                this.Owner.Inventory.Insert(previousItem);

            this.RefreshStats();
            this.Owner.Net.Events.Post(new ActorGearUpdatedEvent(this.Owner as Actor, item, previousItem as Entity));
        }
        protected void Unequip(GearType slotType)
        {
            var actor = this.Owner as Actor;
            var slot = this.GetSlot(slotType);
            var item = slot.Object;
            ArgumentNullException.ThrowIfNull(item);
            // the inventory implicitly removes the item from its previous owner, so no need to clear the slot explicitly
            actor.Inventory.Insert(item);
            this.RefreshStats();
            actor.Net.Events.Post(new ActorGearUpdatedEvent(actor, null, item as Entity));
        }
        public bool EquipToggle(Entity item)
        {
            ArgumentNullException.ThrowIfNull(item);

            var actor = this.Owner as Actor;
            var slotType = item.Def.GearType;
            var gearSlot = actor.Gear.GetSlot(slotType);
            var previousItem = gearSlot.Object as Entity;

            if (item == previousItem) // we are implicitly told to unequip the item, assuming it is currently equipped
            {
                this.Unequip(slotType);
                Packets.SendUnequip(actor, slotType);
                return true;
            }

            //item.OnDespawn(); // in case the item is equipped from the world instead of from the inventory
            // DESPAWN BEFORE EQUIPPING because then the item's global become's the actor's global and the item is removed from the wrong chunk!
            Equip(item);
            Packets.SendEquip(actor, item);
            return true;
        }

        

        public void RefreshStats()
        {
            this.ArmorTotal = 0;
            foreach (var i in this.Equipment.Slots.Where(s => s.HasValue).Select(s => s.Object))
            {
                this.ArmorTotal += i.Def.ApparelProperties?.ArmorValue ?? 0;
            }
        }
        public new class Props : Props<GearComponent>
        {
            public GearType[] Slots;
            public Props(params GearType[] defs)
            {
                this.Slots = defs;
            }
        }
      
        [EnsureStaticCtorCall]
        static class Packets
        {
            static int _packetTypeIdEquip, _packetTypeIdUnequip;
            static Packets()
            {
                _packetTypeIdEquip = Registry.PacketHandlers.Register(ReceiveEquip);
                _packetTypeIdUnequip = Registry.PacketHandlers.Register(ReceiveUnequip);
            }
            static internal void SendEquip(Actor actor, Entity item)
            {
                var server = actor.Net as Server;
                server.BeginPacket(_packetTypeIdEquip)
                    .Write(actor.RefId)
                    .Write(item.RefId);
            }
            static internal void SendUnequip(Actor actor, GearType slot)
            {
                var server = actor.Net as Server;
                server.BeginPacket(_packetTypeIdUnequip)
                    .Write(actor.RefId)
                    .Write(slot);
            }
            static void ReceiveEquip(NetEndpoint net, Packet packet)
            {
                var client = net as Client;
                var r = packet.PacketReader;
                var actor = net.World.GetEntity(r.ReadInt32()) as Actor;
                var item = net.World.GetEntity(r.ReadInt32());
                actor.Gear.Equip(item);
            }
            static void ReceiveUnequip(NetEndpoint net, Packet packet)
            {
                var client = net as Client;
                var r = packet.PacketReader;
                var actor = net.World.GetEntity(r.ReadInt32()) as Actor;
                var slot = r.ReadDef<GearType>();
                actor.Gear.Unequip(slot);
            }
        }
    }
    class ActorGearUpdatedEvent(Actor actor, Entity newItem, Entity oldItem) : EventPayloadBase
    {
        public readonly Actor Actor = actor;
        public readonly Entity NewItem = newItem, OldItem = oldItem;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Project1.Core.UI;
using Project1.Core.Net;
using Project1.Core.Entities.Actors;
using Project1.Core.Base;
using Project1.Core.Helpers;
using Project1.Core.Gear;
using Project1.Core.Inventory;

namespace Project1.Core.Entities
{
    [EnsureStaticCtorCall]
    public class GearComponent : EntityComp
    {
        GearContainer Gear;
        public Container Equipment = new() { Name = "Equipment" };
        public float ArmorTotal;
        public override string Name { get; } = "Gear";
        public Entity this[GearTypeDef gearDef] => this.Gear.GetSlot(gearDef).Object as Entity;
        public override void OnObjectLoaded(GameObject parent)
        {
            base.OnObjectLoaded(parent);
        }

        internal override void Resolve()
        {
            var profile = this.Owner.Profile as ActorDnaDef;
            this.Gear = new(this.Owner as Actor);
            foreach (var slot in profile.Gear)
                this.Gear.Register(slot);
            this.Owner.RegisterContainer(this.Equipment);
        }
  
        public GearComponent()
        {
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
        public GameObject GetGear(GearTypeDef type) => this.Gear.GetSlotContent(type);
        //{
        //    return this.Equipment.GetSlot((int)type.ID).Object;
        //}
        public GameObjectSlot GetSlot(GearTypeDef type) => this.Gear.GetSlot(type);
        //{
        //    var slot = this.Equipment.GetSlot((int)type.ID);
        //    return slot;
        //}
        public GameObjectSlot GetSlot(GameObject item)
        {
            //var slot = this.Equipment.Slots.FirstOrDefault(s => s.Object == item);
            var slot = this.Gear.GetSlot(item);
            return slot;
        }
        internal override IEnumerable<GameObjectSlot> GetSlots()
        {
            //foreach (var slot in this.Equipment.Slots)
            //    yield return slot; 
            foreach (var slot in this.Gear.AllSlots)
                yield return slot;
        }
        //public static bool Equip(GameObject a, GameObject t)
        //{
        //    if (t is null)
        //        return false;
            
        //    var geartype = (int)t.GetComponent<EquipComponent>().Type.ID;

        //    GameObjectSlot gearSlot = a.GetComponent<GearComponent>().Equipment.Slots[geartype];

        //    gearSlot.Assign(t);
        //    return true;
        //}
        protected void Equip(Entity item)
        {
            if (!this.Owner.Inventory.Contains(item))
                throw new Exception();
            var slotType = item.Def.GearType;
            var slot = this.GetSlot(slotType);

            // the slot implictly removes the new item from the inventory or despawns it from the map and outputs the previous item that occupied the slot
            slot.Assign(item, out var previousItem);

            // the previousItem is currently detached from a parent but still exists, so we have to explicitly insert it in the inventory
            if(previousItem != null)
                this.Owner.Inventory.Insert(previousItem);

            this.RefreshStats();
            this.Owner.World.Events.Post(new ActorGearUpdatedEvent(this.Owner as Actor, item, previousItem as Entity));
        }
        protected void Unequip(GearTypeDef slotType)
        {
            var actor = this.Owner as Actor;
            var slot = this.GetSlot(slotType);
            var item = slot.Object;
            ArgumentNullException.ThrowIfNull(item);
            // the inventory implicitly removes the item from its previous owner, so no need to clear the slot explicitly
            actor.Inventory.Insert(item);
            this.RefreshStats();
            actor.World.Events.Post(new ActorGearUpdatedEvent(actor, null, item as Entity));
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
                // slots implicitly get synced
                //Packets.SendUnequip(actor, slotType);
                return true;
            }

            //item.OnDespawn(); // in case the item is equipped from the world instead of from the inventory
            // DESPAWN BEFORE EQUIPPING because then the item's global become's the actor's global and the item is removed from the wrong chunk!
            Equip(item);
            // slots implicitly get synced
            //Packets.SendEquip(actor, item);
            return true;
        }

        public override GroupBox GetGUI() => this.Gear.GetGui();
        

        public void RefreshStats()
        {
            this.ArmorTotal = 0;
            foreach (var i in this.Equipment.Slots.Where(s => s.HasValue).Select(s => s.Object))
            {
                this.ArmorTotal += i.Def.ApparelProperties?.ArmorValue ?? 0;
            }
        }
        public new class Spec : Spec<GearComponent>
        {
            public GearTypeDef[] Slots;
            public Spec(params GearTypeDef[] defs)
            {
                this.Slots = defs;
            }
            protected override void ApplyDefaultsTo(GearComponent comp)
            {
                //foreach (var slot in this.Slots)
                //    comp.Equipment.Slots.Add(new GameObjectSlot((byte)slot.ID) { ContainerNew = comp.Equipment, Name = slot.Name });
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
            static internal void SendUnequip(Actor actor, GearTypeDef slot)
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
                var slot = r.ReadDef<GearTypeDef>();
                actor.Gear.Unequip(slot);
            }
        }
    }
    record struct ActorGearUpdatedEvent(Actor Actor, Entity NewItem, Entity OldItem) : IEventPayload { }

    class GearContainer(Actor owner) : Inspectable
    {
        readonly Actor Owner = owner;
        readonly Dictionary<GearTypeDef, GameObjectSlot> Slots = [];
        public IEnumerable<GameObjectSlot> AllSlots => this.Slots.Values;
        public void Register(GearTypeDef def)
        {
            this.Slots.Add(def, new GameObjectSlot() { Owner = this.Owner });
        }
        public GameObjectSlot GetSlot(GameObject item) => this.Slots.Values.FirstOrDefault(s => s.Object == item);
        public GameObjectSlot GetSlot(GearTypeDef def) => this.Slots[def];
        public Entity GetSlotContent(GearTypeDef def) => this.Slots[def].Object as Entity;

        public GroupBox GetGui()
        {
            var box = new GroupBox();
            var table = new Table<(GearTypeDef def, GameObjectSlot slot)>()
                .AddColumn("geardef", 64, v => new LabelNew(v.def), 1)
                //.AddColumn("slot", 128, v => new LabelNew(() => v.slot.Object?.Name ?? ""), 0);
                .AddColumn("slot", 128, v => new LabelNew(() => v.slot.Object?.Name ?? "") { TooltipFunc = v.slot.GetTooltipInfo }.Bind(v.slot), 0);
            table.AddItems(this.Slots.Select(vk => (vk.Key, vk.Value)));
            box.Controls.Add(table);
            //this.Owner.World.Events.ListenTo<ActorGearUpdatedEvent>(onGearUpdated);
            //void onGearUpdated(ActorGearUpdatedEvent e)
            //{
            //    if (e.Actor == this.Owner)
            //        table.Invalidate(true);
            //}
            return box;
        }
    }
}

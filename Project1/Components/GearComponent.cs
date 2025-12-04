using Start_a_Town_.Components;
using Start_a_Town_.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Start_a_Town_
{
    public class GearComponent : EntityComponent
    {
        public override string Name { get; } = "Gear";

        public override void OnObjectLoaded(GameObject parent)
        {
            base.OnObjectLoaded(parent);
        }
        public override void OnObjectCreated(GameObject parent)
        {
            base.OnObjectCreated(parent);
        }
        public override void MakeChildOf(GameObject parent)
        {
            parent.RegisterContainer(this.Equipment);
        }
      
        public override void OnSpawn()
        {
            base.OnSpawn();
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
            using (var w = new BinaryWriter(new MemoryStream()))
            {
                this.Write(w);
                w.BaseStream.Position = 0;
                //using var r = new BinaryReader(w.BaseStream);
                using var r = new DataReader(w.BaseStream);
                comp.Read(r);
            }
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

        public override void Write(BinaryWriter writer)
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
                t.Despawn();

            // attempt to store current equipped item in inventory, otherwise drop it if inventory is full
            
            // equip new item
            gearSlot.Object = t;

            return true;
        }

        public static bool EquipToggle(Actor actor, Entity item)
        {
            if (item is null)
                return true;
            if (actor.IsEquipping(item))
            {
                //unequip
                actor.Inventory.Insert(item);
                actor.GetComponent<GearComponent>().RefreshStats();
                actor.Net.EventOccured((int)Message.Types.ActorGearUpdated, actor, null, item);
                return true;
            }
            var slotType = item.Def.GearType;
            var gearSlot = actor.Gear.GetSlot(slotType);
            var previousItem = gearSlot.Object as Entity;
            if (item == previousItem)
                return false;

            item.Despawn(); // in case the item is equipped from the world instead of from the inventory
                            // DESPAWN BEFORE EQUIPPING because then the item's global become's the actor's global and the item is despawned from the wrong chunk!

            gearSlot.SetObject(item);
            actor.GetComponent<GearComponent>().RefreshStats();
            if (previousItem != null)
                actor.Inventory.Insert(previousItem);
            actor.Net.EventOccured((int)Message.Types.ActorGearUpdated, actor, item, previousItem);
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

        public class Props : ComponentProps
        {
            public override Type CompClass => typeof(GearComponent);
            public GearType[] Slots;
            public Props(params GearType[] defs)
            {
                this.Slots = defs;
            }
        }
    }
}

using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Systems.Inventory;
using Project1.Core.Systems.Tools;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Gear;

[EnsureStaticCtorCall]
public sealed class GearComp : EntityComp
{
    public override EntityCompDef CompDef => EntityCompDefOf.Gear;
    GearContainer Gear;
    public Container Equipment = new() { Name = "Equipment" };
    public float ArmorTotal;
    readonly Dictionary<StatDef, float> Cache = [];
    bool StatsDirty = true;

    public override string Name { get; } = "Gear";
    public Entity this[GearSlotDef gearDef] => this.Gear.GetSlot(gearDef).Object as Entity;
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

    public GearComp()
    {
    }
    public override IEnumerable<Entity> GetChildren()
    {
        foreach (var o in this.Equipment.Slots.Where(s => s.HasValue).Select(s => s.Object as Entity))
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
    public Entity? GetGear(GearSlotDef slot) 
        => this.Gear.GetSlotContent(slot);
    public GameObjectSlot GetSlot(GearSlotDef type) => this.Gear.GetSlot(type);
    public GameObjectSlot GetSlot(GameObject item)
    {
        var slot = this.Gear.GetSlot(item);
        return slot;
    }

    internal override IEnumerable<GameObjectSlot> GetSlots()
    {
        foreach (var slot in this.Gear.AllSlots)
            yield return slot;
    }

    internal IEnumerable<Entity> GetGear()
        => this.Gear.AllSlots.Where(s => s.HasValue).Select(s => s.Object as Entity);

    internal void Equip(Entity item)
    {
        if (!this.Owner.Inventory.Contains(item))
            throw new Exception();
        //var slotType = item.Def.GearType;
        var gearProfile = (GearProfileDef)item.Profile;
        var gearRole = gearProfile.Role;
        var slotType = gearRole.Slot;
        var slot = this.GetSlot(slotType);

        // the slot implictly removes the new item from the inventory or despawns it from the map and outputs the previous item that occupied the slot
        slot.Assign(item, out var previousItem);

        // the previousItem is currently detached from a parent but still exists, so we have to explicitly insert it in the inventory
        if(previousItem is not null)
            this.Owner.Inventory.Insert(previousItem);

        this.RefreshStats();
        this.Owner.World.Events.Post(new ActorGearUpdatedEvent(this.Owner as Actor, item, previousItem as Entity));

        this.StatsDirty = true;
    }
    internal void Unequip(GearSlotDef slotType)
    {
        var actor = this.Owner as Actor;
        var slot = this.GetSlot(slotType);
        var item = slot.Object;
        ArgumentNullException.ThrowIfNull(item);
        // the inventory implicitly removes the item from its previous owner, so no need to clear the slot explicitly
        actor.Inventory.Insert(item);
        this.RefreshStats();
        actor.World.Events.Post(new ActorGearUpdatedEvent(actor, null, item as Entity));

        this.StatsDirty = true;
    }
    public bool EquipToggle(Entity item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var actor = this.Owner as Actor;
        var gearProfile = (GearProfileDef)item.Profile;
        var gearRole = gearProfile.Role;
        var slotType = gearRole.Slot;
        //var slotType = item.Def.GearType;
        var gearSlot = actor.Gear.GetSlot(slotType);
        var previousItem = gearSlot.Object as Entity;

        if (item == previousItem) // we are implicitly told to unequip the item, assuming it is currently equipped
        {
            this.Unequip(slotType);
            return true;
        }

        //item.OnDespawn(); // in case the item is equipped from the world instead of from the inventory
        // DESPAWN BEFORE EQUIPPING because then the item's global become's the actor's global and the item is removed from the wrong chunk!
        Equip(item);
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

    void RebuildStats()
    {
        if (!this.StatsDirty)
            return;
        this.StatsDirty = false;
        this.Cache.Clear();
        foreach(var gear in this.GetGear())
        {
            var gearProf = gear.Profile as GearProfileDef;
            var gearRole = gearProf.Role;
            foreach(var stat in gearRole.Stats)
            {
                var statValue = ToolSystem.CalculateStat(gear, stat).Value;
                this.Cache.AddOrUpdate(stat, statValue, existing => existing + statValue);
            }
        }
    }

    public new class Spec : Spec<GearComp>
    {
        public GearSlotDef[] Slots;
        public Spec(params GearSlotDef[] defs)
        {
            this.Slots = defs;
        }
        protected override void ApplyDefaultsTo(GearComp comp)
        {
        }
    }
}

using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Gear;
using Project1.Core.Systems.Materials;
using Project1.Framework;
using System;
using System.Collections.Generic;

namespace Project1.Core.Systems.Equipment;

internal static class EquipmentSystem
{
    static public Entity Create(EquipmentDef eqType, MaterialDef material)
    {
        var gear = ItemDefOf.GearOld.Create(profile: eqType);
        gear.Name = $"{material.LabelReadable} {eqType.LabelReadable}";
        //var comp = gear.GetComponent<EquipmentComp>();
        //var tier = material.Tier;
        //comp.Armor = (int)tier;
        gear.Body.Material = material;
        gear.ResolveReferences();
        return gear;
    }
    static public IEnumerable<Entity> GenerateTemplates()
    {
        var gearSlots = Def.Get<EquipmentDef>();
        foreach (var slot in gearSlots)
            foreach (var mat in MaterialSystem.TryGetMaterialsByType(slot.MaterialType))
                yield return Create(slot, mat);
    }

    static public void Validate(EquipmentComp comp)
    {
        var material = comp.Owner.PrimaryMaterial;
        var tier = material.Tier;
        comp.Armor = (int)tier;
    }

    internal static int GetArmor(Actor actor, GearSlotDef slot)
    {
        var actorGear = actor.GetComponent<GearComp>();
        var currentSlotGear = actorGear.GetGear(slot);
        return currentSlotGear?.GetComponent<EquipmentComp>().Armor ?? 0;
    }
}

public sealed class EquipmentDef(string name, GearSlotDef slot, MaterialTypeDef materialType) : Def(name)
{
    public GearSlotDef Slot = slot;
    public MaterialTypeDef MaterialType = materialType;
}

[EnsureStaticCtorCall]
public static class EquipmentDefOf
{
    public static readonly EquipmentDef Pants = new("Pants", GearSlotDefOf.Legs, MaterialTypeDefOf.Skin);
    public static readonly EquipmentDef Leggings = new("Leggings", GearSlotDefOf.Legs, MaterialTypeDefOf.Metal);
    public static readonly EquipmentDef Hat = new("Hat", GearSlotDefOf.Head, MaterialTypeDefOf.Skin);
    public static readonly EquipmentDef Helmet = new("Helmet", GearSlotDefOf.Head, MaterialTypeDefOf.Metal);
    public static readonly EquipmentDef Shoes = new("Shoes", GearSlotDefOf.Feet, MaterialTypeDefOf.Skin); 
    public static readonly EquipmentDef Boots = new("Boots", GearSlotDefOf.Feet, MaterialTypeDefOf.Metal);
    public static readonly EquipmentDef Shirt = new("Shirt", GearSlotDefOf.Chest, MaterialTypeDefOf.Skin);
    public static readonly EquipmentDef Chestguard = new("Chestguard", GearSlotDefOf.Chest, MaterialTypeDefOf.Metal);

    static EquipmentDefOf()
    {
        Def.Register(typeof(EquipmentDefOf));
    }
}

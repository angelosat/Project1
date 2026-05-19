using Project1.Core.Entities;
using Project1.Core.Gear;
using Project1.Core.Systems.Materials;
using Project1.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;

namespace Project1.Core.Systems.Equipment;

internal static class EquipmentSystem
{
    static public Entity Create(EquipmentDef eqType, MaterialDef material)
    {
        var gear = ItemDefOf.Gear.Create(profile: eqType);
        gear.Name = $"{material.LabelReadable} {eqType.LabelReadable}";
        return gear;
    }
    static public IEnumerable<Entity> GenerateTemplates()
    {
        var gearSlots = Def.Get<EquipmentDef>();
        foreach (var slot in gearSlots)
            foreach (var mat in MaterialSystem.TryGetMaterialsByType(slot.MaterialType))
                yield return Create(slot, mat);
        //return gearSlots.Select(Create);
    }
}

public sealed class EquipmentDef(string name, GearTypeDef slot, MaterialTypeDef materialType) : Def(name)
{
    public GearTypeDef Slot = slot;
    public MaterialTypeDef MaterialType = materialType;
}

[EnsureStaticCtorCall]
public static class EquipmentDefOf
{
    public static readonly EquipmentDef Pants = new("Pants", GearTypeDefOf.Legs, MaterialTypeDefOf.Skin);
    public static readonly EquipmentDef Leggings = new("Leggings", GearTypeDefOf.Legs, MaterialTypeDefOf.Metal);
    public static readonly EquipmentDef Hat = new("Hat", GearTypeDefOf.Head, MaterialTypeDefOf.Skin);
    public static readonly EquipmentDef Helmet = new("Helmet", GearTypeDefOf.Head, MaterialTypeDefOf.Metal);
    public static readonly EquipmentDef Shoes = new("Shoes", GearTypeDefOf.Feet, MaterialTypeDefOf.Skin); 
    public static readonly EquipmentDef Boots = new("Boots", GearTypeDefOf.Feet, MaterialTypeDefOf.Metal);
    public static readonly EquipmentDef Shirt = new("Shirt", GearTypeDefOf.Chest, MaterialTypeDefOf.Skin);
    public static readonly EquipmentDef Chestguard = new("Chestguard", GearTypeDefOf.Chest, MaterialTypeDefOf.Metal);

    static EquipmentDefOf()
    {
        Def.Register(typeof(EquipmentDefOf));
    }
}

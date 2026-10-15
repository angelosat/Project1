using Project1.Core.Gear;
using Project1.Core.Systems.Materials;
using Project1.Framework;

namespace Project1.Core.Systems.Equipment
{
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
}

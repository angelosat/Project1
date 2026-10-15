using Project1.Core.Gear;
using Project1.Core.Systems.Materials;

namespace Project1.Core.Systems.Equipment
{
    public sealed class EquipmentDef(string name, GearSlotDef slot, MaterialTypeDef materialType) : Def(name)
    {
        public GearSlotDef Slot = slot;
        public MaterialTypeDef MaterialType = materialType;
    }
}

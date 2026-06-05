namespace Project1.Core.Gear
{
    public class ApparelDef
    {
        public GearSlotDef GearType;
        public int ArmorValue;
        public ApparelDef(GearSlotDef gearType, int armorValue)
        {
            this.GearType = gearType;
            this.ArmorValue = armorValue;
        }
    }
}

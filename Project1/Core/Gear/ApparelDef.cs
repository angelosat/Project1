namespace Project1.Core.Gear
{
    public class ApparelDef
    {
        public GearTypeDef GearType;
        public int ArmorValue;
        public ApparelDef(GearTypeDef gearType, int armorValue)
        {
            this.GearType = gearType;
            this.ArmorValue = armorValue;
        }
    }
}

namespace Project1.Framework.Gear
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

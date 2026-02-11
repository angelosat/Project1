using Project1.Framework;

namespace Project1.Core.Materials
{
    public class Fuel : Inspectable
    {
        public readonly FuelDef Def;
        public float Value;

        public Fuel(FuelDef def, float value)
        {
            Def = def;
            Value = value;
        }
    }
}

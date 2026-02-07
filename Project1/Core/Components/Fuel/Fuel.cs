using Project1.Core.Base;

namespace Project1.Core.Components.Fuel
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

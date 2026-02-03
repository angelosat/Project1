using Start_a_Town_;

namespace Project1.Framework.Components.Fuel
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

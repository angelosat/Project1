namespace Start_a_Town_
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

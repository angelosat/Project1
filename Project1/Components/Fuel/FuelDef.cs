namespace Start_a_Town_
{
    public class FuelDef : Def
    {
        public FuelDef(string name) : base(name)
        {
          
        }
        static public readonly FuelDef Organic = new FuelDef("Organic");
    }
}

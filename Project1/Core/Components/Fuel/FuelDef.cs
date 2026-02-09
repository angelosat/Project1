namespace Project1.Core.Components.Fuel
{
    public class FuelDef : Def
    {
        public FuelDef(string name) : base(name)
        {
          
        }
        static public readonly FuelDef Organic = new FuelDef("Organic");
    }
}

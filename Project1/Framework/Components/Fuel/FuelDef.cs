using Project1.Framework.Base;

namespace Project1.Framework.Components.Fuel
{
    public class FuelDef : Def
    {
        public FuelDef(string name) : base(name)
        {
          
        }
        static public readonly FuelDef Organic = new FuelDef("Organic");
    }
}

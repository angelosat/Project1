using Project1.Framework.Base;

namespace Project1.Framework.Materials
{
    public class MaterialChemistryDef(string name) : Def(name)
    {
    }

    static public class MaterialChemistryDefOf
    {
        static public readonly MaterialChemistryDef Organic = new("Organic");
        static public readonly MaterialChemistryDef Inorganic = new("Inorganic");
    }
}

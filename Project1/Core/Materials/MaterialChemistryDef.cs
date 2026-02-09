namespace Project1.Core.Materials
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

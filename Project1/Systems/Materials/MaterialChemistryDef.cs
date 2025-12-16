using SharpDX.MediaFoundation.DirectX;

namespace Start_a_Town_
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

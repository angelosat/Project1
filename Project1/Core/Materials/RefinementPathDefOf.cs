using Project1.Framework.Base;
using Project1.Framework.Materials;
using Start_a_Town_;

namespace Project1.Core.Materials
{
    [EnsureStaticCtorCall]
    static public class RefinementPathDefOf
    {
        static public readonly RefinementPathDef Raw = new("Raw");//, RawMaterialDefOfNew.Raw);
        static public readonly RefinementPathDef Cut = new("Cut");//,, RawMaterialDefOfNew.Refined);
        static public readonly RefinementPathDef Shaped = new("Shaped");//,, RawMaterialDefOfNew.Processed);
        static public readonly RefinementPathDef Ground = new("Ground");//,, RawMaterialDefOfNew.Advanced);
        static public readonly RefinementPathDef Cast = new("Cast");//,, RawMaterialDefOfNew.Advanced);
        static public readonly RefinementPathDef Forged = new("Forged");//,, RawMaterialDefOfNew.Advanced);
        static public readonly RefinementPathDef Polished = new("Polished");//,, RawMaterialDefOfNew.Advanced);
        static public readonly RefinementPathDef Engraved = new("Engraved");//,, RawMaterialDefOfNew.Advanced);
        static RefinementPathDefOf() => Def.Register(typeof(RefinementPathDefOf));
    }
}

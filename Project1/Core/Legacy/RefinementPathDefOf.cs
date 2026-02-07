using Project1.Core.Base;
using Project1.Core.Helpers;
using Project1.Core.Legacy;

namespace Project1.Core.Legacy
{
    [EnsureStaticCtorCall]
    static public class RefinementPathDefOf
    {
        static public readonly RefinementPathDef Raw = new("Raw");
        static public readonly RefinementPathDef Cut = new("Cut");
        static public readonly RefinementPathDef Shaped = new("Shaped");
        static public readonly RefinementPathDef Ground = new("Ground");
        static public readonly RefinementPathDef Cast = new("Cast");
        static public readonly RefinementPathDef Forged = new("Forged");
        static public readonly RefinementPathDef Polished = new("Polished");
        static public readonly RefinementPathDef Engraved = new("Engraved");
        static RefinementPathDefOf() => Def.Register(typeof(RefinementPathDefOf));
    }
}

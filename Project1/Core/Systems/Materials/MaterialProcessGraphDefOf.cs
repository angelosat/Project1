using Project1.Framework;
using Project1.Core.Legacy;

namespace Project1.Core.Systems.Materials
{
    [EnsureStaticCtorCall]
    static public class MaterialProcessGraphDefOf
    {
        static public readonly MaterialProcessGraphDef Default = new("Default",
            [(RefinementPathDefOf.Raw, [RefinementPathDefOf.Shaped, RefinementPathDefOf.Cut, RefinementPathDefOf.Ground]),
            (RefinementPathDefOf.Cut, [RefinementPathDefOf.Shaped, RefinementPathDefOf.Ground]),
            (RefinementPathDefOf.Shaped, [RefinementPathDefOf.Ground])]
            );

        static MaterialProcessGraphDefOf() => Def.Register(typeof(MaterialProcessGraphDefOf));
    }
}

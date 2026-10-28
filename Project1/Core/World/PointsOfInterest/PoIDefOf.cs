using Project1.Framework;

namespace Project1.Core.World.PointsOfInterest;

[EnsureStaticCtorCall]
static public class PoIDefOf
{
    static public readonly PoIDef Stronghold = new("Stronghold");
    static public readonly PoIDef Dungeon = new("Dungeon");
    static public readonly PoIDef Fort = new("Fort");
    static public readonly PoIDef Castle = new("Castle");
    static public readonly PoIDef Cave = new("Cave");
    static public readonly PoIDef Mine = new("Mine");
    static public readonly PoIDef Nest = new("Nest");


    static PoIDefOf()
    {
        Def.Register(typeof(PoIDefOf));
    }
}
using Project1.Framework;

namespace Project1.Core.Entities
{
    [EnsureStaticCtorCall]
    static public class CraftsmanshipDefOf
    {
        static public readonly CraftsmanshipDef Crude = new("Crude", 1, 5);
        static public readonly CraftsmanshipDef Standard = new("Standard", 2, 40);
        static public readonly CraftsmanshipDef Fine = new("Fine", 3, 25);
        static public readonly CraftsmanshipDef Superior = new("Superior", 3, 15);
        static public readonly CraftsmanshipDef Exceptional = new("Exceptional", 4, 10);
        static public readonly CraftsmanshipDef Masterwork = new("Masterwork", 5, 4);
        static public readonly CraftsmanshipDef Peerless = new("Peerless", 6, 1);

        static CraftsmanshipDefOf()
        {
            Def.Register(typeof(CraftsmanshipDefOf));
        }
    }
}
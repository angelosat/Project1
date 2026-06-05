using Project1.Framework;

namespace Project1.Core.Gear;

[EnsureStaticCtorCall]
internal static class GearSlotDefOf
{
    public static readonly GearSlotDef Mainhand = new("Mainhand");
    public static readonly GearSlotDef Offhand = new("Offhand");
    public static readonly GearSlotDef Head = new("Head");
    public static readonly GearSlotDef Chest = new("Chest");
    public static readonly GearSlotDef Hands = new("Hands");
    public static readonly GearSlotDef Legs = new("Legs");
    public static readonly GearSlotDef Feet = new("Feet");
    static GearSlotDefOf()
    {
        Def.Register(typeof(GearSlotDefOf));
    }
}

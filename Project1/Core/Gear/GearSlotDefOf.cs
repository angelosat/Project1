using Project1.Framework;

namespace Project1.Core.Gear;

[EnsureStaticCtorCall]
internal static class GearSlotDefOf
{
    public static readonly GearSlotDef Mainhand = new("Mainhand");
    public static readonly GearSlotDef Offhand = new("Offhand");
    public static readonly GearSlotDef Head = new("Head") { AttackTargetWeight = 1 };
    public static readonly GearSlotDef Chest = new("Chest") { AttackTargetWeight = 4 };
    public static readonly GearSlotDef Hands = new("Hands") { AttackTargetWeight = 2 };
    public static readonly GearSlotDef Legs = new("Legs") { AttackTargetWeight = 3 };
    public static readonly GearSlotDef Feet = new("Feet") { AttackTargetWeight = 2 };
    static GearSlotDefOf()
    {
        Def.Register(typeof(GearSlotDefOf));
    }
}

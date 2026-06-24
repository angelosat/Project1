using Project1.Core.Gear;
using Project1.Core.Stats;
using Project1.Core.Systems.Gear;
using Project1.Framework;

namespace Project1.Core.Systems.Tools;

[EnsureStaticCtorCall]
static class GearRoleDefOf
{
    public static readonly GearRoleDef Tool = new("Tool", slot: GearSlotDefOf.Mainhand, bones: BoneStructureDefOf.Tool, stats: [StatDefOf.ToolSpeed, StatDefOf.ToolEffectiveness]);
    public static readonly GearRoleDef Head = new("Head", slot: GearSlotDefOf.Head, bones: BoneStructureDefOf.Armor, stats: [StatDefOf.Armor]);

    static GearRoleDefOf()
    {
        Def.Register(typeof(GearRoleDefOf));
    }
}

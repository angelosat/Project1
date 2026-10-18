using Project1.Core.Gear;
using Project1.Core.Stats;
using Project1.Core.Systems.Gear;
using Project1.Framework;

namespace Project1.Core.Systems.Tools;

[EnsureStaticCtorCall]
static class GearRoleDefOf
{
    public static readonly GearRoleDef Tool = new("Tool", slot: GearSlotDefOf.Mainhand, bones: BoneStructureDefOf.Tool, stats: [StatDefOf.ToolSpeed, StatDefOf.ToolEffectiveness]);
    public static readonly GearRoleDef Weapon = new("Weapon", slot: GearSlotDefOf.Mainhand, bones: BoneStructureDefOf.Tool, stats: [StatDefOf.ToolSpeed, StatDefOf.ToolEffectiveness]);
    public static readonly GearRoleDef Head = new("Head", slot: GearSlotDefOf.Head, bones: BoneStructureDefOf.Armor, stats: [StatDefOf.Armor]);
    public static readonly GearRoleDef Feet = new("Feet", slot: GearSlotDefOf.Feet, bones: BoneStructureDefOf.Armor, stats: [StatDefOf.Armor, StatDefOf.WalkSpeed]);
    public static readonly GearRoleDef Torso = new("Torso", slot: GearSlotDefOf.Chest, bones: BoneStructureDefOf.Armor, stats: [StatDefOf.Armor]);
    public static readonly GearRoleDef Legs = new("Legs", slot: GearSlotDefOf.Legs, bones: BoneStructureDefOf.Armor, stats: [StatDefOf.Armor]);

    static GearRoleDefOf()
    {
        Def.Register(typeof(GearRoleDefOf));
    }
}

using Project1.Core.Entities;
using Project1.Core.Gear;
using Project1.Core.Stats;
using Project1.Core.Systems.Gear;
using Project1.Framework.UI;
using System.Collections.Generic;

namespace Project1.Core.Systems.Tools;

//[EnsureStaticCtorCall]
static class GearRoleDefOf
{
    //public static readonly GearRoleDef Tool = new("Tool", slot: GearSlotDefOf.Mainhand, bones: BoneStructureDefOf.Tool, stats: [StatDefOf.ToolSpeed, StatDefOf.ToolEffectiveness]);
    public static readonly GearRoleDef MainHand = new("MainHand", slot: GearSlotDefOf.Mainhand, bones: BoneStructureDefOf.Tool, stats: [/*StatDefOf.ItemSpeed, StatDefOf.ItemPower*/])
    { 
        Worker = new GearRoleWorker_MainHand()
    };
    public static readonly GearRoleDef Head = new("Head", slot: GearSlotDefOf.Head, bones: BoneStructureDefOf.Armor, stats: [/*StatDefOf.Armor*/])
    {
        Worker = new GearRoleWorker_Apparel()
    };
    public static readonly GearRoleDef Feet = new("Feet", slot: GearSlotDefOf.Feet, bones: BoneStructureDefOf.Armor, stats: [/*StatDefOf.Armor, StatDefOf.WalkSpeed*/])
    {
        Worker = new GearRoleWorker_Feet()
    };
    public static readonly GearRoleDef Torso = new("Torso", slot: GearSlotDefOf.Chest, bones: BoneStructureDefOf.Armor, stats: [/*StatDefOf.Armor*/])
    {
        Worker = new GearRoleWorker_Apparel()
    };
    public static readonly GearRoleDef Legs = new("Legs", slot: GearSlotDefOf.Legs, bones: BoneStructureDefOf.Armor, stats: [/*StatDefOf.Armor*/])
    {
        Worker = new GearRoleWorker_Apparel()
    };

    static GearRoleDefOf()
    {
        Def.Register(typeof(GearRoleDefOf));
    }
}

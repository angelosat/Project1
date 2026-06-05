using Project1.Core.Animations;
using Project1.Core.Gear;
using Project1.Core.Systems.Tools;
using Project1.Framework;

#nullable enable

namespace Project1.Core.Systems.Gear;

//public class GearProfileDef(string name, GearSlotDef slot) : Def(name)
//{
//    public readonly GearSlotDef Slot = slot;
//    public readonly ToolUseDef? ToolUse;
//}

//static public class GearProfileDefOf
//{

//    static GearProfileDefOf()
//    {
//        Def.Register(typeof(GearProfileDefOf));
//    }
//}

public class BoneStructureDef(string name, params BoneDef[] bones) : Def(name)
{
    public readonly BoneDef[] Bones = bones;
}

[EnsureStaticCtorCall]
public static class BoneStructureDefOf
{
    public static readonly BoneStructureDef Tool = new("Tool", BoneDefOf.ToolHandle, BoneDefOf.ToolHead);
    public static readonly BoneStructureDef Armor = new("Armor", BoneDefOf.Item);

    static BoneStructureDefOf()
    {
        Def.Register(typeof(BoneStructureDefOf));
    }
}
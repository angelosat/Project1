using Microsoft.Xna.Framework;
using Project1.Core.Animations;
using Project1.Core.Assets;
using Project1.Core.Entities;
using Project1.Core.Gear;
using Project1.Core.Systems.Tools;
using Project1.Framework;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

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

public class BoneStructureDef(string name, Bone body, params BoneDef[] bones) : Def(name)
{
    public readonly BoneDef[] Bones = bones;
    public Bone Body = body;
}

[EnsureStaticCtorCall]
public static class BoneStructureDefOf
{
    public static readonly BoneStructureDef Tool = new("Tool",
        new Bone(BoneDefOf.ToolHandle, ItemContent.LogsGrayscale, Vector2.Zero, 0.001f) { DrawMaterialColor = true, OriginGroundOffset = new Vector2(0, -16) }
                        .AddJoint(Vector2.Zero, new Bone(BoneDefOf.ToolHead, ItemContent.LogsGrayscale) { DrawMaterialColor = true }),
            BoneDefOf.ToolHandle, BoneDefOf.ToolHead);

    public static readonly BoneStructureDef Armor = new("Armor", new Bone(BoneDefOf.Item, ItemContent.HelmetFull), BoneDefOf.Item);

    static BoneStructureDefOf()
    {
        Def.Register(typeof(BoneStructureDefOf));
    }
}
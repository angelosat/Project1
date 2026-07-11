using Microsoft.Xna.Framework;
using Project1.Core.Animations;
using Project1.Core.Assets;
using Project1.Core.Entities;
using Project1.Core.Gear;
using Project1.Core.Systems.Tools;
using Project1.Framework;
using System;
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

    static public readonly BoneStructureDef Npc = new("Npc",
    new Bone(BoneDefOf.Hips, BodyDef.hips) { OriginGroundOffset = new Vector2(0, -12) }
        .AddJoint(new Vector2(-2, 0), new Bone(BoneDefOf.RightFoot, BodyDef.rleg, -.002f))
        .AddJoint(new Vector2(2, 0), new Bone(BoneDefOf.LeftFoot, BodyDef.lleg, -.001f))
        .AddJoint(Vector2.Zero, new Bone(BoneDefOf.Torso, BodyDef.torso)
            .AddJoint(new Vector2(-1, -14), new Bone(BoneDefOf.Head, BodyDef.head, -.002f)
                .AddJoint(BoneDefOf.Helmet, new Joint(0, -6)))
            .AddJoint(new Vector2(5, -9), new Bone(BoneDefOf.LeftHand, BodyDef.lhand, .002f)
                .AddJoint(BoneDefOf.Offhand, new Joint(0, 4) { Angle = 5 * (float)Math.PI / 4f, AttachmentFunc = o => o.Gear.GetGear(GearSlotDefOf.Offhand) }))
            .AddJoint(new Vector2(-4, -9), new Bone(BoneDefOf.RightHand, BodyDef.rhand, -.004f)
                .AddJoint(BoneDefOf.Mainhand, new Joint(-2, 11) { Angle = 5 * (float)Math.PI / 4f, AttachmentFunc = o => o.Gear.GetGear(GearSlotDefOf.Mainhand) })
                .AddJoint(BoneDefOf.Hauled, new Joint(-2, 11) { Angle = (float)Math.PI, AttachmentFunc = o => o.Hauled }))));

    static public readonly BoneStructureDef Tree = new("Tree", new Bone(BoneDefOf.TreeTrunk, ItemContent.TreeFull).AddJoint(new Bone(BoneDefOf.PlantFruit) { DrawMaterialColor = true }));
    static public readonly BoneStructureDef Bush = new("Bush", new Bone(BoneDefOf.PlantStem, ItemContent.BerryBushGrowing).AddJoint(new Bone(BoneDefOf.PlantFruit) { DrawMaterialColor = true }));

    static public readonly BoneStructureDef Default = new("Default", new Bone(BoneDefOf.Item, ItemContent.BarsGrayscale));
    static BoneStructureDefOf()
    {
        Def.Register(typeof(BoneStructureDefOf));
    }
}
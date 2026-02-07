using Project1.Core.Base;
using Project1.Core.Helpers;

namespace Project1.Core.Animations
{
    public class BoneDef : Def
    {
        public BoneDef(string name) : base(name)
        {
        }
    }
    [EnsureStaticCtorCall]
    public static class BoneDefOf
    {
        static public readonly BoneDef Hips = new("Hips");
        static public readonly BoneDef Torso = new("Torso");
        static public readonly BoneDef RightHand = new("Right Hand");
        static public readonly BoneDef LeftHand = new("Left Hand");
        static public readonly BoneDef RightFoot = new("Right Foot");
        static public readonly BoneDef LeftFoot = new("Left Foot");
        static public readonly BoneDef Head = new("Head");
        static public readonly BoneDef Mainhand = new("Mainhand");
        static public readonly BoneDef Offhand = new("Offhand");
        static public readonly BoneDef Hauled = new("Hauled");
        static public readonly BoneDef Helmet = new("Helmet");
        static public readonly BoneDef ToolHead = new("ToolHead");
        static public readonly BoneDef ToolHandle = new("ToolHandle");
        static public readonly BoneDef Item = new("Item");
        static public readonly BoneDef PlantStem = new("Plant Stem");
        static public readonly BoneDef PlantFruit = new("Plant Fruit");
        static public readonly BoneDef TreeTrunk = new("Tree Trunk");

        static BoneDefOf()
        {
            Def.Register(typeof(BoneDefOf));
        }
    }
}

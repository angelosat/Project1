namespace Start_a_Town_
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
            //Def.Register(Hips);
            //Def.Register(Torso);
            //Def.Register(RightHand);
            //Def.Register(LeftHand);
            //Def.Register(RightFoot);
            //Def.Register(LeftFoot);
            //Def.Register(Head);
            //Def.Register(Mainhand);
            //Def.Register(Offhand);
            //Def.Register(Hauled);
            //Def.Register(Helmet);
            //Def.Register(ToolHead);
            //Def.Register(ToolHandle);
            //Def.Register(Item);
            //Def.Register(PlantStem);
            //Def.Register(TreeTrunk);
        }

        //static public readonly BoneDef Hips = new("Hips");
        //static public readonly BoneDef Torso = new("Torso");
        //static public readonly BoneDef RightHand = new("RightHand", "Right Hand");
        //static public readonly BoneDef LeftHand = new("LeftHand", "Left Hand");
        //static public readonly BoneDef RightFoot = new("RightFoot", "Right Foot");
        //static public readonly BoneDef LeftFoot = new("LeftFoot", "Left Foot");
        //static public readonly BoneDef Head = new("Head");
        //static public readonly BoneDef Mainhand = new("Mainhand");
        //static public readonly BoneDef Offhand = new("Offhand");
        //static public readonly BoneDef Hauled = new("Hauled");
        //static public readonly BoneDef Helmet = new("Helmet");
        //static public readonly BoneDef EquipmentHead = new("EquipmentHead", "Head");
        //static public readonly BoneDef EquipmentHandle = new("EquipmentHandle", "Handle");
        //static public readonly BoneDef Item = new("Item");
        //static public readonly BoneDef PlantStem = new("PlantStem", "Plant Stem");
        //static public readonly BoneDef TreeTrunk = new("TreeTrunk", "Tree Trunk");
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_
{
    static class ItemContent
    {
        static public readonly Texture2D BlockDepthMap = Game1.Instance.Content.Load<Texture2D>("Graphics/blockDepth09height19");

        static public readonly Sprite HelmetFull = new Sprite("helmet", new Vector2(16, 24), new Vector2(16, 16));

        static public readonly Sprite ShovelFull = new Sprite("shovel") { OriginGround = new Vector2(16, 32) };
        static public readonly Sprite ShovelHandle = new Sprite("shovel/shovelhandle") { OriginGround = new Vector2(16, 16) };
        static public readonly Sprite ShovelHead = new Sprite("shovel/shovelhead") { OriginGround = new Vector2(16, 16) };

        static public readonly Sprite AxeFull = new Sprite("axe") { OriginGround = new Vector2(16, 32) };
        static public readonly Sprite AxeHandle = new Sprite("axe/axeHandle") { OriginGround = new Vector2(16, 16) };
        static public readonly Sprite AxeHead = new Sprite("axe/axeHead") { OriginGround = new Vector2(16, 16) };

        static public readonly Sprite HammerFull = new Sprite("hammer") { OriginGround = new Vector2(16, 32) };
        static public readonly Sprite HammerHandle = new Sprite("hammer/hammerHandle", 0.5f) { OriginGround = new Vector2(16, 16) };
        static public readonly Sprite HammerHead = new Sprite("hammer/hammerHead", 0.5f) { OriginGround = new Vector2(16, 16) };

        static public readonly Sprite PickaxeFull = new Sprite("pickaxe") { OriginGround = new Vector2(16, 32) };
        static public readonly Sprite PickaxeHandle = new Sprite("pickaxe/pickaxeHandle") { OriginGround = new Vector2(16, 16) };
        static public readonly Sprite PickaxeHead = new Sprite("pickaxe/pickaxeHead") { OriginGround = new Vector2(16, 16) };

        static public readonly Sprite HandsawFull = new Sprite("handsaw/handsaw", 0.5f) { OriginGround = new Vector2(16, 32) };
        static public readonly Sprite HandsawHandle = new Sprite("handsaw/handsawhandle") { OriginGround = new Vector2(16, 16) };
        static public readonly Sprite HandsawHead = new Sprite("handsaw/handsawblade") { OriginGround = new Vector2(16, 16) };

        static public readonly Sprite HoeFull = new Sprite("hoe", 0) { OriginGround = new Vector2(16, 32) };
        static public readonly Sprite HoeHandle = new Sprite("hoe/hoeHandle", 0.5f) { OriginGround = new Vector2(16, 16) };
        static public readonly Sprite HoeHead = new Sprite("hoe/hoeHead", 0.5f) { OriginGround = new Vector2(16, 16) };

        static public readonly Sprite PlanksGrayscale = new Sprite("planksbw", new Vector2(16, 32), new Vector2(16, 24));//, new Vector2(16, 28), new Vector2(16, 24));
        static public readonly Sprite BagsGrayscale = new Sprite("soilbagbw", Block.BlockDepthMap) { OriginY = 8, OriginGround = new Vector2(16, 24), Joint = new Vector2(16, 24) };
        static public readonly Sprite BarsGrayscale = new Sprite("metalbars", Block.BlockDepthMap) { OriginY = 8, OriginGround = new Vector2(16, 24), Joint = new Vector2(16, 24) };
        static public readonly Sprite OreGrayscale = new Sprite("boulder", Block.BlockDepthMap) { OriginY = 8, OriginGround = new Vector2(16, 24) };
        static public readonly Sprite LogsGrayscale = new Sprite("logsbw", BlockDepthMap) { OriginY = 8, OriginGround = new Vector2(16, 24) };//, Joint = new Vector2(16, 24) }; //OriginGround = new Vector2(16, 24), Joint = new Vector2(16, 24) };

        static public readonly Sprite BerriesFull = new Sprite("berries", new Vector2(16, 32), new Vector2(16, 24));
        static public readonly Sprite SeedsFull = new Sprite("seeds", new Vector2(16, 32), new Vector2(16, 24));
        static public readonly Sprite Sapling = new Sprite("sapling", new Vector2(16, 32), new Vector2(16, 24));

        static public readonly Sprite BerryBushGrowing = new Sprite("berrybush1", BlockDepthMap) { OriginY = 8, OriginGround = new Vector2(30 / 2, 28 - 8) };
        //static public readonly Sprite BerryBushGrown = new Sprite("berrybush2", BlockDepthMap) { OriginY = 8, OriginGround = new Vector2(30 / 2, 28 - 8) };
        static public readonly Sprite BerryBushGrown = new Sprite("berrybush1", BlockDepthMap) { OriginY = 8, OriginGround = new Vector2(30 / 2, 28 - 8) };
        static public readonly Sprite BerryBushFruit = new Sprite("berrybushFruit", BlockDepthMap) { OriginY = 8, OriginGround = new Vector2(30 / 2, 28 - 8) };

        static public readonly Sprite TreeFull = new Sprite("trees/tree1g").SetGroundContact(new Vector2(.5f, 1));

        static public readonly Sprite SkeletonFull = new Sprite("mobs/skeleton/full", new Vector2(17 / 2, 38)).SetGroundContact(new Vector2(.5f, 1));
    }
}

using Project1.Core.Assets;
using Project1.Framework;

namespace Project1.Core.Input.Orders
{
    [EnsureStaticCtorCall]
    public static class OrderCommandDefOf
    {
        public static readonly OrderCommandDef Forbid = new("Forbid", ItemContent.BagsGrayscale, typeof(OrderCommandForbid));
        public static readonly OrderCommandDef RemoveDesignation = new("RemoveDesignation", ItemContent.Sapling, typeof(OrderCommandRemoveDesignation));
        public static readonly OrderCommandDef Mine = new("Mine", ItemContent.PickaxeHead, typeof(OrderCommandMine));
        public static readonly OrderCommandDef Chop = new("Chop", ItemContent.AxeHandle, typeof(OrderCommandChop));

        static OrderCommandDefOf()
        {
            Def.Register(typeof(OrderCommandDefOf));
        }
    }
}

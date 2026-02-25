using Project1.Core.Assets;
using Project1.Core.Entities;
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
        public static readonly OrderCommandDef DeleteZone = new("Delete", ItemContent.BerryBushFruit, typeof(OrderCommandDeleteZone));
        public static readonly OrderCommandDef ToggleTownMember = new("MakeTownMember", BodyDef.head, typeof(OrderToggleTownMember));
        public static readonly OrderCommandDef OrderTownMember = new("OrderTownMember", BodyDef.torso, typeof(OrderOrderTownMember));
        public static readonly OrderCommandDef ControlActor = new("ControlActor", BodyDef.hips, typeof(OrderControlActor));
        public static readonly OrderCommandDef Deconstruct = new("Deconstruct", ItemContent.HammerHead, typeof(OrderCommandDeconstruct));
        public static readonly OrderCommandDef CancelUnfinished = new("CancelUnfinished", ItemContent.HammerHandle, typeof(OrderCommandCancelUnfinished));

        static OrderCommandDefOf()
        {
            Def.Register(typeof(OrderCommandDefOf));
        }
    }
}

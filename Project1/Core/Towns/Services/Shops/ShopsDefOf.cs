using Project1.Core.Assets;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Input;
using Project1.Core.Input.Orders;
using Project1.Core.Screens;
using Project1.Core.UI;
using Project1.Framework;

namespace Project1.Core.Towns.Services.Shops;

sealed class OrderCommand_ToggleForSale : OrderCommandWorkerTogglable
{
    internal override bool CanIssue(ISelectable target)
        => target is Entity && target is not Actor;

    internal override void Issue(SelectionFinal selection)
    {
        Ingame.Instance.Events.Post(new PlayerItemToggledForSaleEvent(selection.Entities));
    }

    internal override bool IsToggled(ISelectable target)
        => (target as Entity).IsForSale;
    
}

[EnsureStaticCtorCall]
public static class ShopsDefOf
{
    public static readonly OrderCommandDef OrderToggleForSale = new("Sell", ItemContent.BarsGrayscale, typeof(OrderCommand_ToggleForSale));
    static ShopsDefOf()
    {
        Def.Register(typeof(ShopsDefOf));
    }
}

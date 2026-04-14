using Project1.Core.Blocks;
using Project1.Core.Input;
using Project1.Core.Input.Orders;
using Project1.Core.UI;
using Project1.Framework.UI;
using System.Linq;

namespace Project1.Core.Towns.Services;

internal class OrderCommand_AssignTownServiceToCounter : OrderCommandWorker
{
    internal override bool CanIssue(ISelectable target)
        => target is BlockEntity be && be.HasComp<BlockShopComp>();

    internal override void Issue(SelectionFinal selection)
    {
        var comp = selection.BlockEntities.First().GetComp<BlockShopComp>();
        var win = UIManager.ToggleSingleton<TownServiceAssignmentGui>("Assign service");
        win.Bind(comp);
        win.Window.MoveToScreenCenter();
    }
}

using Project1.Core.Entities.Actors;
using Project1.Core.UI;
using System.Linq;

namespace Project1.Core.Input.Orders
{
    internal sealed class OrderOrderTownMember : CommandWorker
    {
        //internal override bool CanIssue(IReadOnlyCollection<ISelectable> targets)
        //    => targets.Count == 1 && this.CanIssue(targets.First());
        internal override bool CanIssue(ISelectable target) => target is Actor actor && actor.Map.Town.IsMember(actor);

        internal override void Issue(OrderCommandRuntime runtime, SelectionFinal selection)
            => ToolManager.SetTool(new ToolCommandNpc([.. selection.Targets.OfType<Actor>()]));
    }
}

using Project1.Core.Entities.Actors;
using Project1.Core.Screens;
using Project1.Core.UI;
using System.Linq;

namespace Project1.Core.Input.Orders
{
    internal sealed class OrderToggleTownMember : CommandWorker
    {
        internal override bool CanIssue(ISelectable target) => target is Actor;
        internal override void Issue(OrderCommandRuntime runtime, SelectionFinal selection)
        {
            Ingame.Instance.Events.Post(new PlayerTogglingTownMembersEvent([.. selection.Targets.OfType<Actor>()]));
        }
    }
}

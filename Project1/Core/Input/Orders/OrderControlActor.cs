using Project1.Core.Entities.Actors;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Core.UI;
using System.Linq;

namespace Project1.Core.Input.Orders
{
    internal sealed class OrderControlActor : OrderCommandWorker
    {
        //internal override bool CanIssue(IReadOnlyCollection<ISelectable> targets)
        //    => targets.Count == 1 && this.CanIssue(targets.First());
        internal override bool CanIssue(ISelectable target) => target is Actor;
        internal override void Issue(SelectionFinal selection)
            => Ingame.Instance.Events.Post(new PlayerControlActorRequestEvent(Client.Instance.CurrentPlayer, selection.Targets.FirstOrDefault() as Actor));
    }
}

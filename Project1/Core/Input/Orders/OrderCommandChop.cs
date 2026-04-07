using Project1.Core.Screens;
using Project1.Core.Towns.Designations;
using Project1.Core.UI;

namespace Project1.Core.Input.Orders
{
    internal sealed class OrderCommandChop : CommandWorker
    {
        internal override bool CanIssue(ISelectable target) 
            => !target.Map.Town.DesignationManager.IsDesignation(target) && DesignationDefOf.Chop.Worker.IsValid(target);
        internal override void Issue(OrderCommandRuntime runtime, SelectionFinal selection)
            => Ingame.Instance.Events.Post(new PlayerDesignationEntitiesEvent(DesignationDefOf.Chop, Ingame.Net.MainViewport.Map.ID, selection.Entities, false));
    }
}

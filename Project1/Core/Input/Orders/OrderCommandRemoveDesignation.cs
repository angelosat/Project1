using Project1.Core.Screens;
using Project1.Core.Towns.Designations;
using Project1.Core.UI;

namespace Project1.Core.Input.Orders
{
    internal sealed class OrderCommandRemoveDesignation : OrderCommandWorker
    {
        internal override bool CanIssue(ISelectable target) //=> target.Map.Town.DesignationManager.IsDesignation(target);
            => target.Map.Town.DesignationManager.GetDesignation(target)?.IsManual ?? false;
        //internal override bool CanIssue(IReadOnlyCollection<ISelectable> targets)
        //    => targets.Any(this.CanIssue);
        internal override void Issue(SelectionFinal selection)
        {
            if(selection.Begin.HasValue)
                Ingame.Instance.Events.Post(new PlayerDesignationCellsEvent(null, Ingame.Net.MainViewport.Map.ID, selection.Begin.Value, selection.End.Value, true));
            else if(selection.Entities.Count != 0)
                Ingame.Instance.Events.Post(new PlayerDesignationEntitiesEvent(null, Ingame.Net.MainViewport.Map.ID, selection.Entities, true));
        }
    }
}

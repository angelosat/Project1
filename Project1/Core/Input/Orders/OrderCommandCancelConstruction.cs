using Project1.Core.Blocks;
using Project1.Core.Screens;
using Project1.Core.Towns.Constructions;
using Project1.Core.Towns.Designations;
using Project1.Core.UI;
using System.Linq;

namespace Project1.Core.Input.Orders
{
    internal sealed class OrderCommandCancelConstruction : CommandWorker
    {
        internal override bool CanIssue(ISelectable target)
            => target is BlockEntity construction && target.Map.Town.DesignationManager.GetDesignation(construction) == DesignationDefOf.Construct;
        internal override void Issue(OrderCommandRuntime runtime, SelectionFinal selection)
        {
            //if (selection.Begin.HasValue)
            //    Ingame.Instance.Events.Post(new PlayerCancelledConstructionEvent([.. selection.Cells]));
            //else
            //{
                if (selection.Targets.Count == 1 && selection.Targets.First() is CellSelection construction)
                //Ingame.Instance.Events.Post(new PlayerDesignationCellsEvent(DesignationDefOf.Deconstruct, be.OriginGlobal, be.OriginGlobal, false));
                    Ingame.Instance.Events.Post(new PlayerCancelledConstructionEvent([construction.Global]));
            //}
        }
    }
}

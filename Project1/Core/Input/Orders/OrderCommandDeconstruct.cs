using Project1.Core.Blocks;
using Project1.Core.Screens;
using Project1.Core.Towns.Designations;
using Project1.Core.UI;
using Project1.Framework;
using System.Linq;

namespace Project1.Core.Input.Orders
{
    internal sealed class OrderCommandDeconstruct : CommandWorker
    {
        internal override bool CanIssue(ISelectable target)
            => !target.Map.Town.DesignationManager.IsDesignation(target) && DesignationDefOf.Deconstruct.Worker.IsValid(target);
        internal override void Issue(OrderCommandRuntime runtime, SelectionFinal selection)
        {
            if(selection.Begin.HasValue)
                Ingame.Instance.Events.Post(new PlayerDesignationCellsEvent(DesignationDefOf.Deconstruct, selection.Begin.Value, selection.End.Value, false));
            else
            {
                if(selection.Targets.Count == 1 && selection.Targets.First() is BlockEntity be)
                    Ingame.Instance.Events.Post(new PlayerDesignationCellsEvent(DesignationDefOf.Deconstruct, be.OriginGlobal, be.OriginGlobal, false));
            }
        }
    }
}

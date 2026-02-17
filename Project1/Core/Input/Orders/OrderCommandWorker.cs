using Project1.Core.Towns.Designations;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Input.Orders
{
    internal abstract class OrderCommandWorker
    {
        internal abstract bool CanIssue(TargetArgs target);
        internal abstract void Execute(List<TargetArgs> targets);
    }
    internal class OrderCommandMine : OrderCommandWorker
    {
        internal override bool CanIssue(TargetArgs target) => DesignationDefOf.Mine.Worker.IsValid(target);

        internal override void Execute(List<TargetArgs> targets)
        {
            var map = targets.First().Map;
            map.Town.DesignationManager.Add(DesignationDefOf.Mine, targets, false);
        }
    }
    internal class OrderCommandRemoveDesignation : OrderCommandWorker
    {
        internal override bool CanIssue(TargetArgs target) => target.Map.Town.DesignationManager.IsDesignation(target);

        internal override void Execute(List<TargetArgs> targets)
        {
            var map = targets.First().Map;
            map.Town.DesignationManager.Add(null, targets, true);
        }
    }
}

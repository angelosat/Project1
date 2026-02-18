using Project1.Core.Simulation;
using Project1.Core.Towns.Designations;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Input.Orders
{
    internal abstract class OrderCommandWorker
    {
        internal abstract bool CanIssue(TargetArgs target);
        internal void Execute(MapBase map, SelectionIntent selection) => this.Execute(selection.ResolveTargets(map));
        protected abstract void Execute(IEnumerable<TargetArgs> targets);
    }
    internal sealed class OrderCommandMine : OrderCommandWorker
    {
        internal override bool CanIssue(TargetArgs target) => DesignationDefOf.Mine.Worker.IsValid(target);
        protected override void Execute(IEnumerable<TargetArgs> targets)
        {
            var map = targets.First().Map;
            map.Town.DesignationManager.Add(DesignationDefOf.Mine, targets, false);
        }
    }
    internal sealed class OrderCommandRemoveDesignation : OrderCommandWorker
    {
        internal override bool CanIssue(TargetArgs target) => target.Map.Town.DesignationManager.IsDesignation(target);
        protected override void Execute(IEnumerable<TargetArgs> targets)
        {
            var map = targets.First().Map;
            map.Town.DesignationManager.Add(null, targets, true);
        }
    }
}

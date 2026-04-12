using Project1.Core.Simulation;
using Project1.Core.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Project1.Core.Input.Orders
{
    internal abstract class OrderCommandWorker
    {
        internal abstract void Issue(OrderCommandRuntime runtime, SelectionFinal selection);
        internal abstract bool CanIssue(ISelectable target);
        internal bool CanIssue(ValidSelectedCount validCount, IReadOnlyCollection<ISelectable> targets)
            => validCount switch
            {
                ValidSelectedCount.Any => targets.Any(this.CanIssue),
                ValidSelectedCount.Single => targets.Count == 1 && this.CanIssue(targets.First()),
                _ => throw new UnreachableException()
            };
        [Obsolete]
        protected virtual void Execute(MapBase map, IEnumerable<ISelectable> targets) { }
        [Obsolete]
        internal void Execute(MapBase map, SelectionIntent selection) => this.Execute(map, selection.Resolve(map));

    }
}

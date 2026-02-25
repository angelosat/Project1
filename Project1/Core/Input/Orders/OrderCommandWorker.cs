using Project1.Core.Simulation;
using Project1.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Input.Orders
{
    internal abstract class CommandWorker
    {
        internal abstract void Issue(OrderCommandRuntime runtime, SelectionFinal selection);
        internal abstract bool CanIssue(ISelectable target);
        internal virtual bool CanIssue(IReadOnlyCollection<ISelectable> targets) => targets.Any(this.CanIssue);
        [Obsolete]
        protected virtual void Execute(MapBase map, IEnumerable<ISelectable> targets) { }
        [Obsolete]
        internal void Execute(MapBase map, SelectionIntent selection) => this.Execute(map, selection.Resolve(map));

    }
}

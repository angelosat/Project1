using Project1.Core.Entities;
using Project1.Core.Simulation;
using Project1.Core.Towns.Designations;
using Project1.Core.UI;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Input.Orders
{
    internal abstract class OrderCommandWorker
    {
        internal abstract bool CanIssue(ISelectable target);
        internal void Execute(MapBase map, SelectionIntent selection)
        {
            this.Execute(map, selection.Resolve(map));
            map.Events.Post(new PlayerExecutedOrderCommentEvent());
        }
        protected abstract void Execute(MapBase map, IEnumerable<ISelectable> targets);
    }
    internal sealed class OrderCommandMine : OrderCommandWorker
    {
        internal override bool CanIssue(ISelectable target) 
            => !target.Map.Town.DesignationManager.IsDesignation(target) && DesignationDefOf.Mine.Worker.IsValid(target);
        protected override void Execute(MapBase map, IEnumerable<ISelectable> targets) 
            => map.Town.DesignationManager.Add(DesignationDefOf.Mine, targets, false);
    }
    internal sealed class OrderCommandChop : OrderCommandWorker
    {
        internal override bool CanIssue(ISelectable target) => DesignationDefOf.Chop.Worker.IsValid(target);
        protected override void Execute(MapBase map, IEnumerable<ISelectable> targets)
            => map.Town.DesignationManager.Add(DesignationDefOf.Chop, targets, false);
    }
    internal sealed class OrderCommandRemoveDesignation : OrderCommandWorker
    {
        internal override bool CanIssue(ISelectable target) => target.Map.Town.DesignationManager.IsDesignation(target);
        protected override void Execute(MapBase map, IEnumerable<ISelectable> targets)
            => map.Town.DesignationManager.Remove(targets);
    }
    internal sealed class OrderCommandForbid : OrderCommandWorker
    {
        internal override bool CanIssue(ISelectable target) => target is Entity entity && entity.IsForbiddable();
        protected override void Execute(MapBase map, IEnumerable<ISelectable> targets)
        {
            foreach (var entity in targets.OfType<Entity>())
                entity.ToggleForbidden();
        }
    }
}

using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Core.Towns.Designations;
using Project1.Core.Towns.Zones;
using Project1.Core.UI;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Input.Orders
{
    internal abstract class CommandWorker
    {
        internal abstract void Issue(OrderCommandRuntime runtime, SelectionIntent selection);
        internal abstract bool CanIssue(ISelectable target);
        internal virtual bool CanIssue(IReadOnlyCollection<ISelectable> targets) => targets.Any(this.CanIssue);
        protected abstract void Execute(MapBase map, IEnumerable<ISelectable> targets);
        internal void Execute(MapBase map, SelectionIntent selection) => this.Execute(map, selection.Resolve(map));

    }
    internal abstract class OrderCommandWorker : CommandWorker
    {
        internal sealed override void Issue(OrderCommandRuntime runtime, SelectionIntent selection)
        {
            Ingame.Instance.Events.Post(new PlayerIssuedOrderCommandEvent(runtime.Def, selection));
        }
        //internal void Execute(MapBase map, SelectionIntent selection) => this.Execute(map, selection.Resolve(map));
    }
    internal abstract class UICommandWorker : CommandWorker
    { 
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
    internal sealed class OrderCommandDeleteZone : OrderCommandWorker
    {
        internal override bool CanIssue(ISelectable target) => target is Zone;
        protected override void Execute(MapBase map, IEnumerable<ISelectable> targets)
            => map.Town.ZoneManager.DeleteZone(targets.OfType<CellSelection>().Select(c=>c.Global).First());
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
    internal sealed class OrderToggleTownMember : OrderCommandWorker
    {
        internal override bool CanIssue(ISelectable target) => target is Actor actor;
        protected override void Execute(MapBase map, IEnumerable<ISelectable> targets)
        {
            foreach (var actor in targets.OfType<Actor>())
                map.Town.ToggleMember(actor);
        }
    }
    internal sealed class OrderControlTownMember : OrderCommandWorker
    {
        protected override void Execute(MapBase map, IEnumerable<ISelectable> targets)
        {
            ToolManager.SetTool(new ToolCommandNpc([.. targets.OfType<Actor>()]));
        }
        internal override bool CanIssue(IReadOnlyCollection<ISelectable> targets)
            => targets.Count == 1 && this.CanIssue(targets.First());
        internal override bool CanIssue(ISelectable target) => target is Actor actor && actor.Map.Town.IsMember(actor);
    }
}

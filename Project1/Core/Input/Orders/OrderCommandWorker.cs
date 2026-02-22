using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Core.Towns.Designations;
using Project1.Core.Towns.Zones;
using Project1.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

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
    internal abstract class OrderCommandWorker : CommandWorker
    {
        internal sealed override void Issue(OrderCommandRuntime runtime, SelectionFinal selection) { }
    }
    internal abstract class UICommandWorker : CommandWorker
    { 
    }
    internal sealed class OrderCommandDeconstruct : UICommandWorker
    {
        internal override bool CanIssue(ISelectable target)
            => !target.Map.Town.DesignationManager.IsDesignation(target) && DesignationDefOf.Deconstruct.Worker.IsValid(target);
        //internal override void Issue(OrderCommandRuntime runtime, SelectionFinal selection)
        //    => Ingame.Instance.Events.Post(new PlayerDesignationCellsEvent(DesignationDefOf.Deconstruct, selection.Begin.Value, selection.End.Value, false));
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
    internal sealed class OrderCommandMine : UICommandWorker
    {
        internal override bool CanIssue(ISelectable target)
            => !target.Map.Town.DesignationManager.IsDesignation(target) && DesignationDefOf.Mine.Worker.IsValid(target);
        internal override void Issue(OrderCommandRuntime runtime, SelectionFinal selection)
            => Ingame.Instance.Events.Post(new PlayerDesignationCellsEvent(DesignationDefOf.Mine, selection.Begin.Value, selection.End.Value, false));
    }
    internal sealed class OrderCommandChop : UICommandWorker
    {
        internal override bool CanIssue(ISelectable target) 
            => !target.Map.Town.DesignationManager.IsDesignation(target) && DesignationDefOf.Chop.Worker.IsValid(target);
        internal override void Issue(OrderCommandRuntime runtime, SelectionFinal selection)
            => Ingame.Instance.Events.Post(new PlayerDesignationEntitiesEvent(DesignationDefOf.Chop, selection.Entities, false));
    }
    internal sealed class OrderCommandRemoveDesignation : UICommandWorker
    {
        internal override bool CanIssue(ISelectable target) => target.Map.Town.DesignationManager.IsDesignation(target);
        internal override bool CanIssue(IReadOnlyCollection<ISelectable> targets)
            => targets.Any(this.CanIssue);
        internal override void Issue(OrderCommandRuntime runtime, SelectionFinal selection)
        {
            if(selection.Begin.HasValue)
                Ingame.Instance.Events.Post(new PlayerDesignationCellsEvent(null, selection.Begin.Value, selection.End.Value, true));
            else if(selection.Entities.Count != 0)
                Ingame.Instance.Events.Post(new PlayerDesignationEntitiesEvent(null, selection.Entities, true));
        }
    }
    internal sealed class OrderCommandDeleteZone : UICommandWorker
    {
        internal override bool CanIssue(ISelectable target) => target is Zone;
        protected override void Execute(MapBase map, IEnumerable<ISelectable> targets)
            => map.Town.ZoneManager.DeleteZone(targets.OfType<CellSelection>().Select(c=>c.Global).First());
        internal override void Issue(OrderCommandRuntime runtime, SelectionFinal selection)
        {
            if (selection.Zone is Zone zone)
                Ingame.Instance.Events.Post(new PlayerDeletingZoneEvent(zone));
        }
    }
    internal sealed class OrderCommandForbid : UICommandWorker
    {
        internal override bool CanIssue(ISelectable target) => target is Entity entity && entity.IsForbiddable();
        internal override void Issue(OrderCommandRuntime runtime, SelectionFinal selection)
            => Ingame.Instance.Events.Post(new PlayerForbiddingItemsEvent(selection.Entities));
    }
    internal sealed class OrderToggleTownMember : UICommandWorker
    {
        internal override bool CanIssue(ISelectable target) => target is Actor;
        internal override void Issue(OrderCommandRuntime runtime, SelectionFinal selection)
        {
            Ingame.Instance.Events.Post(new PlayerTogglingTownMembersEvent([.. selection.Targets.OfType<Actor>()]));
        }
    }
    internal sealed class OrderOrderTownMember : UICommandWorker
    {
        //protected override void Execute(MapBase map, IEnumerable<ISelectable> targets)
        //{
        //    ToolManager.SetTool(new ToolCommandNpc([.. targets.OfType<Actor>()]));
        //}
        internal override bool CanIssue(IReadOnlyCollection<ISelectable> targets)
            => targets.Count == 1 && this.CanIssue(targets.First());
        internal override bool CanIssue(ISelectable target) => target is Actor actor && actor.Map.Town.IsMember(actor);

        internal override void Issue(OrderCommandRuntime runtime, SelectionFinal selection)
            => ToolManager.SetTool(new ToolCommandNpc([.. selection.Targets.OfType<Actor>()]));
    }
    internal sealed class OrderControlActor : UICommandWorker
    {
        internal override bool CanIssue(IReadOnlyCollection<ISelectable> targets)
            => targets.Count == 1 && this.CanIssue(targets.First());
        internal override bool CanIssue(ISelectable target) => target is Actor;

        internal override void Issue(OrderCommandRuntime runtime, SelectionFinal selection)
            => Ingame.Instance.Events.Post(new PlayerControlActorRequestEvent(Client.Instance.CurrentPlayer, selection.Targets.FirstOrDefault() as Actor));
    }
}

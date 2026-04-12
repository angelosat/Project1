using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Assets;
using Project1.Core.Blocks;
using Project1.Core.Entities.Actors;
using Project1.Core.Input;
using Project1.Core.Input.Orders;
using Project1.Core.Interactions;
using Project1.Core.Screens;
using Project1.Core.UI;
using Project1.Framework;
using Project1.Framework.Events;
using System.Linq;

namespace Project1.Core.Towns.Services.Inns;

[EnsureStaticCtorCall]
public static class InnsDefOf
{
    public static readonly OrderCommandDef OrderToggleInnBed = new("ToggleInnBed", ItemContent.BerryBushFruit, typeof(OrderCommandToggleInnBed), ValidSelectedCount.Single);
    public static readonly InteractionDef InteractionCheckIn = new("CheckingIn", typeof(InteractionCheckIn), InteractionControllers.ExternalFull);
    public static readonly InteractionDef InteractionWaitPayForBed = new("WaitForPay", typeof(InteractionWaitPayForBed), InteractionControllers.ExternalFull);
    //public static readonly InteractionDef InteractionRegisterGuest = new("RegisteringGuest", typeof(InteractionRegisterInnGuest), InteractionControllers.ExternalFull);
    public static readonly InteractionDef InteractionRegisterGuest = new("RegisteringGuest", typeof(InteractionRegisterInnGuest), InteractionControllers.Timed);
    public static readonly PlanDef PlanCheckIn = new("CheckIn", typeof(BehaviorExecutePlanNew), InteractionCheckIn);
    public static readonly PlanDef PlanPayCheckIn = new("PayCheckIn", typeof(BehaviorExecutePlanNew), InteractionDefOf.PayForBed);
    public static readonly PlanDef PlanRegisterGuest = new("RegisteringGuest", typeof(BehaviorExecutePlanNew), InteractionRegisterGuest);
    public static readonly PlanDef PlanWaitForPayForBed = new("WaitForPayForBed", typeof(BehaviorExecutePlanNew), InteractionWaitPayForBed);
    static InnsDefOf()
    {
        Def.Register(typeof(InnsDefOf));
    }
}

sealed class OrderCommandToggleInnBed : CommandWorker
{
    internal override bool CanIssue(ISelectable target)
        => target is BlockEntity be && be.HasComp<BlockBedComp>();

    internal override void Issue(OrderCommandRuntime runtime, SelectionFinal selection)
    {
        if (selection.Targets.Count == 1 && selection.Targets.First() is BlockEntity be && be.HasComp<BlockBedComp>())
            Ingame.Instance.Events.Post(new PlayerToggledInnBedEvent(Ingame.Net.MainViewport.Map.ID, be.OriginGlobal));
    }
}
public record struct PlayerToggledInnBedEvent(MapId MapId, IntVec3 Bed) : IEventPayload { }
public static class InnHelpers
{
    extension(Actor visitor)
    {
        public bool HasCheckedIn => visitor.Map.Town.Inns.HasProfile(visitor);
    }
}

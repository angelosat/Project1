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

namespace Project1.Core.Towns.Inns
{
    [EnsureStaticCtorCall]
    public static class InnsDefOf
    {
        public static readonly OrderCommandDef OrderToggleInnBed = new("ToggleInnBed", ItemContent.BerryBushFruit, typeof(OrderCommandToggleInnBed), ValidSelectedCount.Single);
        //public static readonly InteractionDef InteractionCheckIn = new("CheckingIn", typeof(InteractionCheckIn), InteractionProgressHandlers.Passive);
        public static readonly InteractionDef InteractionCheckIn = new("CheckingIn", typeof(InteractionCheckIn), InteractionProgressHandlers.ExternalFull);
        //public static readonly InteractionDef InteractionRegisterGuest = new("RegisteringGuest", typeof(InteractionRegisterInnGuest), InteractionProgressHandlers.Timed);
        public static readonly InteractionDef InteractionRegisterGuest = new("RegisteringGuest", typeof(InteractionRegisterInnGuest), InteractionProgressHandlers.ExternalFull);
        public static readonly PlanDef PlanCheckIn = new("CheckIn", typeof(BehaviorExecutePlanNew), InteractionCheckIn);
        public static readonly PlanDef PlanRegisterGuest = new("RegisteringGuest", typeof(BehaviorExecutePlanNew), InteractionRegisterGuest);
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
                Ingame.Instance.Events.Post(new PlayerToggledInnBedEvent(be.OriginGlobal));
        }
    }
    public record struct PlayerToggledInnBedEvent(IntVec3 Bed) : IEventPayload { }
    public static class InnHelpers
    {
        extension(Actor visitor)
        {
            public bool HasCheckedIn => visitor.Map.Town.InnManager.HasProfile(visitor);
        }
    }
}

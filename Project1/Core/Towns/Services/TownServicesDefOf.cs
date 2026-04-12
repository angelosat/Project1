using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Assets;
using Project1.Core.Input.Orders;
using Project1.Core.Interactions;
using Project1.Core.Towns.Services.Repairing;
using Project1.Framework;

namespace Project1.Core.Towns.Services;

[EnsureStaticCtorCall]
public static class TownServicesDefOf
{
    public static readonly InteractionDef InteractionQueue = new("Queuing", typeof(Interaction_Customer_Queue), InteractionControllers.ExternalFull);
    public static readonly InteractionDef InteractionWaitItemSubmit = new("Serving", typeof(Interaction_Vendor_WaitItemSubmit), InteractionControllers.ExternalFull);
    public static readonly InteractionDef InteractionWaitMoney = new("WaitMoney", typeof(Interaction_Vendor_WaitPayment), InteractionControllers.ExternalFull);
    public static readonly PlanDef PlanQueue = new("Queue", typeof(BehaviorExecutePlanNew), InteractionQueue);
    public static readonly PlanDef PlanWaitItemSubmit = new("RepairWaitItem", typeof(BehaviorExecutePlanNew), InteractionWaitItemSubmit);
    public static readonly PlanDef PlanWaitMoney = new("WaitMoney", typeof(BehaviorExecutePlanNew), InteractionWaitMoney);
    public static readonly OrderCommandDef OrderAssignServiceToCounter = new("OrderAssignServiceToCounter", ItemContent.HoeHead, typeof(OrderCommand_AssignTownServiceToCounter));
    static TownServicesDefOf()
    {
        Def.Register(typeof(TownServicesDefOf));
    }
}

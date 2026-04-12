using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Planners;
using Project1.Core.Interactions;
using Project1.Framework;

namespace Project1.Core.Towns.Services.Repairing;

[EnsureStaticCtorCall]
public static class ServiceRepairsDefOf
{
    public static readonly InteractionDef InteractionQueue = new("Queuing", typeof(Interaction_RepairCustomer_Queue), InteractionControllers.ExternalFull);
    public static readonly InteractionDef InteractionWaitPrice = new("WaitPrice", typeof(Interaction_RepairCustomer_WaitPriceAnnounce), InteractionControllers.ExternalFull);
    public static readonly InteractionDef InteractionServing = new("Serving", typeof(Interaction_RepairVendor_WaitItemSubmit), InteractionControllers.ExternalFull);
    public static readonly InteractionDef InteractionWaitMoney = new("WaitMoney", typeof(Interaction_Vendor_WaitPayment), InteractionControllers.ExternalFull);
    public static readonly InteractionDef InteractionCustomerWaitItemAvailable = new("WaitItem", typeof(Interaction_RepairCustomer_WaitItemAvailable), InteractionControllers.ExternalFull);

    public static readonly PlanDef PlanQueue = new("Queue", typeof(BehaviorExecutePlanNew), InteractionQueue);
    public static readonly PlanDef PlanCustomerWaitItemReady = new("QueueWait", typeof(BehaviorExecutePlanNew), InteractionWaitPrice);
    public static readonly PlanDef PlanQueueServe = new("QueueServe", typeof(BehaviorExecutePlanNew), InteractionServing);
    public static readonly PlanDef PlanWaitMoney = new("WaitMoney", typeof(BehaviorExecutePlanNew), InteractionWaitMoney);
    public static readonly PlanDef PlanCustomerWaitItemAvailable = new("WaitItem", typeof(BehaviorExecutePlanNew), InteractionCustomerWaitItemAvailable);

    public static readonly PlannerDef PlannerVendor = new("Vendor", typeof(Planner_Repairs_Vendor));
    public static readonly PlannerDef PlannerCustomer = new("Customer", typeof(Planner_Repairs_Customer));
    static ServiceRepairsDefOf()
    {
        Def.Register(typeof(ServiceRepairsDefOf));
    }
}

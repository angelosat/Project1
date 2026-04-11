using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Planners;
using Project1.Core.Interactions;
using Project1.Framework;

namespace Project1.Core.Towns.Services.Repairing;

[EnsureStaticCtorCall]
public static class ServiceRepairsDefOf
{
    public static readonly InteractionDef InteractionQueue = new("Queuing", typeof(Interaction_RepairQueue), InteractionControllers.ExternalFull);
    public static readonly InteractionDef InteractionServed = new("Served", typeof(Interaction_RepairWait), InteractionControllers.ExternalFull);
    public static readonly InteractionDef InteractionServing = new("Serving", typeof(Interaction_RepairServing), InteractionControllers.ExternalFull);
    public static readonly PlanDef PlanQueue = new("Queue", typeof(BehaviorExecutePlanNew), InteractionQueue);
    public static readonly PlanDef PlanQueueWait = new("QueueWait", typeof(BehaviorExecutePlanNew), InteractionServed);
    public static readonly PlanDef PlanQueueServe = new("QueueServe", typeof(BehaviorExecutePlanNew), InteractionServing);
    public static readonly PlannerDef PlannerVendor = new("Vendor", typeof(Planner_Repairs_Vendor));
    public static readonly PlannerDef PlannerCustomer = new("Customer", typeof(Planner_Repairs_Customer));
    static ServiceRepairsDefOf()
    {
        Def.Register(typeof(ServiceRepairsDefOf));
    }
}

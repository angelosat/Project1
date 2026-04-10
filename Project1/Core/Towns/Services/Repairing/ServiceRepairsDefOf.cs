using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Interactions;
using Project1.Framework;

namespace Project1.Core.Towns.Services.Repairing;

[EnsureStaticCtorCall]
public static class ServiceRepairsDefOf
{
    public static readonly InteractionDef Queueing = new("Queuing", typeof(Interaction_RepairQueue), InteractionControllers.ExternalFull);
    public static readonly PlanDef PlanQueue = new("Queue", typeof(BehaviorExecutePlanNew), Queueing);
    static ServiceRepairsDefOf()
    {
        Def.Register(typeof(ServiceRepairsDefOf));
    }
}

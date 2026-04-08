using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Interactions;
using Project1.Framework;

namespace Project1.Core.Towns.Healing;

[EnsureStaticCtorCall]
public static class HealingDefOf
{
    //public static readonly InteractionDef InteractionHealingWaitTarget = new("HealingWaitTarget", typeof(InteractionHealingWaitTarget), InteractionControllers.ExternalFull);
    public static readonly InteractionDef InteractionHealingWaitCaster = new("HealingWaitCaster", typeof(InteractionHealingWaitCaster), InteractionControllers.ExternalFull);
    public static readonly InteractionDef InteractionHealingWaitPay = new("HealingWaitPay", typeof(InteractionHealingWaitPay), InteractionControllers.ExternalFull);
    //public static readonly InteractionDef InteractionHealingPerform = new("HealingPerform", typeof(InteractionHealingPerform), InteractionControllers.ExternalFull);
    public static readonly InteractionDef InteractionHealingSeek = new("HealingSeek", typeof(InteractionHealingSeek), InteractionControllers.ExternalFull);
    public static readonly PlanDef PlanHealingSeek = new("HealingSeek", typeof(BehaviorExecutePlanNew), InteractionHealingSeek);
    public static readonly PlanDef PlanHealingWaitCaster = new("HealingWaitCaster", typeof(BehaviorExecutePlanNew), InteractionHealingWaitCaster);
    public static readonly PlanDef PlanHealingWaitPay = new("HealingWaitPay", typeof(BehaviorExecutePlanNew), InteractionHealingWaitPay);

    static HealingDefOf()
    {
        Def.Register(typeof(HealingDefOf));   
    }
}

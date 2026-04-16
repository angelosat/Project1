using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.Interactions;
using Project1.Framework;

namespace Project1.Core.Towns.Services.Spells;

[EnsureStaticCtorCall]
public static class TownServiceSpellsDefOf
{
    public static readonly InteractionDef InteractionHealingWaitCaster = new("HealingWaitCaster", typeof(Interaction_Spell_WaitCustomer), InteractionControllers.ExternalFull);
    public static readonly InteractionDef InteractionHealingWaitPay = new("HealingWaitPay", typeof(Interaction_Spell_WaitPay), InteractionControllers.ExternalFull);
    public static readonly InteractionDef InteractionHealingSeek = new("HealingSeek", typeof(Interaction_Spell_Customer), InteractionControllers.ExternalFull);
    public static readonly PlanDef PlanHealingSeek = new("HealingSeek", typeof(BehaviorExecutePlanNew), InteractionHealingSeek);
    public static readonly PlanDef PlanHealingWaitCaster = new("HealingWaitCaster", typeof(BehaviorExecutePlanNew), InteractionHealingWaitCaster);
    public static readonly PlanDef PlanHealingWaitPay = new("HealingWaitPay", typeof(BehaviorExecutePlanNew), InteractionHealingWaitPay);

    static TownServiceSpellsDefOf()
    {
        Def.Register(typeof(TownServiceSpellsDefOf));   
    }
}

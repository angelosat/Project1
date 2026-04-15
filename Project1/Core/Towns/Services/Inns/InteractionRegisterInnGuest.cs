using Project1.Core.Interactions;

namespace Project1.Core.Towns.Services.Inns;

sealed class InteractionRegisterInnGuest : InteractionLogic
{
    //sealed class Context : InteractionContext
    //{
    //    internal TownComp_Inns Manager => field ??= this.Actor.Map.Town.Inns;
    //    //internal Queue<Actor> Queue => field ??= this.Manager.GetQueue(this.Target.Global);
    //    //internal Actor NextGuest => field ??= this.Queue.Peek();
    //}
    //protected override InteractionContext CreateContextInt()
    //    => new Context();
    internal override void OnStart(Interaction i)
        => i.Progress.SetMax(Ticks.FromMinutes(10));
    
    internal override void OnSuccess(Interaction i)
    {
        //var typedCtx = (Context)i.Context;
        //var manager = typedCtx.Manager;
        i.Actor.Map.Town.Inns.RegisterGuest(i.Actor.CurrentPlan.ServiceRequest as ServiceRequest_Inn);
    }
}

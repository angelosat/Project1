namespace Project1.Core.Interactions
{
    class InteractionHaulLogic : InteractionLogic
    {
        //public override bool CanPerform(InteractionContext ctx)
        //{
        //    if (ctx.Target.Object.Map == ctx.Actor.Map)
        //        return true;
        //    if (ctx.Actor.Inventory.Contains(ctx.Target.Object))
        //        return true;
        //    return false;
        //}
        //public override bool CanFinish(InteractionContext ctx) => this.CanPerform(ctx);
        internal override void OnFinish(Interaction i)
        {
            var actor = i.Context.Actor;
            if (actor.Net.IsClient) 
                return;
            var target = i.Context.Target;
            var count = i.Context.Count;
            actor.Inventory.HaulNew(target.Object, count);
        }
    }
}

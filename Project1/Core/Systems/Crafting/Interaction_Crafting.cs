using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Interactions;
using Project1.Core.Skills;
using Project1.Core.Systems.Recipes;
using Project1.Framework;
using System.Linq;

namespace Project1.Core.Systems.Crafting;

sealed class Interaction_Crafting : InteractionLogic
{
    public sealed class Context : InteractionContext
    {
        public BlockWorkstationComp Comp => field ??= this.Target.Map.GetBlockEntity(this.Target.Global).Comps.GetComp<BlockWorkstationComp>();
        public override float ProgressBarPercentage => this.Actor.Work.Task.ProgressPercentage;
        internal CraftingOrder Order => field ??= this.Actor.Map.Town.Crafting.Get(this.Actor.CurrentPlan.Order);
        public override SkillDef Skill => field ??= this.Order.WorkstationCapability.Worker.CraftingSkill;
    }
    protected override InteractionContext CreateContextInt() => new Context();
    public override bool CanFinish(InteractionContext ctx) => CanFinish((Context)ctx);
    static bool CanFinish(Context ctx) => CanPerform(ctx);
    public override bool CanPerform(InteractionContext ctx) => CanPerform((Context)ctx);
    static bool CanPerform(Context ctx) => ctx.Comp.IngredientsInPlace(ctx.Actor.CurrentPlan.TargetsA);
    Context ContextTyped(InteractionContext ctx) => (Context)ctx;
    //internal override void OnProgressAdded(Interaction i, int delta)
    //{
    //    this.ContextTyped(i.Context).Order.WorkstationCapability.Worker.OnWorkApplied(i.Actor, delta);
    //}
    
    internal override void OnFinish(Interaction i)
    {
        var actor = i.Actor;
        if (actor.Net.IsClient)
            return;
        var map = actor.Map;
        var plan = actor.CurrentPlan;
        var orderid = plan.Order;
        var order = map.Town.Crafting.Get(orderid);
        var workstation = i.Target;

        // consume fuel
        if (!order.TryConsumeFuel())
            return;
        var ingredients = plan.TargetsA.Select(t => t.Entity);

        //var product = order.WorkstationCapability.Worker.CreateProduct(actor, order, ingredients);
        var product = order.CreateProduct(actor, ingredients);
        //var product = map.Town.Crafting.CreateProductFromOrder(actor, order, ingredients);

        map.Spawn(product, workstation.Global.Above(), Vector3.Zero);
        //order.CompletedBy(actor);
        actor.Map.Town.Crafting.MarkCompleted(order, actor, product);
    }
}

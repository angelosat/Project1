using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Interactions;
using Project1.Framework;
using System.Linq;

namespace Project1.Core.Systems.Crafting;

sealed class Interaction_Crafting : InteractionLogic
{
    public sealed class Context : InteractionContext
    {
        BlockWorkstationComp _comp;
        public BlockWorkstationComp Comp => this._comp ??= this.Target.Map.GetBlockEntity(this.Target.Global).Comps.GetComp<BlockWorkstationComp>();
        public override float ProgressBarPercentage => this.Actor.Work.Task.ProgressPercentage;
    }
    protected override InteractionContext CreateContextInt() => new Context();
    public override bool CanFinish(InteractionContext ctx) => CanFinish((Context)ctx);
    static bool CanFinish(Context ctx) => CanPerform(ctx);
    public override bool CanPerform(InteractionContext ctx) => CanPerform((Context)ctx);
    static bool CanPerform(Context ctx) => ctx.Comp.IngredientsInPlace(ctx.Actor.CurrentPlan.TargetsA);
    
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

        var creationReq = order.GetCreationRequest();
        var ingredients = plan.TargetsA.Select(t => t.Entity);
        var mapping = order.WorkstationCapability.Worker.GetIngredientMapping(order.ProductDef, ingredients);

        foreach (var (bone, item) in mapping)
        {
            creationReq.OverrideMaterial(bone, item.Body.Material);
            map.World.DisposeEntity(item);
        }
        var product = creationReq.Create();
        order.WorkstationCapability.Worker.PostProcess(product, actor, order.Source);
        map.Spawn(product, workstation.Global.Above(), Vector3.Zero);
        //order.CompletedBy(actor);
        actor.Map.Town.Crafting.MarkCompleted(order, actor, product);
    }
}

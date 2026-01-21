using Microsoft.Xna.Framework;
using System.Linq;

namespace Start_a_Town_
{
    class InteractionCraftingLogic : InteractionLogic
    {
        public sealed class Context : InteractionContext
        {
            BlockWorkstationComp _comp;
            public BlockWorkstationComp Comp => this._comp ??= this.Target.Map.GetBlockEntity(this.Target.Global).Comps.GetComp<BlockWorkstationComp>();
            public override float ProgressPercentage => this.Actor.Work.Task.ProgressPercentage;
        }
        protected override InteractionContext CreateContextInternal() => new Context();
        public override bool CanFinish(InteractionContext ctx) => this.CanFinish((Context)ctx);
        bool CanFinish(Context ctx) => this.CanPerform(ctx);
        public override bool CanPerform(InteractionContext ctx) => this.CanPerform((Context)ctx);
        bool CanPerform(Context ctx)
        {
            return ctx.Comp.IngredientsInPlace(ctx.Actor.CurrentTask.TargetsA);
        }
        internal override void OnFinish(Interaction i)
        {
            var actor = i.Actor;
            var target = i.Target;
            if (actor.Net.IsClient)
                return;
            var map = actor.Map;
            var plan = actor.CurrentTask;
            var order = plan.Order;
            var workstation = target;

            // consume fuel
            if (!order.TryConsumeFuel())
                return;

            var inSlots = plan.TargetsA.Select(t => t.Entity as Entity);
            var creationReq = order.GetCreationRequest(); /*new EntityCreationRequest(order.Refinement, null, stackSize: 1);*/
            var targetBones = order.GetSlotMapping();
            var mapping = targetBones.Zip(inSlots);
            foreach (var pair in mapping)
            {
                creationReq.OverrideMaterial(pair.First, pair.Second.Body.Material);
                map.World.DisposeEntity(pair.Second);
            }
            var product = EntityFactory.Create(creationReq);
            map.World.Register(product);
            map.Spawn(product, workstation.Global.Above(), Vector3.Zero);
            order.CompletedBy(actor);
        }
    }
}

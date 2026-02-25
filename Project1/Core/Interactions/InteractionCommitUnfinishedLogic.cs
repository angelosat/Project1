using Microsoft.Xna.Framework;
using Project1.Core.Animations;
using Project1.Core.Blocks;
using Project1.Core.Crafting;
using Project1.Core.Tools;
using Project1.Framework;
using System.Linq;

namespace Project1.Core.Interactions
{
    class InteractionCommitUnfinishedLogic : InteractionLogic
    {
        class Context : InteractionContext
        {
            internal BlockWorkstationComp Workstation => field ??= this.Target.Map.GetBlockEntity(this.Target.Global).Comps.GetComp<BlockWorkstationComp>();
        }
        static bool CanPerform(Context ctx) => ctx.Workstation.IngredientsInPlace(ctx.Actor.CurrentPlan.TargetsA);
        public override bool CanPerform(InteractionContext ctx) => CanPerform((Context)ctx);
        protected override InteractionContext CreateContextInternal() => new Context();
        internal override void OnFinish(Interaction i)
        {
            var actor = i.Actor;
            if (actor.Net.IsClient)
                return;
            var plan = actor.CurrentPlan;
            var order = plan.Order;
            var ctx = (Context)i.Context;

            var workstationCell = i.Target;
            var map = actor.Map;
            var ingredients = CraftingSystem.GetIngredientMapping(order.ProductDef, plan.TargetsA.Select(t => t.Entity));
            var item = ToolSystem.CreateUnfinishedItem(
                actor,
                order,
                ingredients[BoneDefOf.ToolHandle].Body.Material,
                ingredients[BoneDefOf.ToolHead].Body.Material);

            foreach (var ingredient in ingredients.Values)
                map.World.DisposeEntity(ingredient);

            item.Author = actor;
            map.Spawn(item, workstationCell.Global.Above(), Vector3.Zero);
        }
        
       
    }
}

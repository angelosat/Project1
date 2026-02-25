using Microsoft.Xna.Framework;
using Project1.Core.Animations;
using Project1.Core.Blocks;
using Project1.Core.Crafting;
using Project1.Core.Entities;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Resources;
using Project1.Core.Tools;
using Project1.Framework;
using System;
using System.Linq;

namespace Project1.Core.Interactions
{
    class InteractionAdvanceUnfinishedLogic : InteractionLogic
    {
        class Context : InteractionContext
        {
            internal Entity UnfinishedItem => field ??= this.Workstation.GetUnfinishedItem();
            internal UnfinishedItemComp UnfinishedComp => field ??= this.UnfinishedItem?.GetComponent<UnfinishedItemComp>();
            internal Resource Assembly => field ??= this.UnfinishedItem.Resources[ResourceDefOf.Assembly];
            internal BlockWorkstationComp Workstation => field ??= this.Target.Map.GetBlockEntity(this.Target.Global).Comps.GetComp<BlockWorkstationComp>();
            public override float ProgressPercentage => this.Assembly?.Percentage ?? 0;
        }
        protected override InteractionContext CreateContextInternal() => new Context();

        public override void ApplyWork(InteractionContext ctx, int workAmount)
        {
            var actor = ctx.Actor;
            if (actor.Net.IsClient)
                return;
            var ctxTyped = (Context)ctx;
            var unfinishedItem = ctxTyped.UnfinishedItem;
            if (unfinishedItem is null)
                throw new InvalidOperationException();
            ctxTyped.UnfinishedComp.ApplyWork(workAmount);
            //unfinishedItem.Resources[ResourceDefOf.Assembly].ApplyDelta(1);
            ctxTyped.Assembly.ApplyDelta(workAmount);
        }
        internal override void OnFinish(Interaction i)
        {
            var actor = i.Actor;
            if (actor.Net.IsClient)
                return;
            //var plan = actor.CurrentPlan;
            var ctxTyped = (Context)i.Context;
            var unfinishedItem = ctxTyped.UnfinishedItem;
            //var order = unfinishedItem.GetComponent<UnfinishedItemComp>().Order;
            var orderId = unfinishedItem.GetComponent<UnfinishedItemComp>().OrderId;
            var map = actor.Map;
            var creationReq = ctxTyped.UnfinishedComp.GetCreationRequest();
            var ctx = i.Context as Context;
            foreach (var pair in ctx.UnfinishedComp.MaterialBindings)
                creationReq.OverrideMaterial(pair.Key, pair.Value);
            var product = creationReq.Create();
            product.Author = actor;

            map.Spawn(product, unfinishedItem.Global, unfinishedItem.Velocity);
            map.World.DisposeEntity(unfinishedItem);
            var order = map.Town.CraftingManagerNew.GetOrder(orderId);
            order.CompletedBy(actor);
        }
    }
    class InteractionSpawnUnfinishedLogic : InteractionLogic
    {
        class Context : InteractionContext
        {
            //internal Entity UnfinishedItem => field ??= this.Workstation.GetUnfinishedItem();
            //internal UnfinishedItemComp UnfinishedComp => field ??= this.UnfinishedItem?.GetComponent<UnfinishedItemComp>();
            internal BlockWorkstationComp Workstation => field ??= this.Target.Map.GetBlockEntity(this.Target.Global).Comps.GetComp<BlockWorkstationComp>();
            //public override float ProgressPercentage => this.UnfinishedComp?.ProgressPercentage ?? 0;
        }
        static bool CanPerform(Context ctx) => ctx.Workstation.IngredientsInPlace(ctx.Actor.CurrentPlan.TargetsA);
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
    class InteractionCraftingUnfinishedLogic : InteractionLogic
    {
        class Context : InteractionContext
        {
            internal Entity UnfinishedItem => field ??= this.Workstation.GetUnfinishedItem();
            internal UnfinishedItemComp UnfinishedComp => field ??= this.UnfinishedItem?.GetComponent<UnfinishedItemComp>();
            internal BlockWorkstationComp Workstation => field ??= this.Target.Map.GetBlockEntity(this.Target.Global).Comps.GetComp<BlockWorkstationComp>();
            public override float ProgressPercentage => this.UnfinishedComp?.ProgressPercentage ?? 0;
        }
        static bool CanPerform(Context ctx) => ctx.UnfinishedItem is not null || ctx.Workstation.IngredientsInPlace(ctx.Actor.CurrentPlan.TargetsA);
        public override bool CanPerform(InteractionContext ctx) => CanPerform((Context)ctx);
        protected override InteractionContext CreateContextInternal() => new Context();
        internal override void OnStart(Interaction i)
        {
            var actor = i.Actor;
            if (actor.Net.IsClient)
                return;
            var plan = actor.CurrentPlan;
            var order = plan.Order;
            var ctx = (Context)i.Context;
            var unfinishedItem = ctx.UnfinishedItem;
            if (unfinishedItem is not null)
                return;

            var workstation = i.Target;
            var map = actor.Map;

            var ingredients = CraftingSystem.GetIngredientMapping(order.ProductDef, plan.TargetsA.Select(t => t.Entity));
            var item = ToolSystem.CreateUnfinishedItem(
                actor, 
                order, 
                ingredients[BoneDefOf.ToolHandle].Body.Material, 
                ingredients[BoneDefOf.ToolHead].Body.Material);

            foreach (var ingredient in ingredients.Values)
                map.World.DisposeEntity(ingredient);

            map.Spawn(item, workstation.Global.Above(), Vector3.Zero);
        }
        public override void ApplyWork(InteractionContext ctx, int workAmount)
        {
            var actor = ctx.Actor;
            if (actor.Net.IsClient)
                return;
            var ctxTyped = (Context)ctx;
            var unfinishedItem = ctxTyped.UnfinishedItem;
            if (unfinishedItem is null)
                throw new InvalidOperationException();
            ctxTyped.UnfinishedComp.ApplyWork(workAmount);
        }
        internal override void OnFinish(Interaction i)
        {
            var actor = i.Actor;
            if (actor.Net.IsClient)
                return;
            var plan = actor.CurrentPlan;
            var order = plan.Order;
            var ctxTyped = (Context)i.Context;
            var unfinishedItem = ctxTyped.UnfinishedItem;
            var map = actor.Map;
            var creationReq = ctxTyped.UnfinishedComp.GetCreationRequest();
            var ctx = i.Context as Context;
            foreach (var pair in ctx.UnfinishedComp.MaterialBindings)
                creationReq.OverrideMaterial(pair.Key, pair.Value);
            var product = creationReq.Create();
            map.Spawn(product, unfinishedItem.Global, unfinishedItem.Velocity);
            map.World.DisposeEntity(unfinishedItem);
            order.CompletedBy(actor);
        }
    }
    class InteractionCraftingLogic : InteractionLogic
    {
        public sealed class Context : InteractionContext
        {
            BlockWorkstationComp _comp;
            public BlockWorkstationComp Comp => this._comp ??= this.Target.Map.GetBlockEntity(this.Target.Global).Comps.GetComp<BlockWorkstationComp>();
            public override float ProgressPercentage => this.Actor.Work.Task.ProgressPercentage;
        }
        protected override InteractionContext CreateContextInternal() => new Context();
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
            var order = plan.Order;
            var workstation = i.Target;

            // consume fuel
            if (!order.TryConsumeFuel())
                return;

            var creationReq = order.GetCreationRequest();
            var mapping = CraftingSystem.GetIngredientMapping(order.ProductDef, plan.TargetsA.Select(t => t.Entity as Entity));
            foreach (var (bone, item) in mapping)
            {
                creationReq.OverrideMaterial(bone, item.Body.Material);
                map.World.DisposeEntity(item);
            }
            var product = creationReq.Create();
            map.Spawn(product, workstation.Global.Above(), Vector3.Zero);
            order.CompletedBy(actor);
        }
    }
}

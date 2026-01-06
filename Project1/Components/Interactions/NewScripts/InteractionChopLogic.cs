using Start_a_Town_.Components;
using System;

namespace Start_a_Town_
{
    class InteractionChopLogic : InteractionLogic
    {
        public class Context : InteractionContext
        {
            Resource _hp;
            public Resource HitPoints => this._hp ??= this.Target.Object.GetResource(ResourceDefOf.HitPoints);
            PlantComponent _plantComp;
            public PlantComponent PlantComp => this._plantComp ??= this.Target.Object.GetComponent<PlantComponent>();
            public override float ProgressPercentage => 1 - this.HitPoints.Percentage;
        }
        protected override InteractionContext CreateContextInternal() => new Context();
        public override bool CanPerform(InteractionContext ctx) => CanPerform((Context)ctx);
        public override bool CanFinish(InteractionContext ctx) => CanFinish((Context)ctx);
        public override bool WillFinish(InteractionContext ctx, int workAmount) => WillFinish((Context)ctx, workAmount);
        public override void ApplyWork(InteractionContext ctx, int workAmount) => ApplyWork((Context)ctx, workAmount);
        static bool CanPerform(Context ctx)
        {
            var plantTarget = ctx.Target;
            if (plantTarget.Object.Map != ctx.Actor.Map)
                return false;
            if (!ctx.Actor.Map.Town.DesignationManager.IsDesignation(plantTarget, DesignationDefOf.Chop))
                return false;
            return true;
        }
        static bool CanFinish(Context ctx) => CanPerform(ctx);
        
        static bool WillFinish(Context ctx, int workAmount) => ctx.HitPoints.Value - workAmount <= 0;
        
        static void ApplyWork(Context ctx, int workAmount)
        {
            //ctx.HitPoints.Value -= workAmount;
            ctx.PlantComp.Wiggle((float)Math.PI / 32f, 20, ctx.PlantComp.Species.StemMaterial.Density);
            ctx.Target.Map.Events.Post(new PlantChoppedEvent(ctx.Actor, ctx.Target, workAmount));

            if (ctx.Actor.Net.IsClient)
                return;
            ctx.HitPoints.Adjust(-workAmount);
        }
    }
}

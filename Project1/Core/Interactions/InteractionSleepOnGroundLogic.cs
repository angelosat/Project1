using Microsoft.Xna.Framework;
using Project1.Core.Animations;
using Project1.Core.Effects;
using Project1.Core.Needs;
using System;

namespace Project1.Core.Interactions
{
    sealed class InteractionSleepOnGroundLogic : InteractionLogic 
    {
        sealed class Context : InteractionContext
        {
            Need _energy;
            Need Energy => _energy ??= this.Actor.GetNeed(NeedDefOf.Energy);
            public override float ProgressPercentage => this.Energy.Percentage;
        }
        protected override InteractionContext CreateContextInternal() => new Context();
        internal override void OnStart(Interaction i)
        {
            var a = i.Actor;
            var t = i.Target;
            a.Effects.Apply(EffectDefOf.Sleeping);
            var body = a.Body;
            body.RestingFrame = new Keyframe(0, Vector2.Zero, (float)(Math.PI / 2f));
            body.OriginGroundOffset = new Vector2(0, -4);/// Vector2.Zero;
        }
        internal override void OnFinish(Interaction i)
        {
            var a = i.Actor;
            var t = i.Target;
            a.Effects.Remove(EffectDefOf.Sleeping);
            var body = a.Body;
            body.RestingFrame = new Keyframe(0, Vector2.Zero, 0);
            body.OriginGroundOffset = a.Def.Body.OriginGroundOffset;
        }
    }
}

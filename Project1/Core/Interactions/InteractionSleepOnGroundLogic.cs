using Microsoft.Xna.Framework;
using Project1.Core.Animations;
using Project1.Core.Needs;
using Project1.Core.Simulation;
using Project1.Core.Systems.Effects;
using System;

namespace Project1.Core.Interactions;

sealed class InteractionSleepOnGroundLogic : InteractionLogic 
{
    sealed class Context : InteractionContext
    {
        NeedRuntime _energy;
        NeedRuntime Energy => _energy ??= this.Actor.GetNeed(NeedDefOf.Energy);
        public override float ProgressBarPercentage => this.Energy.Percentage;
    }
    protected override InteractionContext CreateContextInt() => new Context();
    internal override void OnStart(Interaction i)
    {
        var a = i.Actor;
        var t = i.Target;
        var rate = Ticks.FromMinutes(1);
      
        a.World.Events.Post(new ActorEffectAppliedNewNewEvent((StaticWorld)a.World,
                                                              a.RefId,
                                                              new EffectConfig_ModifyNeed { Need = NeedDefOf.Energy, Delta = 1f / rate },
                                                              EffectSchedule.Timed(rate, SimulationDuration.Infinite)));
        var body = a.Body;
        body.RestingFrame = new Keyframe(0, Vector2.Zero, (float)(Math.PI / 2f));
        body.OriginGroundOffset = new Vector2(0, -4);/// Vector2.Zero;
    }
    internal override void OnFinish(Interaction i)
    {
        var a = i.Actor;
        var t = i.Target;
        a.Effects.Abort(EffectDefOf.ModifyNeed, NeedDefOf.Energy);
        var body = a.Body;
        body.RestingFrame = new Keyframe(0, Vector2.Zero, 0);
        body.OriginGroundOffset = a.Def.Body.OriginGroundOffset;
    }
}

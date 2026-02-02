using Project1.Core.Gear;
using Project1.Framework.Resources;
using Project1.Framework.Stats;
using Start_a_Town_;
using System;

namespace Project1.Framework.Interactions
{
    internal class InteractionResolverDef(string name, Type workerType) : Def(name)
    {
        internal InteractionResolver Worker = ActivatorSafe<InteractionResolver>.CreateInstance(workerType);
    }
    internal abstract class InteractionResolver
    {
        internal abstract float Resolve(Actor actor);
    }
    internal class WorkSpeedResolver : InteractionResolver
    {
        internal override float Resolve(Actor actor)
        {
            var tool = actor.GetEquipmentSlot(GearTypeDefOf.Mainhand);
            var toolspeed = tool?.Stats[StatDefOf.ToolSpeed] ?? 0;

            var speed = 1 + toolspeed;

            var stamina = actor[ResourceDefOf.Stamina];
            speed *= stamina.CurrentThreshold.Value;

            return speed;
        }
    }
    [EnsureStaticCtorCall]
    static class InteractionResolverDefOf
    {
        public static readonly InteractionResolverDef WorkSpeed = new("WorkSpeed", typeof(WorkSpeedResolver));
        static InteractionResolverDefOf()
        {
            Def.Register(typeof(InteractionResolverDefOf));
        }
    }
}

using System;
using Project1.Core.Gear;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Resources;
using Project1.Core.Stats;

namespace Project1.Core.Interactions
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
}

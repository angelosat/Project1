using Project1.Core.Entities.Actors;
using Project1.Framework;

#nullable enable

namespace Project1.Core.Systems.Effects;

[EnsureStaticCtorCall]
static class EffectsSystem
{
    static EffectsSystem()
    {
        Registry.WorldEventHooksClient.Register<ActorEffectAppliedNewEvent>(OnActorEffectApplied);
        Registry.WorldEventHooksServer.Register<ActorEffectAppliedNewEvent>(OnActorEffectApplied);

        Registry.WorldEventHooksClient.Register<ActorEffectRemovedEvent>(OnActorEffectRemoved);
        Registry.WorldEventHooksServer.Register<ActorEffectRemovedEvent>(OnActorEffectRemoved);

        Registry.WorldEventHooksClient.Register<ActorEffectRemovedIndexedEvent>(OnActorEffectIndexedRemoved);
        Registry.WorldEventHooksServer.Register<ActorEffectRemovedIndexedEvent>(OnActorEffectIndexedRemoved);
    }

    private static void OnActorEffectApplied(ActorEffectAppliedNewEvent e)
    {
        var actor = e.World.Resolve<Actor>(e.ActorId);
        actor.Effects.Apply(CreateRuntime(e.EffectDef, e.OptionalTarget, e.Magnitude));
    }

    private static void OnActorEffectIndexedRemoved(ActorEffectRemovedIndexedEvent e)
    {
        var world = e.World;
        var actor = world.Resolve<Actor>(e.ActorId);
        actor.Effects.RemoveAt(e.EffectId);
    }

    private static void OnActorEffectRemoved(ActorEffectRemovedEvent e)
    {
        var world = e.World;
        var actor = world.Resolve<Actor>(e.ActorId);
        var effectDef = e.EffectDef;
        actor.Effects.RemoveInternal(effectDef);
    }

    //static public EntityEffectRuntime CreateRuntime(EntityEffectKey key, int magnitude)
    //    => key.Def.Worker.CreateRuntime(key.Target, magnitude);

    static EntityEffectRuntime CreateRuntime(EffectDef effect, Def? target, int magnitude)
       => effect.Worker.CreateRuntime(effect, target, magnitude);
}

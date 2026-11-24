using Project1.Core.Entities.Actors;

namespace Project1.Core.Systems.Effects;

static class EffectsSystem
{
    static EffectsSystem()
    {
        Registry.WorldEventHooksClient.Register<ActorEffectRemovedEvent>(OnActorEffectRemoved);
        Registry.WorldEventHooksServer.Register<ActorEffectRemovedEvent>(OnActorEffectRemoved);

        Registry.WorldEventHooksClient.Register<ActorEffectRemovedIndexedEvent>(OnActorEffectIndexedRemoved);
        Registry.WorldEventHooksServer.Register<ActorEffectRemovedIndexedEvent>(OnActorEffectIndexedRemoved);
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
}

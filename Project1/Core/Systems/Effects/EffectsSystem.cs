using Project1.Core.Entities.Actors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project1.Core.Systems.Effects;

static class EffectsSystem
{
    static EffectsSystem()
    {
        Registry.WorldEventHooksClient.Register<ActorEffectRemovedEvent>(OnActorEffectRemoved);
        Registry.WorldEventHooksServer.Register<ActorEffectRemovedEvent>(OnActorEffectRemoved);
    }

    private static void OnActorEffectRemoved(ActorEffectRemovedEvent e)
    {
        var world = e.World;
        var actor = world.Resolve<Actor>(e.ActorId);
        var effectDef = e.EffectDef;
        actor.Effects.RemoveInternal(effectDef);
    }
}

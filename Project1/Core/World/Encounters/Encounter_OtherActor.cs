using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Simulation;
using Project1.Core.Systems.Biology;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Effects;
using Project1.Core.Systems.Thoughts;
using Project1.Core.World.Combat;
using Project1.Core.World.Encounters;
using Project1.Core.World.Frontiers;
using Project1.Framework;
using Project1.Framework.Events;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace Project1.Core.World.Actions;

internal sealed class Encounter_OtherActor : EncounterWorker
{
    sealed record Context(EntityRefId Other) : EncounterContext;

    private static bool TryUsePotion(Actor actor, Actor other)
    {
        if (actor.Inventory.First(i => i.Profile == ConsumableDefOf.Potion && i.Consumable.HasEffect(EffectDefOf.ResourceDelta, ResourceDefOf.Health)) is not Entity potion)
            return false;
        var health = other.Resources.View(ResourceDefOf.Health);
        var prevHealth = health.Value;
        ConsumableSystem.Activate(potion, actor, other);
        var heal = health.Value - prevHealth;
        if (!other.Biology.IsIncapacitated)
        {
            actor.World.Events.Post(new ActorHelpedIncapacitatedEvent(actor, other, potion));
            $"{actor} used {potion.LabelReadable} and healed {other} for {heal}".ToConsole();
            if (actor.World.Resolve((EntityRefId)other.RefId) is null)
                throw new System.Exception();
        }
        return true;
    }

    private static void HealArbitrary(Actor actor, Actor other)
    {
        var health = other.Resources.View(ResourceDefOf.Health);
        var heal = -health.Value + 10;
        health.ApplyDelta(heal);
        actor.World.Events.Post(new ActorHelpedIncapacitatedEvent(actor, other, null));
        $"{actor} healed {other} for {heal}".ToConsole();
        if (actor.World.Resolve((EntityRefId)other.RefId) is null)
            throw new System.Exception();
    }

    internal override IEnumerable<(EncounterDef encounter, EncounterContext? context, int weight)> GetCandidates(FrontierRuntime frontier, Actor actor)
    {
        var world = (StaticWorld)actor.World;
        if (!world.Combat.IsAvailableForNewCombat(actor))
            yield break;
        foreach (var k in frontier.ActorsActiveNew
            .Where(a => a != actor.RefId && world.Combat.IsAvailableForNewCombat(a) && !actor.Biology.IsIncapacitated)
            .Select(id => (EncounterDefOf.ActorEncounter, new Context(id), 1000)))
            yield return k;
    }

    internal override WorldActionData? DecideAction(Actor actor, EncounterContext context)
    {
        var ctx = (Context)context;
        var world = actor.World;
        var other = world.Resolve<Actor>(ctx.Other);
        if (other.Biology.IsIncapacitated)
            throw new InvalidOperationException($"{actor} tried to initiate combat against incapacitated actor {other}.");
        if (!ActorCombatAISystem.IsConfident(actor))
            return null;
        return new WorldActionData_CombatEngage(actor) { TargetId = ctx.Other };
    }

    record struct ActorsMetOffMapEvent(Actor Actor, Actor Other) : IEventPayload;

    sealed class Thought_MetOtherActor : ThoughtSource<ActorsMetOffMapEvent>
    {
        internal override void Handle(ActorsMetOffMapEvent e)
        {
            e.Actor.AI.State.Log.Write($"Encountered {e.Other.Name}");
            e.Other.AI.State.Log.Write($"Encountered {e.Actor}");
        }
    }

    record struct ActorHelpedIncapacitatedEvent(Actor Actor, Actor Other, Entity? ItemUsed) : IEventPayload;

    sealed class Thought_HelpedOtherActor : ThoughtSource<ActorHelpedIncapacitatedEvent>
    {
        internal override void Handle(ActorHelpedIncapacitatedEvent e)
        {
            e.Actor.AI.State.Log.Write($"Helped {e.Other.Name}{(e.ItemUsed is Entity item ? $" by using {item.LabelReadable}" : "")}");
            e.Other.AI.State.Log.Write($"Was helped by {e.Actor}");
        }
    }
}

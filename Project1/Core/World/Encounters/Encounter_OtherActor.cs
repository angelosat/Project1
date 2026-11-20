using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Simulation;
using Project1.Core.Systems.Biology;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Effects;
using Project1.Core.Systems.Thoughts;
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
    //internal override int GetWeight(FrontierRuntime frontier, Actor actor)
    //    //=> frontier.IncapacitatedActors.Count * 10;
    //    => ((StaticWorld)actor.World).Combat.IsAvailableForNewCombat(actor) ? (frontier.OccupantCount - 1) : 0;// * 100;

    //protected override void OnSuccess(FrontierRuntime frontier, EncounterRuntime encounter)
    //{
    //    var actor = encounter.Context.Actor;
    //    var otherActorIds = frontier.ActorsActiveNew.Except([actor.RefId]).ToList();
    //    //if (otherActors.Count == 0)
    //    //    return;
    //    var world = (StaticWorld)actor.World;

    //    var other = actor.World.Get<Actor>(otherActorIds.SelectRandom(world.Random));
    //    if (!world.Combat.AreAvailableForNewCombat(actor, other))
    //        throw new InvalidOperationException(
    //            $"{actor} is unavailable for combat"
    //            );
    //    ArgumentNullException.ThrowIfNull(other);
    //    if (FactionsSystem.AreEnemies(actor, other))
    //    {
    //        if (!world.Combat.IsEngaged(other))
    //            world.Combat.Engage(actor, other);
    //        return;
    //    }

    //    if (other.Biology.IsIncapacitated)
    //    {
    //        TryUsePotion(actor, other);
    //        //HealArbitrary(actor, other);
    //    }
    //    else
    //    {
    //        actor.Relationships.ApplyDelta(other, 1);
    //        other.Relationships.ApplyDelta(actor, 1);
    //        actor.World.Events.Post(new ActorsMetOffMapEvent(actor, other));
    //    }
    //}

    private static bool TryUsePotion(Actor actor, Actor other)
    {
        if (actor.Inventory.First(i => i.Profile == ConsumableDefOf.Potion && i.Consumable.HasEffect(EffectDefOf.RestoreResource, ResourceDefOf.Health)) is not Entity potion)
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

    internal override IEnumerable<(EncounterDef encounter, WorldActionData? context, int weight)> GetCandidates(FrontierRuntime frontier, Actor actor)
    {
        var world = (StaticWorld)actor.World;
        if (!world.Combat.IsAvailableForNewCombat(actor))
            yield break;
        //foreach (var k in frontier.ActorsActiveNew.Where(a => a != actor.RefId).Select(id => (EncounterDefOf.ActorEncounter, new WorldActionData_Combat(actor) { Other = id }, 1000)))
        foreach (var k in frontier.ActorsActiveNew
            .Where(a => a != actor.RefId && world.Combat.IsAvailableForNewCombat(a) && !actor.Biology.IsIncapacitated)
            //.Select(id => (EncounterDefOf.ActorEncounter, new WorldActionWorker_Attack.Data(actor) { TargetId = id }, 1000)))
            .Select(id => (EncounterDefOf.ActorEncounter, new WorldActionData_Combat(actor) { TargetId = id }, 1000)))
            yield return k;
    }

    internal override WorldActionDef? DecideAction(WorldActionData context)
    {
        //var ctx = (WorldActionWorker_Attack.Data)context;
        //if (ctx.Target.Biology.IsIncapacitated)
        //    return null;
        //return WorldActionDefOf.Attack;
        var ctx = (WorldActionData_Combat)context;
        var other = ctx.Actor.World.Resolve<Actor>(ctx.TargetId);
        if (other.Biology.IsIncapacitated)
            throw new InvalidOperationException("Tried to initiate combat against incapacitated actor.");
        return WorldActionDefOf.Combat;
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

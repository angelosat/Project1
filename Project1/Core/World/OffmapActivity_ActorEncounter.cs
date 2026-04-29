using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Systems.Biology;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Effects;
using Project1.Core.Systems.Thoughts;
using Project1.Core.World.WorldAreas;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Helpers;
using System.Linq;

namespace Project1.Core.World;

internal sealed class OffmapActivity_ActorEncounter : OffmapActivity
{
    internal override void Tick(FrontierWrapper frontier, Actor actor)
    {
        var otherActors = frontier.Actors.Except([actor]).ToList();
        if (otherActors.Count == 0)
            return;
        var other = otherActors.SelectRandom(actor.World.Random);
        if (other.Biology.IsIncapacitated)
        {
            TryUsePotion(actor, other);
            //HealArbitrary(actor, other);
        }
        else
        {
            actor.Relationships.ApplyDelta(other, 1);
            other.Relationships.ApplyDelta(actor, 1);
            actor.World.Events.Post(new ActorsMetOffMapEvent(actor, other));
        }
    }

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
            if (actor.World.Get((EntityRefId)other.RefId) is null)
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
        if (actor.World.Get((EntityRefId)other.RefId) is null)
            throw new System.Exception();
    }

    internal override int GetWeight(FrontierWrapper frontier, Actor actor)
        => frontier.IncapacitatedActors.Count * 10;

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

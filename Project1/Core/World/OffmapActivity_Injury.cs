using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Systems.Thoughts;
using Project1.Core.World.WorldAreas;
using Project1.Framework;
using Project1.Framework.Events;
using System;

namespace Project1.Core.World;

internal sealed class OffmapActivity_Injury : OffmapActivity
{
    internal override void Tick(FrontierWrapper frontier, Actor actor)
    {
        var dmgBase = frontier.Def.Tier * 2;
        var dmg = dmgBase + actor.World.Random.Next(-dmgBase, dmgBase) / 2;
        //dmg += 50;
        actor.Resources.ApplyDelta(ResourceDefOf.Health, -dmg);
        actor.World.Events.Post(new ActorOffmapHealthEvent(actor, dmg));
        $"{actor} received {dmg} damage".ToConsole();
    }

    internal override int GetWeight(FrontierWrapper frontier, Actor actor)
    {
        return frontier.Def.Tier;
    }

    sealed class Thought_Injury : ThoughtSource<ActorOffmapHealthEvent>
    {
        internal override void Handle(ActorOffmapHealthEvent e)
        {
            var heal = e.Delta > 0;
            e.Actor.AI.State.Log.Write($"{(heal ? "Recovered" : "Lost")} {Math.Abs(e.Delta)} health");
            //e.Actor.AI.State.Log.Write($"Lost {e.Damage} health");
        }
    }

    record struct ActorOffmapHealthEvent(Actor Actor, int Delta) : IEventPayload;

    //internal override void Tick(FrontierWrapper frontier, Actor actor)
    //{
    //    var dmgBase = frontier.Def.Tier * 2;
    //    var dmg = dmgBase + actor.World.Random.Next(-dmgBase, dmgBase) / 2;
    //    dmg += 50;
    //    actor.Resources.ApplyDelta(ResourceDefOf.Health, -dmg);
    //    //actor.AI.State.Log.Write($"I was injured! ({delta} hp)");
    //    //actor.AI.State.Log.Write($"[Lost {dmg} health,{Color.Red}]");// while exploring {this.Name}");
    //    actor.AI.State.Log.Write($"Lost {dmg} health");// while exploring {this.Name}");
    //}
}

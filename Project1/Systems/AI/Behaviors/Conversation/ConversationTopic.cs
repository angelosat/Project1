using Project1.Core.Towns.AI.Needs;
using Project1.Framework.Base;
using Project1.Framework.Entities.Actors;
using System;
using System.Linq;

namespace Start_a_Town_
{
    public class ConversationTopic : Def
    {
        static public readonly ConversationTopic Guidance = new("Guidance")
        {
            Apply = (actor, convo) =>
            {
                var pop = actor.Map.World.Population;
                foreach (var p in convo.GetParticipants().Where(a => a != actor))
                {
                    //p.ModifyNeed(AdventurerNeedsDefOf.Guidance, n => 50);
                    p.GetNeed(AdventurerNeedsDefOf.Guidance).SetValue(50);
                    var props = pop.GetVisitorProperties(p);
                    actor.Net.Report($"{p.Name} received guidance by {actor.Name}");
                }
            },
            ApplyNew = (source, target) =>
            {
                //target.ModifyNeed(AdventurerNeedsDefOf.Guidance, n => n + 15);
                target.GetNeed(AdventurerNeedsDefOf.Guidance).ApplyDelta(15);
                target.Net.Report($"{source.Name} received guidance by {target.Name}");
            },
            Tick = (source, target) =>
            {
            }
        };
        static public readonly ConversationTopic Riches = new("Riches");
        static public readonly ConversationTopic Fame = new("Fame");
        static public readonly ConversationTopic Lore = new("Lore");

        public Action<Actor, ConversationNew> Apply;
        public Action<Actor, Actor> ApplyNew;
        public Action<Actor, Actor> Tick;

        public int MaxTicks = 5;

        public ConversationTopic(string name) : base(name)
        {
        }
        static ConversationTopic()
        {
            Def.Register(Guidance);
            Def.Register(Riches);
            Def.Register(Fame);
            Def.Register(Lore);
        }
    }
}

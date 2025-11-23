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
                    p.ModifyNeed(VisitorNeedsDefOf.Guidance, n => 50);
                    var props = pop.GetVisitorProperties(p);
                    actor.Net.Write($"{p.Name} received guidance by {actor.Name}");
                }
            },
            ApplyNew = (source, target) =>
            {
                target.ModifyNeed(VisitorNeedsDefOf.Guidance, n => n + 15);
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

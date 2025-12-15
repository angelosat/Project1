using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public sealed class MoodletDef : Def
    {
        public Moodlet.Modes Mode;
        public string Description;
        public int Value, Duration;
        public Func<Actor, bool> Condition;
       
        public MoodletDef(string name) : base(name)
        {
        }
        
        

        static public readonly HashSet<MoodletDef> All = new HashSet<MoodletDef>() { MoodLetDefOf.NoRoom, MoodLetDefOf.JustAte };

        public bool TryAssignOrRemove(Actor actor)
        {
            var hasMoodlet = actor.HasMoodlet(this);
            var condition = this.Condition?.Invoke(actor) ?? false;
            if (condition && !hasMoodlet)
            {
                actor.AddMoodlet(this.Create());
                return true;
            }
            else if (!condition && hasMoodlet)
            {
                actor.RemoveMoodlet(this);
                return true;
            }
            return false;
        }

        public Moodlet Create()
        {
            return new Moodlet(this);
        }
    }
}

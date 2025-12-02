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
        static public void Init() 
        {
            Register(NoRoom);
            Register(JustAte);
        }
        MoodletDef(string name) : base(name)
        {
        }
        static public readonly MoodletDef NoRoom = new("NoRoom")
        {
            Description = "No room assigned",
            Value = -15,
            Mode = Moodlet.Modes.Indefinite,
            Condition = a => a.IsTownMember && a.AssignedRoom == null
        };
        static public readonly MoodletDef JustAte = new("Meal")
        {
            Description = "Just had a nice meal",
            Value = 20,
            Mode = Moodlet.Modes.Finite,
            Duration = Ticks.PerSecond * 10
        };

        static public readonly HashSet<MoodletDef> All = new HashSet<MoodletDef>() { NoRoom, JustAte };

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

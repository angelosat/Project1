using Project1.Framework.Mood;
using Start_a_Town_;

namespace Project1.Core.Mood
{
    [EnsureStaticCtorCall]
    static public class MoodLetDefOf
    {
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
        static MoodLetDefOf()
        {
            Def.Register(typeof(MoodLetDefOf));
        }
    }
}

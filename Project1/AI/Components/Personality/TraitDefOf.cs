namespace Start_a_Town_
{
    public class TraitDefOf
    {
        static public readonly TraitDef Patience = new TraitDef("Patience")
        {
            NameNegative = "Impatient",
            NamePositive = "Patient"
        };
        static public readonly TraitDef Attention = new TraitDef("Attention")
        {
            NameNegative = "Absent minded",
            NamePositive = "Focused"
        };
        static public readonly TraitDef Composure = new TraitDef("Composure")
        {
            NameNegative = "Nervous",
            NamePositive = "Calm"
        };
        static public readonly TraitDef Activity = new TraitDef("Activity")
        {
            NameNegative = "Lazy",
            NamePositive = "Athletic",
            Description = "Affects how many items the actor will decide to carry during hauling tasks, depending on their weight. Also determines the stamina threshold below wich he won't start any new tasks."
        };
        static public readonly TraitDef Planning = new TraitDef("Planning")
        {
            NameNegative = "Hasty",
            NamePositive = "Thorough",
            Description = "Affects the range of which the actor will search for opportunistic hauls."
        };
        static public readonly TraitDef Resilience = new TraitDef("Resilience") //stout? stoic?
        {
            NameNegative = "Oversensitive",
            NamePositive = "Thick-skinned",
            Description = "Affects how fast mood changes."
        };
    }
}
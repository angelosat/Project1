namespace Project1.Core.AI.Personality;

public class TraitDefOf
{
    static TraitDefOf()
        => Def.Register(typeof(TraitDefOf));
    
    static public readonly TraitDef Patience = new TraitDef("Patience")
    {
        NameNegative = "Impatient",
        NamePositive = "Patient"
    };
    //static public readonly TraitDef Attention = new TraitDef("Attention")
    static public readonly TraitDef Focus = new TraitDef("Attention")
    {
        //NameNegative = "Absent minded",
        NameNegative = "Distracted",
        NamePositive = "Focused"
    };
    //static public readonly TraitDef Composure = new TraitDef("Composure")
    static public readonly TraitDef Temperament = new TraitDef("Composure")
    {
        NameNegative = "Nervous",
        NamePositive = "Calm"
    };
    ////static public readonly TraitDef Activity = new TraitDef("Activity")
    //static public readonly TraitDef Industriousness = new TraitDef("Industriousness")
    //{
    //    NameNegative = "Lazy",
    //    //NamePositive = "Athletic",
    //    NamePositive = "Hardworking",
    //    Description = "Affects how many items the actor will decide to carry during hauling tasks, depending on their weight. Also determines the stamina threshold below wich he won't start any new tasks."
    //};
    //static public readonly TraitDef Activity = new TraitDef("Activity")
    static public readonly TraitDef Drive = new("Activity")
    {
        NameNegative = "Unmotivated",
        NamePositive = "Driven",
    };

    /// <summary>
    /// Affects the range of which the actor will search for opportunistic hauls
    /// </summary>
    static public readonly TraitDef Deliberation = new TraitDef("Planning")
    {
        NameNegative = "Hasty",
        //NamePositive = "Thorough",
        NamePositive = "Methodical",
        Description = "Affects the range of which the actor will search for opportunistic hauls."
    };
    static public readonly TraitDef Resilience = new TraitDef("Resilience")
    {
        //NameNegative = "Oversensitive",
        NameNegative = "Sensitive",
        NamePositive = "Thick-skinned",
        Description = "Affects how fast mood changes."
    };
    static public readonly TraitDef Manners = new("Manners")
    {
        NameNegative = "Rude",
        NamePositive = "Polite"
    };
    static public readonly TraitDef Selflessness = new("Selflessness")
    {
        NameNegative = "Manipulative",
        NamePositive = "Altruistic"
    };
    // empathy
    static public readonly TraitDef Sociability = new("Sociability", typeof(TraitWorker_Introvert))
    {
        NameNegative = "Extrovert",
        NamePositive = "Introvert"
    };
}
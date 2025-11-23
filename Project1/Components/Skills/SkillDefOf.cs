using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public static class SkillDefOf
    {
        static public readonly SkillDef Digging = new("Digging")
        {
            Description = "Efficiency when digging soil, gravel, mud, and sand.",
            Icon = new Icon(UIManager.Icons32, 12, 32)
        };
        static public readonly SkillDef Mining = new("Mining")
        {
            Description = "Efficiency when mining stone and minerals.",
            Icon = new Icon(UIManager.Icons32, 12, 32)
        };
        static public readonly SkillDef Construction = new("Construction")
        {
            Description = "Efficiency when building structures such as furniture or walls.",
            Icon = new Icon(UIManager.Icons32, 12, 32)
        };
        static public readonly SkillDef Cooking = new("Cooking")
        {
            Description = "cooking description",
            Icon = new Icon(UIManager.Icons32, 12, 32)
        };
        static public readonly SkillDef Tinkering = new("Tinkering")
        {
            Description = "Tinkering description",
            Icon = new Icon(UIManager.Icons32, 12, 32)
        };
        static public readonly SkillDef Argiculture = new("Argiculture")
        {
            Description = "Argiculture description",
            Icon = new Icon(UIManager.Icons32, 12, 32)
        };
        static public readonly SkillDef Carpentry = new("Carpentry")
        {
            Description = "Carpentry description",
            Icon = new Icon(UIManager.Icons32, 12, 32)
        };
        static public readonly SkillDef Crafting = new("Crafting")
        {
            Description = "Crafting description",
            Icon = new Icon(UIManager.Icons32, 12, 32)
        };
        static public readonly SkillDef Plantcutting = new("Plantcutting")
        {
            Description = "Plantcutting description",
            Icon = new Icon(UIManager.Icons32, 12, 32)
        };
        static public readonly SkillDef[] All = { Digging, Mining, Construction, Cooking, Tinkering, Argiculture, Carpentry, Crafting, Plantcutting };
        static SkillDefOf()
        {
            Def.Register(Digging);
            Def.Register(Mining);
            Def.Register(Construction);
            Def.Register(Cooking);
            Def.Register(Tinkering);
            Def.Register(Argiculture);
            Def.Register(Carpentry);
            Def.Register(Crafting);
            Def.Register(Plantcutting);
        }
    }
}

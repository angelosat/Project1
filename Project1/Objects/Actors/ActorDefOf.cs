namespace Start_a_Town_
{
    static class ActorDefOf
    {
        static public readonly ActorDef NpcProps = new("NpcProps")
        {
            Needs = [
                NeedDefOf.Energy,
                NeedDefOf.Hunger,
                NeedDefOf.Comfort,
                NeedDefOf.Social,
                NeedDefOf.Work ],
            Attributes = [
                AttributeDefOf.Strength,
                AttributeDefOf.Intelligence,
                AttributeDefOf.Dexterity ],
            Resources = [
                ResourceDefOf.Health,
                ResourceDefOf.Stamina ],
            GearSlots = [ 
                GearType.Mainhand,
                GearType.Offhand,
                GearType.Head,
                GearType.Chest,
                GearType.Feet,
                GearType.Hands,
                GearType.Legs ],
            Skills = [
                SkillDefOf.Digging,
                SkillDefOf.Mining,
                SkillDefOf.Construction,
                SkillDefOf.Cooking,
                SkillDefOf.Tinkering,
                SkillDefOf.Argiculture,
                SkillDefOf.Carpentry,
                SkillDefOf.Crafting,
                SkillDefOf.Plantcutting ]
            ,
            Traits =
            [
                TraitDefOf.Attention,
                TraitDefOf.Composure,
                TraitDefOf.Patience,
                TraitDefOf.Activity,
                TraitDefOf.Planning,
                TraitDefOf.Resilience ]
        };

        static public readonly ItemDef Npc = new("Npc")
        {
            ItemClass = typeof(Actor),
            Description = "A person.",
            Height = 1.5f,
            Weight = 50,
            Body = BodyDef.NpcNew,
            DefaultMaterial = MaterialDefOf.Human,
            ActorProperties = NpcProps,
            Factory = Actor.Create,
            Size = ObjectSize.Haulable
        };

        static ActorDefOf()
        {
            Def.Register(Npc);
        }
    }
}

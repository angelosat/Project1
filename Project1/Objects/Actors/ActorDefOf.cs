using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    static class ActorDefOf
    {
        static public readonly ItemVariantDef NpcProps = new ItemVariantDef(ActorDefOf.Npc, "NpcProps")
        .AddSpec(new NeedsComponent.Spec([
                NeedDefOf.Energy,
                NeedDefOf.Hunger,
                NeedDefOf.Comfort,
                NeedDefOf.Social,
                NeedDefOf.Work ]))
        .AddSpec(new AttributesComponent.Spec([
                AttributeDefOf.Strength,
                AttributeDefOf.Intelligence,
                AttributeDefOf.Dexterity ]))
        .AddSpec(new ResourcesComponent.Spec([
                ResourceDefOf.Health,
                ResourceDefOf.Stamina ]))
        .AddSpec(new GearComponent.Spec([
                GearType.Mainhand,
                GearType.Offhand,
                GearType.Head,
                GearType.Chest,
                GearType.Feet,
                GearType.Hands,
                GearType.Legs ]))
        .AddSpec(new NpcSkillsComponent.Spec([
                SkillDefOf.Digging,
                SkillDefOf.Mining,
                SkillDefOf.Construction,
                SkillDefOf.Cooking,
                SkillDefOf.Tinkering,
                SkillDefOf.Argiculture,
                SkillDefOf.Carpentry,
                SkillDefOf.Crafting,
                SkillDefOf.Plantcutting ]))
        .AddSpec(new PersonalityComponent.Spec([
                TraitDefOf.Attention,
                TraitDefOf.Composure,
                TraitDefOf.Patience,
                TraitDefOf.Activity,
                TraitDefOf.Planning,
                TraitDefOf.Resilience ]))

        ;

        static public readonly ItemDef Npc = new ItemDef("Npc", typeof(Actor))
        {
            Description = "A person.",
            Height = 1.5f,
            Weight = 50,
            Body = BodyDef.NpcNew,
            DefaultMaterial = MaterialDefOf.Human,
            IsHaulable = false,
            //ActorProperties = NpcProps,
            //Factory = Actor.Create,
            Size = ObjectSize.Haulable,
            CompTypes = [
                typeof(PossessionsComponent),
                typeof(HaulComponent),
                typeof(NpcComponent),
                typeof(InventoryComponent),
                typeof(StatsComponent),
                typeof(MobileComponent),
                typeof(MoodComp),
                typeof(WorkComponent),
                typeof(EffectsComponent),
                typeof(ResourcesComponent),
                typeof(NeedsComponent),
                typeof(AttributesComponent),
                typeof(NpcSkillsComponent),
                typeof(GearComponent),
                typeof(PersonalityComponent),
                typeof(AIComponent),
            ]
        }
            //.AddSpec(new ResourcesComponent.Spec([
            //    ResourceDefOf.Health, 
            //    ResourceDefOf.Stamina ]))
            //.AddSpec(new NeedsComponent.Spec([
            //    NeedDefOf.Energy,
            //    NeedDefOf.Hunger,
            //    NeedDefOf.Comfort,
            //    NeedDefOf.Social,
            //    NeedDefOf.Work ]))
            //.AddSpec(new AttributesComponent.Spec([
            //    AttributeDefOf.Strength,
            //    AttributeDefOf.Intelligence,
            //    AttributeDefOf.Dexterity ]))
            //.AddSpec(new NpcSkillsComponent.Spec([
            //    SkillDefOf.Digging,
            //    SkillDefOf.Mining,
            //    SkillDefOf.Construction,
            //    SkillDefOf.Cooking,
            //    SkillDefOf.Tinkering,
            //    SkillDefOf.Argiculture,
            //    SkillDefOf.Carpentry,
            //    SkillDefOf.Crafting,
            //    SkillDefOf.Plantcutting ]))
            //.AddSpec(new GearComponent.Spec([
            //    GearType.Mainhand,
            //    GearType.Offhand,
            //    GearType.Head,
            //    GearType.Chest,
            //    GearType.Feet,
            //    GearType.Hands,
            //    GearType.Legs ]))
            //.AddSpec(new PersonalityComponent.Spec([
            //    TraitDefOf.Attention,
            //    TraitDefOf.Composure,
            //    TraitDefOf.Patience,
            //    TraitDefOf.Activity,
            //    TraitDefOf.Planning,
            //    TraitDefOf.Resilience ]))
            //.AddSpec(new PossessionsComponent.Spec())
            //.AddSpec(new HaulComponent.Spec())
            //.AddSpec(new NpcComponent.Spec())
            //.AddSpec(new InventoryComponent.Spec(16))
            //.AddSpec(new StatsComponent.Spec())
            //.AddSpec(new MobileComponent.Spec())
            //.AddSpec(new MoodComp.Spec())
            //.AddSpec(new WorkComponent.Spec())
            //.AddSpec(new EffectsComponent.Spec())
            ;

        static ActorDefOf()
        {
            Def.Register(Npc);
        }
    }
}

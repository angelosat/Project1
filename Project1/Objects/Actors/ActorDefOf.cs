using Project1.Core.Gear;
using Project1.Core.Materials;
using Project1.Core.Needs;
using Project1.Framework.Attributes;
using Project1.Framework.Base;
using Project1.Framework.Components;
using Project1.Framework.Effects;
using Project1.Framework.Gear;
using Project1.Framework.Interactions;
using Project1.Framework.Inventory;
using Project1.Framework.Mood;
using Project1.Framework.Needs;
using Project1.Framework.Ownership;
using Project1.Framework.Physics;
using Project1.Framework.Resources;
using Project1.Framework.Skills;
using Project1.Framework.Stats;
using Start_a_Town_.AI.Behaviors;

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
                GearTypeDefOf.Mainhand,
                GearTypeDefOf.Offhand,
                GearTypeDefOf.Head,
                GearTypeDefOf.Chest,
                GearTypeDefOf.Feet,
                GearTypeDefOf.Hands,
                GearTypeDefOf.Legs ]))
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

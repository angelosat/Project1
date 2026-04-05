using Project1.Framework;
using Project1.Core.Needs;
using Project1.Core.Gear;
using Project1.Core.AI.Behaviors;
using Project1.Core.Resources;
using Project1.Core.Skills;
using Project1.Core.Attributes;
using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.Systems.Materials;
using Project1.Core.AI.Personality;

namespace Project1.Core.Entities.Actors
{
    [EnsureStaticCtorCall]
    public class ActorDnaDefOf
    {
        public static readonly ActorDnaDef Npc = new("Npc")
        {
            Needs = [
                NeedDefOf.Energy,
                NeedDefOf.Hunger,
                NeedDefOf.Comfort,
                NeedDefOf.Social ],
            Attributes = [
                AttributeDefOf.Strength,
                AttributeDefOf.Intelligence,
                AttributeDefOf.Dexterity ],
            Resources = [
                ResourceDefOf.Health,
                ResourceDefOf.Stamina,
                ResourceDefOf.Patience],
            Skills = [
                SkillDefOf.Digging,
                SkillDefOf.Mining,
                SkillDefOf.Construction,
                SkillDefOf.Cooking,
                SkillDefOf.Tinkering,
                SkillDefOf.Argiculture,
                SkillDefOf.Carpentry,
                SkillDefOf.Crafting,
                SkillDefOf.Plantcutting,
                SkillDefOf.Social
                ],
            Gear = [
                GearTypeDefOf.Mainhand,
                GearTypeDefOf.Offhand,
                GearTypeDefOf.Head,
                GearTypeDefOf.Chest,
                GearTypeDefOf.Feet,
                GearTypeDefOf.Hands,
                GearTypeDefOf.Legs ],
            Traits = [
                TraitDefOf.Focus,
                TraitDefOf.Temperament,
                TraitDefOf.Patience,
                TraitDefOf.Drive,
                TraitDefOf.Deliberation,
                TraitDefOf.Resilience,
                TraitDefOf.Manners,
                TraitDefOf.Selflessness],
            Diet = [
                MaterialTypeDefOf.Fruit,
                MaterialTypeDefOf.Flesh,
                //MaterialTypeDefOf.Fiber,
                ],
            Behavior = BehaviorPackageDefOf.Npc.Root.Clone() as Behavior
        };

        static ActorDnaDefOf()
        {
            Def.Register(typeof(ActorDnaDefOf));
        }
    }
}

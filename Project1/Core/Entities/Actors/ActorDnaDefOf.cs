using Project1.Framework;
using Project1.Core.Needs;
using Project1.Core.Gear;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI;
using Project1.Core.Resources;
using Project1.Core.Skills;
using Project1.Core.Attributes;
using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.Systems.Materials;

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
                ResourceDefOf.Stamina ],
            Skills = [
                SkillDefOf.Digging,
                SkillDefOf.Mining,
                SkillDefOf.Construction,
                SkillDefOf.Cooking,
                SkillDefOf.Tinkering,
                SkillDefOf.Argiculture,
                SkillDefOf.Carpentry,
                SkillDefOf.Crafting,
                SkillDefOf.Plantcutting ],
            Gear = [
                GearTypeDefOf.Mainhand,
                GearTypeDefOf.Offhand,
                GearTypeDefOf.Head,
                GearTypeDefOf.Chest,
                GearTypeDefOf.Feet,
                GearTypeDefOf.Hands,
                GearTypeDefOf.Legs ],
            Traits = [
                TraitDefOf.Attention,
                TraitDefOf.Composure,
                TraitDefOf.Patience,
                TraitDefOf.Activity,
                TraitDefOf.Planning,
                TraitDefOf.Resilience ],
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

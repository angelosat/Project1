using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
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
                GearType.Mainhand,
                GearType.Offhand,
                GearType.Head,
                GearType.Chest,
                GearType.Feet,
                GearType.Hands,
                GearType.Legs ],
            Traits = [
                TraitDefOf.Attention,
                TraitDefOf.Composure,
                TraitDefOf.Patience,
                TraitDefOf.Activity,
                TraitDefOf.Planning,
                TraitDefOf.Resilience ],
            Behavior = BehaviorPackageDefOf.Npc.Root.Clone() as Behavior
            //new BehaviorQueue(
            //       new AIMemory(),
            //       new BehaviorHandleResources(),
            //       new BehaviorHandleOrders(),
            //       new BehaviorHandleTasks())
        };

        static ActorDnaDefOf()
        {
            Def.Register(typeof(ActorDnaDefOf));
        }
    }
}

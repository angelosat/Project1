using Project1.Core.AI;
using Project1.Core.Attributes;
using Project1.Core.Entities.Stats;
using Project1.Core.Resources;
using Project1.Core.Components;
using Project1.Core.Simulation.Physics;
using Project1.Core.Skills;
using Project1.Core.Interactions;
using Project1.Core.Mood;
using Project1.Core.Gear;
using Project1.Core.Needs;
using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Inventory;

namespace Project1.Core.Entities.Actors
{
    static class ActorDefOf
    {
        static public readonly ItemDef Npc = new("Npc", typeof(Actor))
        {
            Description = "A person.",
            Height = 1.5f,
            Weight = 50,
            Body = BodyDef.NpcNew,
            DefaultMaterial = MaterialDefOf.Human,
            IsHaulable = false,
            Size = ObjectSize.Haulable,
            Comps = [
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
                typeof(SkillsComponent),
                typeof(GearComponent),
                typeof(PersonalityComponent),
                typeof(AIComponent),
            ],
            CompDefs = [
                EntityCompDefOf.Possessions,
                EntityCompDefOf.Haul,
                EntityCompDefOf.Npc,
                EntityCompDefOf.Inventory,
                EntityCompDefOf.Stats,
                EntityCompDefOf.Mobile,
                EntityCompDefOf.Mood,
                EntityCompDefOf.Work,
                EntityCompDefOf.Effects,
                EntityCompDefOf.Resources,
                EntityCompDefOf.Needs,
                EntityCompDefOf.Attributes,
                EntityCompDefOf.Skills,
                EntityCompDefOf.Gear,
                EntityCompDefOf.Personality,
                EntityCompDefOf.AI
            ]
        };

        static ActorDefOf()
        {
            Def.Register(Npc);
        }
    }
}

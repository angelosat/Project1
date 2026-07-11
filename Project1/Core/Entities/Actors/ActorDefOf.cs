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
using Project1.Core.Systems.Abilities;
using Project1.Core.Systems.Biology;
using Project1.Core.Systems.MentalState;
using Project1.Core.Systems.Recipes;
using Project1.Core.Systems.Gear;

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
            DefaultBoneStruct = BoneStructureDefOf.Npc,
            DefaultMaterial = MaterialDefOf.Human,
            IsHaulable = false,
            Size = ObjectSize.Haulable,
            Comps = [
                typeof(PossessionsComponent),
                typeof(HaulComponent),
                typeof(NpcComponent),
                typeof(InventoryComp),
                typeof(StatsComp),
                typeof(MobileComponent),
                typeof(MoodComp),
                typeof(WorkComponent),
                typeof(EffectsComp),
                typeof(ResourcesComp),
                typeof(NeedsComp),
                typeof(AttributesComponent),
                typeof(SkillsComponent),
                typeof(GearComp),
                typeof(PersonalityComponent),
                typeof(AIComp),
                typeof(BiologyComp),
                typeof(AbilitiesComp),
                typeof(MentalStateComp),
                typeof(RecipesComp),
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
                EntityCompDefOf.AI,
                EntityCompDefOf.Relationships,
                EntityCompDefOf.Biology,
                EntityCompDefOf.Abilities,
                EntityCompDefOf.MentalState,
                EntityCompDefOf.Recipes
            ]
        };

        static ActorDefOf()
        {
            Def.Register(Npc);
        }
    }
}

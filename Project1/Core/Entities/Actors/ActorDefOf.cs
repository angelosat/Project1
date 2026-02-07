using Project1.Core.AI;
using Project1.Core.Attributes;
using Project1.Core.Entities.Mood;
using Project1.Core.Entities.Ownership;
using Project1.Core.Entities.Stats;
using Project1.Core.Materials;
using Project1.Core.Resources;
using Project1.Core.Base;
using Project1.Core.Components;
using Project1.Core.Entities.Actors;
using Project1.Core.Simulation.Physics;

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
                typeof(SkillsComponent),
                typeof(GearComponent),
                typeof(PersonalityComponent),
                typeof(AIComponent),
            ]
        };

        static ActorDefOf()
        {
            Def.Register(Npc);
        }
    }
}

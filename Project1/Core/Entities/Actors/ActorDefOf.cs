using Project1.Core.Gear;
using Project1.Core.Materials;
using Project1.Core.Needs;
using Project1.Framework.Attributes;
using Project1.Framework.Base;
using Project1.Framework.Components;
using Project1.Framework.Effects;
using Project1.Framework.Entities;
using Project1.Framework.Entities.Actors;
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
using Start_a_Town_;

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
                typeof(NpcSkillsComponent),
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

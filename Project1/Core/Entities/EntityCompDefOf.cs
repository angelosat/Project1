using Project1.Framework;
using Project1.Core.AI;
using Project1.Core.Attributes;
using Project1.Core.Components;
using Project1.Core.Components.Combat;
using Project1.Core.Components.Plants;
using Project1.Core.Entities.Mood;
using Project1.Core.Entities.Stats;
using Project1.Core.Graphics.Particles;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Plants;
using Project1.Core.Resources;
using Project1.Core.Simulation.Physics;
using Project1.Core.Tools;
using Project1.Core.Gear;
using Project1.Core.Skills;
using Project1.Core.Interactions;
using Project1.Core.Animations;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Entities
{
    [EnsureStaticCtorCall]
    public class EntityCompDefOf
    {
        public static readonly EntityCompDef DefComp = new("Def", typeof(DefComponent));
        public static readonly EntityCompDef Transform = new("Transform", typeof(PositionComponent));
        public static readonly EntityCompDef Physics = new("Physics", typeof(PhysicsComponent));
        public static readonly EntityCompDef Skills = new("Skills", typeof(SkillsComponent));
        public static readonly EntityCompDef Needs = new("Needs", typeof(NeedsComponent));
        public static readonly EntityCompDef Ownership = new("Ownership", typeof(OwnershipComponent));
        public static readonly EntityCompDef Stats = new("Stats", typeof(StatsComponent));
        public static readonly EntityCompDef Attributes = new("Attributes", typeof(AttributesComponent));
        public static readonly EntityCompDef Resources = new("Resources", typeof(ResourcesComponent));
        public static readonly EntityCompDef Work = new("Work", typeof(WorkComponent));
        public static readonly EntityCompDef AI = new("AI", typeof(AIComponent));
        public static readonly EntityCompDef Tool = new("Tool", typeof(ToolComp));
        public static readonly EntityCompDef Plant = new("Plant", typeof(PlantComponent));
        public static readonly EntityCompDef Sprite = new("Sprite", typeof(SpriteComp));
        public static readonly EntityCompDef Haul = new("Haul", typeof(HaulComponent));
        public static readonly EntityCompDef Inventory = new("Inventory", typeof(InventoryComponent));
        public static readonly EntityCompDef Possessions = new("Possesions", typeof(PossessionsComponent));
        public static readonly EntityCompDef Mobile = new("Mobile", typeof(MobileComponent));
        public static readonly EntityCompDef Mood = new("Mood", typeof(MoodComp));
        public static readonly EntityCompDef Gear = new("Gear", typeof(GearComponent));
        public static readonly EntityCompDef Effects = new("Effects", typeof(EffectsComponent));
        public static readonly EntityCompDef Personality = new("Personality", typeof(PersonalityComponent));
        public static readonly EntityCompDef Npc = new("Npc", typeof(NpcComponent));
        public static readonly EntityCompDef Seed = new("Seed", typeof(SeedComponent));
        public static readonly EntityCompDef Consumable = new("Consumable", typeof(ConsumableComponent));
        public static readonly EntityCompDef UnfinishedItem = new("UnfinishedItem", typeof(UnfinishedItemComp));
        public static readonly EntityCompDef Blood = new("Blood", typeof(BloodComponent));
        public static readonly EntityCompDef Bomb = new("Bomb", typeof(BombComponent));
        public static readonly EntityCompDef Block = new("Block", typeof(BlockingComponent));
        public static readonly EntityCompDef Equip = new("Equip", typeof(EquipComponent));
        public static readonly EntityCompDef Particles = new("Particles", typeof(ParticlesComponent));
        public static readonly EntityCompDef Tree = new("Tree", typeof(TreeComponent));
        static EntityCompDefOf()
        {
            Def.Register(typeof(EntityCompDefOf));
            //Assembly[] assemblies =
            //[
            //    typeof(EntityComp).Assembly,
            //];
            //IEnumerable<Type> compTypes =
            //    assemblies
            //        .SelectMany(a => a.GetTypes())
            //        .Where(t =>
            //            !t.IsAbstract &&
            //            typeof(EntityComp).IsAssignableFrom(t));
            //foreach(var comptype in compTypes)
            //{
            //    Def.Register(new EntityCompDef(comptype.Name, comptype));
            //}
        }
    }
}

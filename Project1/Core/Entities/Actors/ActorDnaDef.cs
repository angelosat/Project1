using Project1.Core.AI;
using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.AI.Personality;
using Project1.Core.Attributes;
using Project1.Core.Gear;
using Project1.Core.Needs;
using Project1.Core.Resources;
using Project1.Core.Skills;
using Project1.Core.Systems.Materials;
using System.Collections.Generic;

namespace Project1.Core.Entities.Actors
{
    public sealed class ActorDnaDef(string name) : Def(name)
    {
        public NeedDef[] Needs;
        public AttributeDef[] Attributes;
        public SkillDef[] Skills;
        public TraitDef[] Traits;
        public ResourceDef[] Resources;
        public GearTypeDef[] Gear;
        public MaterialTypeDef[] Diet;
        public Behavior Behavior;

        public IEnumerable<EntityComp.Spec> GenerateSpecs()
        {
            yield return new NeedsComponent.Spec(Needs);
            yield return new AttributesComponent.Spec(Attributes);
            yield return new ResourcesComponent.Spec(Resources);
            yield return new GearComponent.Spec(Gear);
            yield return new SkillsComponent.Spec(Skills);
            yield return new PersonalityComponent.Spec(Traits);
            yield return new AIComponent.Spec(Behavior);
        }
    }
}

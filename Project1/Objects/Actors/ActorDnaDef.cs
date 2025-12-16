using Start_a_Town_.Components;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public class ActorDnaDef : Def
    {
        public NeedDef[] Needs;
        public AttributeDef[] Attributes;
        public SkillDef[] Skills;
        public TraitDef[] Traits;
        public ResourceDef[] Resources;
        public GearType[] Gear;
        public Behavior Behavior;
        public ActorDnaDef(string name) : base(name) { }

        public IEnumerable<EntityComp.Spec> GenerateSpecs()
        {
            yield return new NeedsComponent.Spec(Needs);
            yield return new AttributesComponent.Spec(Attributes);
            yield return new ResourcesComponent.Spec(Resources);
            yield return new GearComponent.Spec(Gear);
            yield return new NpcSkillsComponent.Spec(Skills);
            yield return new PersonalityComponent.Spec(Traits);
            yield return new AIComponent.Spec(Behavior);
        }
    }
}

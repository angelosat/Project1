using Project1.Framework.Resources;
using Project1.Framework.Skills;
using Start_a_Town_;

namespace Project1.Framework.Attributes
{
    internal record struct AttributeAdjustedEvent(Actor Owner, AttributeDef Def, float Value) : IEventPayload { }
    internal record struct ResourceAdjustedEvent(Entity Owner, ResourceDef Def, float Value) : IEventPayload { }
    internal record struct SkillAdjustedEvent(Actor Actor, Skill Skill) : IEventPayload { }
    internal record struct SkillLevelUpEvent(Actor Actor, Skill Skill) : IEventPayload { }
}
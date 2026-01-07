namespace Start_a_Town_
{
    internal record struct AttributeAdjustedEvent(Actor Owner, AttributeDef Def, float Value) : IEventPayload { }
    internal record struct ResourceAdjustedEvent(Entity Owner, ResourceDef Def, float Value) : IEventPayload { }
    internal record struct SkillAdjustedEvent(Actor Actor, Skill Skill) : IEventPayload { }
    internal record struct SkillLevelUpEvent(Actor Actor, Skill Skill) : IEventPayload { }
}
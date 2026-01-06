namespace Start_a_Town_
{
    internal record struct AttributeIncreasedEvent(Actor Owner, AttributeDef Def, float Delta) : IEventPayload { }
    internal record struct ResourceAdjustedEvent(Entity Owner, ResourceDef Def, float Delta) : IEventPayload { }
}
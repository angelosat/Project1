namespace Start_a_Town_
{
    public record struct AILocationDecisionEvent(Actor Actor, FrontierDef Frontier) : IEventPayload { }
    public record struct AILogEntry(Actor Actor, string Text) : IEventPayload { }
}

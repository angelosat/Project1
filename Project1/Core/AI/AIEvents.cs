using Project1.Core.World.WorldAreas;
using Project1.Core.Entities.Actors;
using Project1.Framework.Events;

namespace Project1.Core.AI
{
    public record struct AILocationDecisionEvent(Actor Actor, FrontierDef Frontier) : IEventPayload { }
    public record struct AILogEntryEvent(Actor Actor, string Text) : IEventPayload { }
}

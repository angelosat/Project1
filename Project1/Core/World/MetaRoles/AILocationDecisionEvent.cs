using Project1.Core.World.WorldAreas;
using Project1.Core.Base;
using Project1.Core.Entities.Actors;

namespace Project1.Core.World.MetaRoles
{
    public record struct AILocationDecisionEvent(Actor Actor, FrontierDef Frontier) : IEventPayload { }
    public record struct AILogEntry(Actor Actor, string Text) : IEventPayload { }
}

using Project1.Core.Entities.Actors;
using Project1.Framework.Events;

namespace Project1.Core.Towns.Duties
{
    internal record struct DutyUpdatedEvent(Actor Actor, DutyDef Duty) : IEventPayload { }
    internal record struct PlayerDutyAdjustPriorityEvent(Actor Actor, DutyDef Duty, int Delta) : IEventPayload { }
    internal record struct PlayerDutyToggleEvent(Actor Actor, DutyDef Duty) : IEventPayload { }
}

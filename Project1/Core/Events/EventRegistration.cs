using Project1.Framework.Events;
using System;

#nullable enable

namespace Project1.Core.Events;

internal sealed record EventRegistration<T>(Action<T> Handler)
    : IEventHandlerDeclaration
    where T : IEventPayload
{
    public void Subscribe(EventBus bus)
    {
        bus.ListenTo(Handler);
    }
}

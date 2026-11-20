using Project1.Framework.Events;
using System.Collections.Immutable;

#nullable enable

namespace Project1.Core.Events;

internal sealed class EventHandlerDeclarations(params IEventHandlerDeclaration[] items)
{
    private readonly ImmutableArray<IEventHandlerDeclaration> _items = [.. items];

    public void Subscribe(EventBus bus)
    {
        foreach (var item in _items)
            item.Subscribe(bus);
    }
}
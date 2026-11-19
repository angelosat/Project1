using Project1.Framework.Events;

#nullable enable

namespace Project1.Core.Events;

internal interface IEventHandlerDeclaration
{
    void Subscribe(EventBus bus);
}

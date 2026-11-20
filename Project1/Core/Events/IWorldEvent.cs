using Project1.Core.Simulation;
using Project1.Framework.Events;

namespace Project1.Core.Events;

internal interface IWorldEvent : IEventPayload
{
    StaticWorld World { get; set; }
}

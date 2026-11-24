using Project1.Core.Entities;
using Project1.Core.Simulation;
using Project1.Framework.Events;

namespace Project1.Core.Events;

internal interface IWorldEvent : IEventPayload
{
    //T Resolve<T>(EntityRefId refId) where T : Entity => this.World.Resolve<T>(refId);
    StaticWorld World { get; set; }
}

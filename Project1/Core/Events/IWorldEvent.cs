using Project1.Core.Simulation;
using Project1.Framework.Events;
using System;
using System.Linq;

namespace Project1.Core.Events;

internal interface IWorldEvent : IEventPayload
{
    StaticWorld World { get; set; }
}

using Project1.Core.Entities;
using Project1.Core.World.Actions;
using Project1.Framework.Events;
using System;
using System.Linq;

#nullable enable

namespace Project1.Core.World.Frontiers
{
    internal record struct EncounterBeginEvent(Entity Entity, EncounterRuntime Encounter) : IEventPayload { }
}

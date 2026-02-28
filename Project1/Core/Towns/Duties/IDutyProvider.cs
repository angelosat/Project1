using Project1.Core.Simulation;
using System.Collections.Generic;

namespace Project1.Core.Towns.Duties
{
    interface IDutyProvider
    {
        MapBase Map { get; }
        IReadOnlyCollection<DutyDef> AvailableDuties { get; }
    }
}

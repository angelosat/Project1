using Project1.Framework;
using System;
using System.Collections.Generic;

namespace Project1.Core.Towns.Zones
{
    public class ZoneDef(string name, Type zoneClass, Type workerClass) : Def(name)
    {
        public Type RuntimeClass = zoneClass;

        public Type WorkerClass = workerClass;

        ZoneWorker _workerCached;
        public ZoneWorker Worker => _workerCached ??= (ZoneWorker)Activator.CreateInstance(this.WorkerClass);

        public Zone CreateRuntimeWrapper()
        {
            var zone = Activator.CreateInstance(this.RuntimeClass) as Zone;
            return zone;
        }
        public Zone Create(ZoneManager manager, IEnumerable<IntVec3> positions)
        {
            var zone = Activator.CreateInstance(this.RuntimeClass, manager) as Zone;
            zone.Cells.Add(positions);
            return zone;
        }
    }
}
using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public class ZoneDef : Def
    {
        public Type ZoneClass;

        public Type WorkerClass;

        ZoneWorker _workerCached;
        public ZoneWorker Worker => _workerCached ??= (ZoneWorker)Activator.CreateInstance(this.WorkerClass);

        public ZoneDef(string name, Type zoneClass, Type workerClass)
            : base(name)
        {
            this.ZoneClass = zoneClass;
            this.WorkerClass = workerClass;
        }
        public Zone Create()
        {
            var zone = Activator.CreateInstance(this.ZoneClass) as Zone;
            return zone;
        }
        public Zone Create(ZoneManager manager, IEnumerable<IntVec3> positions)
        {
            //return Activator.CreateInstance(this.ZoneClass, manager, positions) as Zone;
            var zone = Activator.CreateInstance(this.ZoneClass, manager) as Zone;
            zone.Positions.Add(positions);
            return zone;
        }
    }
}

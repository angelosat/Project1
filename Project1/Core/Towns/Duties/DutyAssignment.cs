using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Framework.Serialization;
using System.Collections.Generic;

namespace Project1.Core.Towns.Duties
{
    public sealed class DutyAssignment : ISerializableNewNew<DutyAssignment>
    {
        public EntityRefId ActorId { get; private set; }
        readonly Dictionary<DutyDef, Duty> _priorities = [];
        public IReadOnlyDictionary<DutyDef, Duty> Duties => this._priorities;
        DutyAssignment()
        {
            
        }
        public DutyAssignment(Actor actor, IReadOnlyCollection<DutyDef> duties)
        {
            this.ActorId = actor.RefId;
            foreach (var d in duties)
                this._priorities.Add(d, new(d));
        }
        public static DutyAssignment Create(IDataReader r)
        {
            var actorid = r.ReadEntityRefId();
            var result = new DutyAssignment
            {
                ActorId = actorid
            };
            var list = r.ReadListNewNew<Duty>();
            foreach (var d in list)
                result._priorities.Add(d.Def, d);
            return result;
        }

        public IDataWriter Write(IDataWriter w)
        {
            w.Write(this.ActorId);
            w.WriteNew(this._priorities.Values);
            return w;
        }
    }
}

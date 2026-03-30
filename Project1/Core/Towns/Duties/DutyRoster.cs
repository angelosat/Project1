using Project1.Core.Entities.Actors;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns.Duties
{
    public sealed class DutyRoster
    {
        static List<DutyDef> AllDutyDefs => field ??= [.. Def.GetDefs<DutyDef>()];
        Dictionary<Actor, DutyAssignment> _rosterByActor = [];
        Dictionary<DutyDef, HashSet<Actor>> _rosterByDuty;
        public IReadOnlyDictionary<Actor, DutyAssignment> Roster => this._rosterByActor;
        readonly Lazy<DutiesGui> UILabors;
        internal readonly IDutyProvider Provider;
        public IEnumerable<Duty> GetDuties(Actor actor)
            => this.Roster[actor].Duties.Values.Where(d => d.Enabled);
        public bool HasDuty(Actor actor, DutyDef dutyDef) 
            => this.Roster[actor].Duties.TryGetValue(dutyDef, out var duty) && duty.Enabled;
        public Duty GetDuty(Actor actor, DutyDef dutyDef)
        { 
            if (this.Roster[actor].Duties.TryGetValue(dutyDef, out var duty)) 
                return duty;
            return null;
        }
        internal DutyRoster(IDutyProvider provider)
        {
            this.UILabors = new(() => new DutiesGui(this));
            this.Provider = provider;
            this.BuildRosterByDuty();
        }
        internal void Add(Actor actor)
        {
            var assignment = new DutyAssignment(actor, this.Provider.AvailableDuties);
            this._rosterByActor.Add(actor, assignment);
            foreach (var duty in assignment.Duties)
                this._rosterByDuty[duty.Key].Add(actor);
        }
        
        internal void Remove(Actor actor)
        {
            this._rosterByActor.Remove(actor);
        }
        public void ToggleLaborsWindow()
        {
            var window = this.UILabors.Value.GetWindow() ?? new Window("Duties", this.UILabors.Value);
            window.Toggle();
        }

        public IDataWriter Write(IDataWriter w)
        {
            w.WriteNew(this._rosterByActor.Values);
            return w;
        }
        public IDataReader Read(IDataReader r)
        {
            this._rosterByActor = r.ReadListNewNew<DutyAssignment>().ToDictionary(d => this.Provider.Map.World.Get<Actor>(d.ActorId), d => d);
            this.BuildRosterByDuty();
            return r;
        }

        internal void Toggle(Actor actor, DutyDef duty)
        {
            this.Roster[actor].Duties[duty].Toggle();
            var byDuty = this._rosterByDuty[duty];
            if (!byDuty.Remove(actor))
                byDuty.Add(actor);
            this.Provider.Map.Events.Post(new DutyUpdatedEvent(actor, duty));
        }

        internal void ApplyPriorityDelta(Actor actor, DutyDef duty, int delta)
        {
            this.Roster[actor].Duties[duty].ApplyPriorityDelta(delta);
            this.Provider.Map.Events.Post(new DutyUpdatedEvent(actor, duty));
        }

        void BuildRosterByDuty()
        {
            this._rosterByDuty = AllDutyDefs.ToDictionary(d => d, d => new HashSet<Actor>());
            foreach (var kv in this._rosterByActor)
            {
                foreach(var duty in kv.Value.Duties)
                {
                    this._rosterByDuty[duty.Key].Add(kv.Key);
                }
            }
        }
     

        public IReadOnlySet<Actor> GetAssigned(DutyDef duty)
            => this._rosterByDuty[duty];
        public bool HasAssigned(DutyDef duty)
            => this._rosterByDuty[duty].Count > 0;
    }
}

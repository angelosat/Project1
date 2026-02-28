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
        Dictionary<Actor, DutyAssignment> _roster = [];
        public IReadOnlyDictionary<Actor, DutyAssignment> Roster => this._roster;
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
        }
        internal void Add(Actor actor)
        {
            this._roster.Add(actor, new(actor, this.Provider.AvailableDuties));
        }
        
        internal void Remove(Actor actor)
        {
            this._roster.Remove(actor);
        }
        public void ToggleLaborsWindow()
        {
            var window = this.UILabors.Value.GetWindow() ?? new Window("Duties", this.UILabors.Value);
            window.Toggle();
        }

        public IDataWriter Write(IDataWriter w)
        {
            w.WriteNew(this._roster.Values);
            return w;
        }
        public IDataReader Read(IDataReader r)
        {
            this._roster = r.ReadListNewNew<DutyAssignment>().ToDictionary(d => this.Provider.Map.World.GetEntity<Actor>(d.ActorId), d => d);
            return r;
        }

        internal void Toggle(Actor actor, DutyDef duty)
        {
            this.Roster[actor].Duties[duty].Toggle();
            this.Provider.Map.Events.Post(new DutyUpdatedEvent(actor, duty));
        }

        internal void ApplyPriorityDelta(Actor actor, DutyDef duty, int delta)
        {
            this.Roster[actor].Duties[duty].ApplyPriorityDelta(delta);
            this.Provider.Map.Events.Post(new DutyUpdatedEvent(actor, duty));
        }
    }
}

using Project1.Core.AI;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Core.UI;
using Project1.Core.UI.Hud;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

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
    interface IDutyProvider
    {
        MapBase Map { get; }
        //IEntityProvider Entities { get; }
        IReadOnlyCollection<DutyDef> AvailableDuties { get; }
    }
    public sealed class DutyRoster// : ISerializableNewNew<DutyRoster>// : TownComponent
    {
        //static DutyRoster()
        //{
        //    Registry.GameEvents.Register<JobUpdatedEvent>();
        //}
        static List<DutyDef> AllDutyDefs => field ??= [.. Def.GetDefs<DutyDef>()];
        Dictionary<Actor, DutyAssignment> _roster = [];
        public IReadOnlyDictionary<Actor, DutyAssignment> Roster => this._roster;
        readonly Lazy<DutiesGui> UILabors;
        internal readonly IDutyProvider Provider;

        //public override string Name => "Duties"; 


        internal DutyRoster(IDutyProvider provider)
        {
            //this.Town = town;
            //this.UILabors = new Lazy<Control>(this.CreateJobsTable(this));
            this.UILabors = new(() => new DutiesGui(this));
            this.Provider = provider;
            //town.Map.Events.ListenTo<MemberAddedEvent>(OnMemberAdde)
        }
        internal void Add(Actor actor)
        {
            this._roster.Add(actor, new(actor, this.Provider.AvailableDuties));
        }
        //internal override IEnumerable<Tuple<Func<string>, Action>> OnQuickMenuCreated()
        //{
        //    yield return new Tuple<Func<string>, Action>(()=>"Labors", this.ToggleLaborsWindow);
        //}
        internal void Remove(Actor actor)
        {
            this._roster.Remove(actor);
        }

        public void ToggleLaborsWindow()
        {
            var window = this.UILabors.Value.GetWindow() ?? new Window("Duties", this.UILabors.Value);
            window.Toggle();
        }

        internal /*override*/ void OnTargetSelected(IUISelection info, ISelectable target)
        {
            //base.OnTargetSelected(info, target);
        }
        internal /*override*/ void OnTargetSelected(SelectionManager info, ISelectable target)
        {
            //base.OnTargetSelected(info, target);
        }

        Control CreateJobsTable(DutyRoster roster)
        {
            var box = new GroupBox();
            var tableBox = new GroupBox();
            var tableAuto = new TableScrollableCompact<Actor>(true)
                            .AddColumn(null, "Name", 100, o => new Label(o.Name, () => { }));
            var tableManual = new TableScrollableCompact<Actor>(true)
                           .AddColumn(null, "Name", 100, o => new Label(o.Name, () => { }));
            //var player = this.Player;
            foreach (var labor in DutyDefOf.All)
            {
                var ic = labor.Icon;

                var icon = new PictureBox(ic.SpriteSheet, ic.SourceRect) { HoverText = labor.Name };
                var iconManual = new PictureBox(ic.SpriteSheet, ic.SourceRect) { HoverText = labor.Name };

                tableAuto.AddColumn(labor, icon, CheckBoxNew.DefaultBounds.Width, (actor) =>
                {
                    var state = AIState.GetState(actor);
                    var job = state.GetJob(labor);
                    var ch = new CheckBoxNew
                    {
                        Value = actor.HasJob(job.Def),
                        HoverText = job.Def.LabelReadable
                    };
                    ch.LeftClickAction = () => { ch.ToggleValue(); PacketsDuties.SendLaborToggle(actor, labor); };
                    ch.ListenTo<DutyUpdatedEvent>(args =>
                    {
                        if (args.Actor == actor && args.Duty == job.Def)
                            ch.SetChecked(args.Actor.HasJob(args.Duty));
                    });
                    return ch;
                }, 0);
                tableManual.AddColumn(labor, iconManual, CheckBoxNew.DefaultBounds.Width, (actor) =>
                {
                    //var state = AIState.GetState(actor);
                    //var job = state.GetJob(labor);
                    var assignment = roster.Roster[actor];
                    var job = assignment.Duties[labor];
                    var btn = new Button(CheckBoxNew.CheckedRegion.Width)
                    {
                        TextFunc = () => { var val = job.Priority; return job.Enabled ? val.ToString() : ""; },
                        LeftClickAction = () => PacketsDuties.SendPriorityModify(actor, job, job.Priority + 1),
                        RightClickAction = () => PacketsDuties.SendPriorityModify(actor, job, job.Priority - 1),
                        HoverText = job.Def.LabelReadable
                    };
                    return btn;
                }, 0);
            }
            //var net = this.Town.Net;
            //var actors = this.Town.Members.Select(id => net.World.GetEntity(id) as Actor);
            var actors = roster.Roster.Keys;
            tableAuto.AddItems(actors);
            tableManual.AddItems(actors);

            var currentTable = tableAuto;

            tableBox.AddControls(tableAuto);
            var btnTogglePriorities = new CheckBoxNew("Manual priorities") { TickedFunc = () => currentTable == tableManual, LeftClickAction = switchTables };
            box.AddControlsVertically(
                btnTogglePriorities,
                tableBox);

            return box;

            void switchTables()
            {
                tableBox.ClearControls();
                currentTable = currentTable == tableManual ? tableAuto : tableManual;
                tableBox.AddControls(currentTable);
            }
        }

        public IDataWriter Write(IDataWriter w)
        {
            w.WriteNew(this._roster.Values);
            return w;
        }
        public IDataReader Read(IDataReader r)
        {
            //this._roster = r.ReadListNewNew<DutyAssignment>().ToDictionary(d => this.Provider.Entities.GetEntity<Actor>(d.ActorId), d => d);
            this._roster = r.ReadListNewNew<DutyAssignment>().ToDictionary(d => this.Provider.Map.World.GetEntity<Actor>(d.ActorId), d => d);
            return r;
        }

        internal void Toggle(Actor actor, DutyDef duty)
        {
            this.Roster[actor].Duties[duty].Toggle();
            this.Provider.Map.Events.Post(new DutyUpdatedEvent(actor, duty));
        }
    }
    class DutiesGui : GroupBox
    {
        internal DutiesGui(DutyRoster roster)
        {
            //var box = new GroupBox();
            var tableBox = new GroupBox();
            var tableAuto = new TableScrollableCompact<Actor>(true)
                            .AddColumn(null, "Name", 100, o => new Label(o.Name));
            var tableManual = new TableScrollableCompact<Actor>(true)
                           .AddColumn(null, "Name", 100, o => new Label(o.Name));
            foreach (var duty in roster.Provider.AvailableDuties)
            {
                var ic = duty.Icon;

                var icon = new PictureBox(ic.SpriteSheet, ic.SourceRect) { HoverText = duty.Name };
                var iconManual = new PictureBox(ic.SpriteSheet, ic.SourceRect) { HoverText = duty.Name };

                tableAuto.AddColumn(duty, icon, CheckBoxFinal.DefaultBounds.Width, (actor) =>
                {
                    var job = roster.Roster[actor].Duties[duty];
                    var ch = new CheckBoxFinal(() => Ingame.Instance.Events.Post(new PlayerDutyToggleEvent(actor, duty)), () => job.Enabled);
                    var unsub = job.Subscribe(() => ch.Invalidate(true));
                    ch.HideAction = unsub.Dispose;
                    return ch;
                }, 0);
                tableManual.AddColumn(duty, iconManual, CheckBoxNew.DefaultBounds.Width, (actor) =>
                {
                    var assignment = roster.Roster[actor];
                    var job = assignment.Duties[duty];
                    var btn = new Button(CheckBoxNew.CheckedRegion.Width)
                    {
                        TextFunc = () => { var val = job.Priority; return job.Enabled ? val.ToString() : ""; },
                        LeftClickAction = () => PacketsDuties.SendPriorityModify(actor, job, job.Priority + 1),
                        RightClickAction = () => PacketsDuties.SendPriorityModify(actor, job, job.Priority - 1),
                        HoverText = job.Def.LabelReadable
                    };
                    return btn;
                }, 0);
            }
            //var net = this.Town.Net;
            //var actors = this.Town.Members.Select(id => net.World.GetEntity(id) as Actor);
            var actors = roster.Roster.Keys;
            tableAuto.AddItems(actors);
            tableManual.AddItems(actors);

            var currentTable = tableAuto;

            tableBox.AddControls(tableAuto);
            var btnTogglePriorities = new CheckBoxNew("Manual priorities") { TickedFunc = () => currentTable == tableManual, LeftClickAction = switchTables };
            this.AddControlsVertically(
                btnTogglePriorities,
                tableBox);

            //return box;

            void switchTables()
            {
                tableBox.ClearControls();
                currentTable = currentTable == tableManual ? tableAuto : tableManual;
                tableBox.AddControls(currentTable);
            }
        }
    }
}

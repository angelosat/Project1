using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using Start_a_Town_.AI;
using System.IO;

namespace Start_a_Town_
{
    public class JobsManager : TownComponent
    {
        class Packets
        {
            static int pToggle, pMod, pSync;
            static public void Init()
            {
                pToggle = Registry.PacketHandlers.Register(HandleLaborToggle);
                pMod = Registry.PacketHandlers.Register(HandleJobModRequest);
                pSync = Registry.PacketHandlers.Register(HandleJobSync);
            }

            private static void HandleJobModRequest(NetEndpoint net, Packet pck)
            {
                var r = pck.PacketReader;
                var server = net as Server;
                var player = server.GetPlayer(r.ReadInt32());
                var actor = server.World.GetEntity(r.ReadInt32()) as Actor;
                var jobDef = Def.GetDef<JobDef>(r.ReadString());
                var job = actor.GetJob(jobDef);
                job.Read(r);
                net.EventOccured((int)Components.Message.Types.JobUpdated, actor, job.Def);
                SyncJob(player, actor, job);
            }

            public static void SendPriorityModify(PlayerData player, Actor actor, Job job, int priority)
            {
                var net = actor.Net;
                if (net is Server)
                {
                    job.Priority = (byte)priority;
                    net.EventOccured((int)Components.Message.Types.JobUpdated, actor, job.Def);
                    SyncJob(player, actor, job);
                }
                else
                {
                    var w = net.BeginPacket(pMod);
                    w.Write(player.ID, actor.RefId, job.Def.Name, priority);

                }
            }
            public static void SendLaborToggle(PlayerData player, Actor actor, JobDef jobDef)
            {
                var net = actor.Net;
                if (net is Server)
                {
                    actor.ToggleJob(jobDef);
                    net.EventOccured((int)Components.Message.Types.JobUpdated, actor, jobDef);
                }
                net.BeginPacket(pToggle).Write(player.ID, actor.RefId, jobDef.Name);
            }
            private static void HandleLaborToggle(NetEndpoint net, Packet pck)
            {
                var r = pck.PacketReader;
                var player = net.GetPlayer(r.ReadInt32());
                var actor = net.World.GetEntity(r.ReadInt32()) as Actor;
                var jobDef = Def.GetDef<JobDef>(r.ReadString());
                if (net is Client)
                {
                    actor.ToggleJob(jobDef);
                    net.EventOccured((int)Components.Message.Types.JobUpdated, actor, jobDef);
                }
                else
                    SendLaborToggle(player, actor, jobDef);
            }
            public static void SyncJob(PlayerData player, Actor actor, Job job)
            {
                var net = actor.Net as Server;
                var w = net.GetOutgoingStreamOrderedReliable();
                w.Write(pSync, player.ID, actor.RefId);
                w.Write(job.Def.Name);
                job.Write(w);
            }
            private static void HandleJobSync(NetEndpoint net, Packet pck)
            {
                var r = pck.PacketReader;
                var client = net as Client;
                var player = client.GetPlayer(r.ReadInt32());
                var actor = client.World.GetEntity(r.ReadInt32()) as Actor;
                var jobDef = Def.GetDef<JobDef>(r.ReadString());
                var job = actor.GetJob(jobDef);
                job.Read(r);
                net.EventOccured((int)Components.Message.Types.JobUpdated, actor, jobDef);
            }
        }
        readonly Lazy<Control> UILabors;

        public override string Name
        {
            get { return "Labors"; }
        }
        static JobsManager()
        {
            Packets.Init();
        }
        public JobsManager(Town town)
        {
            this.Town = town;
            this.UILabors = new Lazy<Control>(this.CreateJobsTable);
        }
        
        internal override IEnumerable<Tuple<Func<string>, Action>> OnQuickMenuCreated()
        {
            yield return new Tuple<Func<string>, Action>(()=>"Labors", this.ToggleLaborsWindow);
        }

        public void ToggleLaborsWindow()
        {
            var window = this.UILabors.Value.GetWindow() ?? new Window("Jobs", this.UILabors.Value);
            window.Toggle();
        }

        internal override void OnTargetSelected(IUISelection info, ISelectable target)
        {
            base.OnTargetSelected(info, target);
        }
        internal override void OnTargetSelected(SelectionManager info, ISelectable target)
        {
            base.OnTargetSelected(info, target);
        }
        Control CreateJobsTable()
        {
            var box = new GroupBox();
            var tableBox = new GroupBox();
            var tableAuto = new TableScrollableCompact<Actor>(true)
                            .AddColumn(null, "Name", 100, o => new Label(o.Name, () => { }));
            var tableManual = new TableScrollableCompact<Actor>(true)
                           .AddColumn(null, "Name", 100, o => new Label(o.Name, () => { }));
            var player = this.Player;
            foreach (var labor in JobDefOf.All)
            {
                var ic = labor.Icon;

                var icon = new PictureBox(ic.SpriteSheet, ic.SourceRect) { HoverText = labor.Name };
                var iconManual = new PictureBox(ic.SpriteSheet, ic.SourceRect) { HoverText = labor.Name };

                tableAuto.AddColumn(labor, icon, CheckBoxNew.DefaultBounds.Width, (actor) =>
                {
                    var state = AIState.GetState(actor);
                    var job = state.GetJob(labor);
                    var ch =  new CheckBoxNew
                    {
                        TickedFunc = () => job.Enabled,
                        LeftClickAction = () => Packets.SendLaborToggle(player, actor, labor),
                        HoverText = job.Def.Label
                    };
                    return ch;
                }, 0);
                tableManual.AddColumn(labor, iconManual, CheckBoxNew.DefaultBounds.Width, (actor) =>
                {
                    var state = AIState.GetState(actor);
                    var job = state.GetJob(labor);
                    var btn = new Button(CheckBoxNew.CheckedRegion.Width)
                    {
                        TextFunc = () => { var val = job.Priority; return job.Enabled ? val.ToString() : ""; },
                        LeftClickAction = () => Packets.SendPriorityModify(player, actor, job, job.Priority + 1), 
                        RightClickAction = () => Packets.SendPriorityModify(player, actor, job, job.Priority - 1),
                        HoverText = job.Def.Label
                    };
                    return btn;
                }, 0);
            }
            var net = this.Town.Net;
            var actors = this.Town.Members.Select(id => net.World.GetEntity(id) as Actor);
            tableAuto.AddItems(actors);
            tableManual.AddItems(actors);

            var currentTable = tableAuto;

            tableBox.AddControls(tableAuto);
            var btnTogglePriorities = new CheckBoxNew("Manual priorities") { TickedFunc = () => currentTable == tableManual, LeftClickAction = switchTables };
            box.AddControlsVertically(
                btnTogglePriorities,
                tableBox);

            box.ListenTo(Components.Message.Types.JobUpdated, args =>
            {
                var a = args[0] as Actor;
                var j = args[1] as JobDef;
                tableAuto.GetItem(a, j).Validate();
                tableManual.GetItem(a, j).Validate();
            });

            return box;

            void switchTables()
            {
                tableBox.ClearControls();
                currentTable = currentTable == tableManual ? tableAuto : tableManual;
                tableBox.AddControls(currentTable);
            }
        }
    }
}

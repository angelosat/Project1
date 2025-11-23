using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;
using Start_a_Town_;
using Start_a_Town_.AI;
using System.Runtime.CompilerServices;

namespace Start_a_Town_
{
    public abstract class Workplace : ISerializable, ISaveable
    {
        public IntVec3? Counter;

        public int ID;

        public string Name;

        bool Active;

        readonly public HashSet<int> Rooms = new();

        public Town Town;

        readonly protected HashSet<int> StockpilesInput = new();
        readonly protected HashSet<int> StockpilesOutput = new();
        readonly protected ObservableHashSet<int> Workers = new();

        protected Dictionary<int, WorkerProps> WorkerProps = new();

        static GroupBox WorkersUI;

        public Workplace()
        {
            this.Facilities.CollectionChanged += Facilities_CollectionChanged;
        }

        private void Facilities_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(e.NewItems != null)
                foreach (var n in e.NewItems.Cast<IntVec3>())
                    this.FacilitiesTargetsCached.Add(n, new TargetArgs(this.Map, n));
            if(e.OldItems != null)
                foreach (var n in e.OldItems.Cast<IntVec3>())
                    this.FacilitiesTargetsCached.Remove(n);
        }

        public Workplace(TownComponent manager, int id) : this(manager)
        {
            this.ID = id;
        }

        public Workplace(TownComponent manager) : this()
        {
            this.Town = manager.Town;
        }

        static public Control _Gui { get; } = new Gui();

        public string DefaultName => $"{this.GetType().Name}{this.ID}";

        public MapBase Map => this.Town.Map;

        public INetwork Net => this.Town.Net;

        public void OpenGui()
        {
            var guy = _Gui as Gui;
            guy.OpenGui(this);

            //guy.Refresh(this);
            //if (guy.Window == null)
            //{
            //    guy.ToWindow(this.Name);
            //}
            //guy.Window.Show();
        }

        static public (Control control, Action<Workplace> refresh) CreateUI()
        {
            return (_Gui,
                a => a.OpenGui()
                //{
                //    var guy = _Gui as Gui;
                //    guy.Refresh(a);
                //    if (guy.Window == null)
                //    {
                //        guy.ToWindow(a.Name);
                //    }
                //    guy.Window.Show();
                //}
            );
            int listw = 200, listh = 300;
            var box = new ScrollableBoxTest(listw, listh, ScrollModes.Vertical);

            //Workplace selectedShop = null;
            //var tablestockpiles = new TableCompact<Stockpile>()
            //    .AddColumn(null, "sp", 200, st => new CheckBoxNew($"{st.Name}", () => { }, () => selectedShop.Stockpiles.Contains(st.ID)));

            var liststockpiles = new ListBoxNoScroll<Stockpile, Button>(i => new Button(i.Name));
            var listfacilities = new ListBoxNoScroll<TargetArgs, Button>(
                t => new Button(t.Block.Name,
                    () =>
                    {
                        SelectionManager.Select(t);
                        Ingame.Instance.Camera.CenterOn(t.Global);
                    }));


            string nameGetter() => box.Tag is Workplace wp ? $" {wp.Name}" : "";
            var boxstockpiles = liststockpiles.ToPanelLabeled(() => $"Stockpiles{nameGetter()}");
            var boxfacilities = listfacilities.ToPanelLabeled(() => $"Facilities{nameGetter()}");

            var btnCounter = new Button()
            {
                TextFunc = () =>
                {
                    var text = "Counter:";
                    if (box.Tag is Shop shop)
                        text += " " + (shop.Counter.HasValue ? shop.Counter.Value.ToString() : "null");
                    return text;
                }
            };
            var boxtabs = new GroupBox();
            var boxLists = new GroupBox();

            boxtabs.AddControlsLineWrap(new[] {
                new Button("Stockpiles", ()=>selectTab(boxstockpiles)),
                new Button("Facilities", ()=>selectTab(boxfacilities)) },
                listw);

            void selectTab(Control tab)
            {
                boxLists.ClearControls();
                boxLists.AddControls(tab);
                tab.Validate(true);
            }

            boxLists.AddControlsHorizontally(boxstockpiles);

            box.AddControlsVertically(boxtabs, boxLists);

            void refresh(Workplace shop)
            {
                if (box.Tag?.GetType() != shop.GetType())
                    boxLists.ClearControls();

                liststockpiles.Clear().AddItems(shop.StockpilesInput.Select(i => shop.Town.ZoneManager.GetZone<Stockpile>(i)));
                listfacilities.Clear().AddItems(shop.GetFacilities().Select(f => new TargetArgs(shop.Town.Map, f)));

                boxtabs.ClearControls();
                boxtabs.AddControlsLineWrap(new[] {
                new Button("Stockpiles", ()=>selectTab(boxstockpiles)),
                new Button("Facilities", ()=>selectTab(boxfacilities)) },
                listw);
                boxtabs.AddControlsLineWrap(shop.GetUIBase().Select(b =>
                {
                    return new Button(b.Name, () =>
                    {
                        b.GetData(shop);
                        selectTab(b);
                    });
                }), listw);

                box.ClearControls();

                box.AddControlsVertically(boxtabs, boxLists);
                box.Validate(true);
                box.Tag = shop;
            }
            liststockpiles.OnGameEventAction = e =>
            {
                var shop = box.Tag as Shop;
                switch (e.Type)
                {
                    case Components.Message.Types.ShopUpdated:
                        if (e.Parameters[0] != shop)
                            break;
                        if (e.Parameters[1] is Stockpile[] p)
                        {
                            for (int i = 0; i < p.Length; i++)
                            {
                                var st = p[i];
                                if (shop.StockpilesInput.Contains(st.ID))
                                    liststockpiles.AddItems(st);
                                else
                                    liststockpiles.RemoveItems(st);
                            }
                        }
                        break;
                    default:
                        break;
                }
            };

            return (box, refresh);
        }

        public bool ActorHasJob(Actor a, JobDef def)
        {
            if (!this.WorkerProps.TryGetValue(a.RefID, out var wprops))
                return false;
            return wprops.Jobs[def].Enabled;
        }

        public readonly ObservableHashSet<IntVec3> Facilities = new();
        public readonly ObservableDictionary<IntVec3, TargetArgs> FacilitiesTargetsCached = new();
        public virtual IEnumerable<IntVec3> GetFacilities() { yield break; }

        public virtual CraftOrder GetOrder(int orderID)
        {
            return null;
        }

        public virtual IEnumerable<JobDef> GetRoleDefs() { yield break; }

        public IEnumerable<Room> GetRooms()
        {
            var manager = this.Town.RoomManager;
            return this.Rooms.Select(manager.GetRoom);
        }

        public virtual AITask GetTask(Actor actor)
        {
            foreach (var role in this.GetWorkerProps(actor).Jobs.Values.Where(j => j.Enabled))
                foreach (var taskGiver in role.Def.GetTaskGivers())
                    if (taskGiver.FindTask(actor) is TaskGiverResult result)
                        return result.Task;
            return null;
        }

        public Job GetWorkerJob(Actor a, JobDef j)
        {
            return this.GetWorkerProps(a).GetJob(j);
        }

        public WorkerProps GetWorkerProps(Actor a)
        {
            var aID = a.RefID;
            return this.WorkerProps[aID];
        }

        public bool HasStockpile(int stockpileID)
        {
            return this.StockpilesInput.Contains(stockpileID);
        }

        public bool HasWorker(Actor actor)
        {
            return this.Workers.Contains(actor.RefID);
        }

        public virtual bool IsAllowed(Block block) { return false; }

        public virtual bool IsValid() { return true; }

        public virtual bool IsValidRoom(Room room) { return false; }

        public ISaveable Load(SaveTag tag)
        {
            tag.TryGetTagValue("ID", out this.ID);
            if (!tag.TryGetTagValue("Name", out this.Name))
                this.Name = this.DefaultName;
            this.StockpilesInput.Load(tag, "Stockpiles");
            this.Facilities.Load(tag, "Facilities");
            this.Workers.Load(tag, "Workers");
            this.Rooms.TryLoad(tag, "Rooms");
            if (!tag.TryGetTag("WorkerProps", v => this.WorkerProps = v.LoadList<WorkerProps>().ToDictionary(i => i.ActorID, i => i)))
                this.InitWorkerProps();
            tag.TryGetTagValue<Vector3>("Counter", v => this.Counter = v);
            this.LoadExtra(tag);
            return this;
        }

        public ISerializable Read(BinaryReader r)
        {
            this.ID = r.ReadInt32();
            this.Name = r.ReadString();
            this.StockpilesInput.Read(r);
            this.Facilities.Read(r);
            this.Workers.Read(r);
            this.Rooms.Read(r);
            this.Counter = r.ReadVector3Nullable();
            this.WorkerProps = r.ReadList<WorkerProps>().ToDictionary(w => w.ActorID, w => w);
            this.ReadExtra(r);
            return this;
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(this.ID.Save("ID"));
            this.Name.Save(tag, "Name");
            tag.Add(this.StockpilesInput.Save("Stockpiles"));
            tag.Add(this.Facilities.Save("Facilities"));
            tag.Add(this.Workers.Save("Workers"));
            tag.Add(this.Rooms.Save("Rooms"));
            this.WorkerProps.Values.SaveNewBEST(tag, "WorkerProps");

            if (this.Counter.HasValue)
                tag.Add(this.Counter.Value.Save("Counter"));
            this.SaveExtra(tag);
            return tag;
        }

        public virtual void Tick() { }

        public void Write(BinaryWriter w)
        {
            w.Write(this.ID);
            w.Write(this.Name);
            w.Write(this.StockpilesInput);
            w.Write(this.Facilities);
            w.Write(this.Workers);
            w.Write(this.Rooms);
            w.Write(this.Counter);
            this.WorkerProps.Values.Write(w);
            this.WriteExtra(w);
        }

        internal virtual void AddFacility(IntVec3 global)
        {

        }

        internal void AddRoom(Room room)
        {
            this.Rooms.Add(room.ID);
        }

        internal void AddWorker(Actor actor)
        {
            if (this.HasWorker(actor))
            {
                this.RemoveWorker(actor);
                return;
            }
            this.Town.ShopManager.GetShop<Shop>(actor)?.RemoveWorker(actor);
            this.Workers.Add(actor.RefID);
            this.WorkerProps.Add(actor.RefID, new WorkerProps(actor, this.GetRoleDefs().ToArray()));
            this.Town.Net.EventOccured(Components.Message.Types.ShopUpdated, this, new[] { actor });
        }

        internal IEnumerable<Actor> GetWorkers()
        {
            foreach (var actor in this.Workers)
                yield return this.Town.Net.GetNetworkObject(actor) as Actor;
        }

        internal virtual void OnBlocksChanged(IEnumerable<IntVec3> positions) { }

        internal void RemoveRoom(Room room)
        {
            this.Rooms.Remove(room.ID);
        }

        internal void RemoveWorker(Actor actor)
        {
            this.Workers.Remove(actor.RefID);
            this.WorkerProps.Remove(actor.RefID);
            this.Town.Net.EventOccured(Components.Message.Types.ShopUpdated, this, new[] { actor });
        }

        internal void ResolveReferences()
        {
            if (this.Counter.HasValue)
                if (this.Town.Map.GetBlock(this.Counter.Value) is not BlockShopCounter)
                    this.Counter = null;
            this.Rooms.RemoveWhere(rID => !this.Town.RoomManager.TryGetRoom(rID, out _));
            this.ResolveExtraReferences();
        }

        internal void RoomChanged(Room room)
        {
            if (!this.IsValidRoom(room))
            {
                if (room.Workplace != this)
                    throw new Exception();
                room.SetWorkplace(null);
            }
        }
        internal string Rename(string name)
        {
            var oldName = this.Name;
            this.Name = name;
            return oldName;
        }
        protected virtual IEnumerable<GroupBox> GetUI() { yield break; }

        protected virtual void LoadExtra(SaveTag tag) { }

        protected virtual void ReadExtra(BinaryReader r) { }

        protected virtual void ResolveExtraReferences() { }

        protected virtual void SaveExtra(SaveTag tag) { }

        protected virtual void WriteExtra(BinaryWriter w) { }

        [Obsolete]
        static Control CreateWorkersUI(Workplace shop)
        {
            var town = shop.Town;
            var manager = town.ShopManager;
            var box = new ScrollableBoxTest(200, UIManager.LargeButton.Height * 7, ScrollModes.Vertical);
            var listworkers = new ListBoxNoScroll<Actor, ButtonNew>(
                 a =>
                 a.GetButton(box.Client.Width,
                 () => a.Workplace != null ? $"Assigned to {a.Workplace.Name}" : "",
                 () => manager.ToggleWorker(a, shop)));
            listworkers.AddItems(town.GetAgents());
            return listworkers;
        }
        [Obsolete]
        static GroupBox GetWorkersUI()
        {
            var box = new GroupBox() { Name = "Workers" };
            Workplace tav = null;
            var table = new TableScrollableCompact<Actor>(true);

            var btnworkers = new Button("Assign Workers", () =>
            {
                CreateWorkersUI(tav).ToContextMenu("Assign workers").SnapToMouse().Toggle();
            });
            var boxContainer = new GroupBox();
            boxContainer.AddControlsVertically(btnworkers, table);

            var tablePanel = boxContainer.ToPanelLabeled("Workers");
            table.OnGameEventAction = e =>
            {
                switch (e.Type)
                {
                    case Components.Message.Types.ShopUpdated:
                        if (e.Parameters[0] != tav)
                            break;
                        if (e.Parameters[1] is Actor[] actors)
                        {
                            for (int i = 0; i < actors.Length; i++)
                            {
                                var actor = actors[i];
                                if (tav.Workers.Contains(actor.RefID))
                                    table.AddItems(actor);
                                else
                                    table.RemoveItems(actor);
                            }
                        }
                        break;

                    default:
                        break;
                }
            };
            box.SetGetDataAction(o =>
            {
                tav = o as Workplace;
                table.Clear();
                table.AddColumn(new(), "", 128, a => new Label(a.Name), 0);
                foreach (var role in tav.GetRoleDefs())
                {
                    table.AddColumn(role, role.Label, 32, a =>
                    {
                        var j = tav.GetWorkerJob(a, role);

                        return new CheckBoxNew()
                        {
                            TickedFunc = () => j.Enabled,
                            LeftClickAction = () => Packets.UpdateWorkerRoles(tav.Net, tav.Net.GetPlayer(), tav, role, a)
                        };
                    });
                }
                table.AddItems(tav.Workers.Select(tav.Net.GetNetworkObject).Cast<Actor>());
            });

            box.AddControlsVertically(
                tablePanel);
            return box;
        }

        IEnumerable<GroupBox> GetUIBase()
        {
            yield return WorkersUI ??= GetWorkersUI();
            foreach (var b in this.GetUI())
                yield return b;
        }

        void InitWorkerProps()
        {
            this.WorkerProps.Clear();
            foreach (var worker in this.Workers)
                this.WorkerProps.Add(worker, new WorkerProps(worker, this.GetRoleDefs().ToArray()));
        }

        private void ToggleJob(Actor actor, JobDef role)
        {
            this.GetWorkerProps(actor).GetJob(role).Toggle();
        }

        Control CreateJobsTable()
        {
            var box = new GroupBox();
            var tableBox = new GroupBox();
            var tableAuto = new TableScrollableCompact<Actor>(true)
                            .AddColumn(null, "Name", 100, o => new Label(o.Name, () => { }));
            var tableManual = new TableScrollableCompact<Actor>(true)
                           .AddColumn(null, "Name", 100, o => new Label(o.Name, () => { }));

            void refreshJobsTable(Workplace wp)
            {
                var roles = wp.GetRoleDefs();
                foreach (var labor in roles)
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
                            TickedFunc = () => job.Enabled,
                            //LeftClickAction = () => Packets.SendLaborToggle(player, actor, labor),
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
                            //LeftClickAction = () => Packets.SendPriorityModify(player, actor, job, job.Priority + 1),
                            //RightClickAction = () => Packets.SendPriorityModify(player, actor, job, job.Priority - 1),
                            HoverText = job.Def.Label
                        };
                        return btn;
                    }, 0);
                }
            }
            var net = this.Town.Net;
            var actors = this.Town.Townies.Select(id => net.GetNetworkObject(id) as Actor);
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

        class Gui : GroupBox
        {
            Workplace SelectedShop;
            TableCompact<Stockpile> TableStockpiles;
            TableCompact<Stockpile> TableShoppingDisplays;
            TableCompact<TargetArgs> TableFacilities;
            TableCompact<int, Actor> TableJobRoles;
            ListBoxObservable<int, Actor, ButtonNew> ListAvailableWorkers;
            CheckBoxTest ChkBoxEnabled;
            Label LabelName;
            Control WorkersGui;

            public void OpenGui(Workplace shop)
            {
                var guy = _Gui as Gui;
                guy.Refresh(shop);
                if (guy.Window == null)
                    guy.ToWindow(()=>this.SelectedShop.Name);
                guy.Window.Show();
            }

            public Gui()
            {
                //this.LabelName = new Label(() => this.SelectedShop?.Name ?? "", () => { new DialogInput("Enter shop name", (string newName) => { }, 16, this.SelectedShop.Name); });
                this.LabelName = new Label(() => this.SelectedShop?.Name ?? "", () => { DialogInput.ShowInputDialog("Enter new name", nn => Packets.SendPlayerRenameShop(this.SelectedShop.Net, this.SelectedShop.Net.GetPlayer().ID, this.SelectedShop.ID, nn), 16, this.SelectedShop.Name); });
                this.ChkBoxEnabled = new("Open", () => this.SelectedShop?.Active ?? false, () => Packets.SendPlayerToggleShop(this.SelectedShop.Net, this.SelectedShop.Net.GetPlayer().ID, this.SelectedShop.ID));
                int listw = 200, listh = 300;
                //var box = new ScrollableBoxTest(listw, listh, ScrollModes.Vertical);
                this.TableStockpiles = new TableCompact<Stockpile>() { Name = "Stockpiles" }
                    .AddColumn(null, "name", 200, st => new CheckBoxTest(() => st.Name, () => this.SelectedShop.StockpilesInput.Contains(st.ID), () => TickStockpile(st)));

                this.TableShoppingDisplays = new TableCompact<Stockpile>() { Name = "Shopping" }
                    .AddColumn(null, "name", 200, st => new CheckBoxTest(() => st.Name, () => this.SelectedShop.StockpilesOutput.Contains(st.ID), () => TickShoppingArea(st)));

                this.TableFacilities = new TableCompact<TargetArgs>() { Name = "Facilities" }
                    .AddColumn(null, "name", 200 - Icon.Cross.Width, st => new Label(() => st.Block.Name))
                    .AddColumn(null, "delete", Icon.Cross.Width, st => IconButton.CreateSmall(Icon.Cross, () => WorkplaceManager.Packets.SendPlayerShopAssignCounter(st.Map.Net, st.Map.Net.GetPlayer(), this.SelectedShop, st.Global), "remove"));

                var workersTab = new GroupBox() { Name = "Workers" };
                this.TableJobRoles = new TableCompact<int, Actor>(i => this.SelectedShop.Net.GetNetworkObject<Actor>(i), true) { Name = "Workers" }
                .AddColumn(null, "Worker".ToLabel(), 90, a => new Label(a.Name));
                this.ListAvailableWorkers = new ListBoxObservable<int, Actor, ButtonNew>(
                         a =>
                         a.GetButton(200,//boxlistworkers.Client.Width,
                         () => a.Workplace != null ? $"Assigned to {a.Workplace.Name}" : "",
                         () => a.Town.ShopManager.ToggleWorker(a, this.SelectedShop)));
                var listWorkersContext = this.ListAvailableWorkers.ToContextMenu("Assign workers");

                workersTab.AddControlsVertically(
                    new Button("Assign workers", () => listWorkersContext.SnapToMouse().Show(), this.TableJobRoles.Width),
                    this.TableJobRoles);
                this.WorkersGui = GetWorkersUI();

                //var tabs = new PanelWithTabs(200, 300, new Control[] { this.TableStockpiles, this.TableShoppingDisplays, this.TableFacilities, this.WorkersGui });
                var tabs = new PanelWithVerticalTabs(200, 300).InitTabs(new Control[] { 
                    new GroupBox(){Name="General" }.AddControlsVertically(this.LabelName, this.ChkBoxEnabled),
                    this.TableStockpiles, 
                    this.TableShoppingDisplays,
                    this.TableFacilities,
                    workersTab });// this.TableJobRoles });

                //box.AddControls(this.TableStockpiles);
                //this.AddControls(box);
                this.AddControls(tabs);

            }
            public void Refresh(Workplace shop)
            {
                var prevShop = this.SelectedShop;
                this.SelectedShop = shop;
                if (prevShop == null || prevShop.Town != shop.Town)
                {
                    this.TableStockpiles.Bind(shop.Town.ZoneManager.Zones);
                    this.TableShoppingDisplays.Bind(shop.Town.ZoneManager.Zones);
                    this.TableFacilities.Bind(shop.FacilitiesTargetsCached);
                    this.TableJobRoles.ClearColumns();
                    this.TableJobRoles.AddColumn(null, "Worker", 90, a => new Label(a.Name));
                    foreach (var role in shop.GetRoleDefs())
                        this.TableJobRoles.AddColumn(null, role.Label, 32, a => new CheckBoxNew());
                    //this.TableJobRoles.Bind(shop.Town.Townies);
                    this.TableJobRoles.Bind(shop.Workers);
                    this.ListAvailableWorkers.Bind(shop.Town.Townies, shop.Net.GetNetworkObject<Actor>);
                }
                this.Window?.SetTitle(shop.Name);
            }
            void TickStockpile(Stockpile st)
            {
                var net = st.Net;
                WorkplaceManager.Packets.SendPlayerAddStockpileToShop(net, net.GetPlayer().ID, this.SelectedShop.ID, st.ID);
            }
            void TickShoppingArea(Stockpile st)
            {
                var net = st.Net;
                WorkplaceManager.Packets.SendPlayerAddShoppingArea(net, net.GetPlayer().ID, this.SelectedShop.ID, st.ID);
            }
        }

        [EnsureStaticCtorCall]
        class Packets
        {
            static int PacketUpdateWorkerRoles, PacketPlayerRenameShop, PacketPlayerToggleShop;
            static Packets()
            {
                PacketUpdateWorkerRoles = Network.RegisterPacketHandler(UpdateWorkerRoles);
                PacketPlayerRenameShop = Network.RegisterPacketHandler(ReceivePlayerRenameShop);
                PacketPlayerToggleShop = Network.RegisterPacketHandler(ReceivePlayerToggleShop);
            }
           
            public static void UpdateWorkerRoles(INetwork net, PlayerData player, Workplace tavern, JobDef role, Actor actor)
            {
                if (net is Server)
                    tavern.ToggleJob(actor, role);
                var w = net.GetOutgoingStream();
                w.Write(PacketUpdateWorkerRoles, player.ID, tavern.ID, role.Name, actor.RefID);
            }

            static void UpdateWorkerRoles(INetwork net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var tavern = net.Map.Town.GetShop(r.ReadInt32());
                var role = Def.GetDef<JobDef>(r.ReadString());
                var actor = net.GetNetworkObject<Actor>(r.ReadInt32());
                if (net is Client)
                    tavern.ToggleJob(actor, role);
                else
                    UpdateWorkerRoles(net, player, tavern, role, actor);
            }

            static public void SendPlayerRenameShop(INetwork net, int playerID, int shopID, string name)
            {
                if (shopID < 0)
                    return;
                var w = net.GetOutgoingStream();
                w.Write(PacketPlayerRenameShop);
                w.Write(playerID);
                w.Write(shopID);
                w.Write(name);
            }
            private static void ReceivePlayerRenameShop(INetwork net, BinaryReader r)
            {
                var playerID = r.ReadInt32();
                var shopid = r.ReadInt32();
                var shopmanager = net.Map.Town.ShopManager;
                var shop = shopmanager.GetShop(shopid);
                var name = r.ReadString();
                var oldName = shop.Rename(name);
                DebugConsole.Write($"Player {net.GetPlayer(playerID).Name} renamed shop {oldName} to {shop.Name}");

                if (net is Server)
                    SendPlayerRenameShop(net, playerID, shopid, name);
            }

            static public void SendPlayerToggleShop(INetwork net, int playerID, int shopID)
            {
                if (shopID < 0)
                    return;
                var w = net.GetOutgoingStream();
                w.Write(PacketPlayerToggleShop);
                w.Write(playerID);
                w.Write(shopID);
            }
            private static void ReceivePlayerToggleShop(INetwork net, BinaryReader r)
            {
                var playerID = r.ReadInt32();
                var shopid = r.ReadInt32();
                var shopmanager = net.Map.Town.ShopManager;
                var shop = shopmanager.GetShop(shopid);
                shop.ToggleOpen();
                DebugConsole.Write($"Player {net.GetPlayer(playerID).Name} toggled shop {shop.Name} open");  
                if (net is Server)
                    SendPlayerToggleShop(net, playerID, shopid);
            }
        }

        private void ToggleOpen()
        {
            this.Active = !this.Active;
        }
    }
}

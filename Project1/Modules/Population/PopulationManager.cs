using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Start_a_Town_
{
    public class PopulationManager : Inspectable, ISaveable, ISerializable
    {
        internal static class Packets
        {
            static int PacketVisitorArrived, PacketAdventurerCreated;
            public static void Init()
            {
                PacketVisitorArrived = Registry.PacketHandlers.Register(ReceiveNotifyVisit);
                PacketAdventurerCreated = Registry.PacketHandlers.Register(ReceiveNotifyAdventurerCreated);
            }

            public static void SendNotifyVisit(Actor actor)
            {
                var w = Server.Instance.OutgoingStreamTimestamped;
                w.Write(PacketVisitorArrived, actor.RefId);
            }
            public static void SendNotifyAdventurerCreated(Actor actor)
            {
                var w = Server.Instance.OutgoingStreamTimestamped;
                w.Write(PacketAdventurerCreated, actor.RefId);
            }
            private static void ReceiveNotifyAdventurerCreated(NetEndpoint net, Packet pck)
            {
                var r = pck.PacketReader;
                var client = net as Client;
                var actorID = r.ReadInt32();
                var actor = client.World.GetEntity(actorID) as Actor;
                var world = client.Map.World as StaticWorld;
                world.Population.RegisterVisitor(actor);
            }
            private static void ReceiveNotifyVisit(NetEndpoint net, Packet pck)
            {
                var r = pck.PacketReader;
                if (net is Server)
                    throw new Exception();
                var actorID = r.ReadInt32();
                var actor = net.World.GetEntity(actorID) as Actor;
                ReportVisit(net, actor);
            }

            private static void ReportVisit(INetEndpoint net, Actor actor)
            {
                var props = actor.GetVisitorProperties();
                net.Report($"{actor.Name} is {(!actor.Exists ? ("visiting" + (props.Discovered ? "" : " for the first time!")) : "departing")}");
                props.Discovered = true;
            }
        }
        internal static void Init()
        {
            Packets.Init();
            OffsiteAreaDefOf.Init();
            VisitorNeedsDefOf.Init();
        }

        bool Populated;
        readonly ObservableCollection<VisitorProperties> ActorsAdventuring = new();// ActorsCap);
        public IEnumerable<VisitorProperties> AllActors => this.ActorsAdventuring;
        public readonly StaticWorld World;
        const int WorldPopulationCap = 8;
        const float TickRate = 1 / 3f, InitialChance = .05f, VisitChanceBaseRate = .001f;// 2 seconds per tick //1 tick per second 
        const int InitialApproval = 50;

        int TickCount = (int)(Ticks.PerSecond / TickRate);
        public PopulationManager(StaticWorld world)
        {
            this.World = world;
            world.Events.ListenTo<EntityDisposedEvent>(OnEntityDisposed);
        }

        private void OnEntityDisposed(EntityDisposedEvent e)
        {
            var existing = this.ActorsAdventuring.FirstOrDefault(p => p.Actor == e.Entity);
            if (existing != null)
                this.ActorsAdventuring.Remove(existing);
        }

        public void Update(INetEndpoint net)
        {
            if (net is Server)
                this.HandleErrors();
            foreach (var v in this.ActorsAdventuring)
                v.Tick();
            this.TickCount--;
            if (this.TickCount > 0)
                return;
            this.TickCount = (int)(Ticks.PerSecond / TickRate);
            this.Tick(net);
        }

        private void HandleErrors()
        {
            var map = this.World.Map;
            var net = map.Net;
            var allActors = net.World.GetEntities().OfType<Actor>();// GetNetworkObjects().OfType<Actor>();
            var citizens = map.Town.GetMembers();
            foreach (var actor in allActors)
            {
                if (citizens.Contains(actor))
                    continue;
                if (!this.ActorsAdventuring.Any(v => v.Actor == actor))
                {
                    this.Populated = true;
                    Packets.SendNotifyAdventurerCreated(actor);
                    this.RegisterVisitor(actor);
                    Log.WriteToFile($"{actor.Name} is not a town member and was missing from the world population list.");
                }
            }
        }

        internal void Initialize()
        {
            this.InitializeInhabitants();
        }

        void InitializeInhabitants()
        {
            for (int i = 0; i < WorldPopulationCap; i++)
                this.RegisterVisitor(GenerateInhabitant());
        }

        void Tick(INetEndpoint net)
        {
            this.PopulateNew(net);
        }

        private Actor PopulateNew(INetEndpoint net)
        {
            if (net is Server && this.ActorsAdventuring.Count < WorldPopulationCap)
            {
                Actor actor = GenerateInhabitant();
                actor.SyncInstantiate(Server.Instance);
                Packets.SendNotifyAdventurerCreated(actor);
                //this.RegisterActor(Server.Instance, actor);
                this.RegisterVisitor(actor);
                AnnounceInhabitantCreated(net, actor);
                return actor;
            }
            return null;
        }

        private static Actor GenerateInhabitant()
        {
            var visitor = ActorDefOf.Npc.Create() as Actor;
            visitor.Inventory.Insert(ItemDefOf.Coins.Create().SetStackSize(500));
            return visitor;
        }

        private void RegisterVisitor(Actor actor)
        {
            var props = new VisitorProperties(this.World, actor, InitialChance, InitialApproval) { OffsiteArea = OffsiteAreaDefOf.Forest };
            this.ActorsAdventuring.Add(props);
            MakeVisitor(actor);
        }

        private static void AnnounceInhabitantCreated(INetEndpoint net, Actor actor)
        {
            net.Write($"{actor.Name} created");
            //net.EventOccured((int)Components.Message.Types.NewAdventurerCreated, actor);
        }

        private static void MakeVisitor(Actor actor)
        {
            actor.AddNeed(VisitorNeedsDefOf.All.ToArray());
            actor.ModifyNeed(VisitorNeedsDefOf.Guidance, n => 10);
        }

        public IEnumerable<VisitorProperties> Find(Func<VisitorProperties, bool> pred)
        {
            foreach (var v in this.ActorsAdventuring.Where(pred))
                yield return v;
        }

        internal IEnumerable<VisitorProperties> GetVisitorProperties()
        {
            foreach (var v in this.ActorsAdventuring)
                yield return v;
        }
        internal VisitorProperties GetVisitorProperties(Actor actor)
        {
            return this.ActorsAdventuring.FirstOrDefault(v => v.Actor == actor);
        }
        internal void OnTargetSelected(IUISelection info, ISelectable selected)
        {
        }
        internal void OnTargetSelected(SelectionManager info, ISelectable selected)
        {
        }

        Control _gui;
        public Control Gui => this._gui ??= this.CreateGui();
        Control CreateGui()
        {
            //var box = new ScrollableBoxNewNew(200, UIManager.LargeButton.Height * 8);
            var box = new ScrollableBoxTest(200, UIManager.LargeButton.Height * 8);
            var list = new ListBoxObservable<VisitorProperties, ButtonNew>(props =>
            {
                var npc = props.Actor;
                var btn = ButtonNew.CreateBig(() => SelectionManager.Select(npc), box.Viewport.Width, npc.RenderIcon(), () => npc.Npc.FullName, () => npc.Exists ? "Visiting" : (props.Discovered ? "" : "Unknown"));

                // debugging stuff
                btn.RightClickActionNew = b =>
                {
                    if (!InputState.IsKeyDown(System.Windows.Forms.Keys.LShiftKey))
                        return;
                    ContextMenuManager.PopUp(
                        ("Force visit", () => Server.Instance.World.GetEntity<Actor>(npc.RefId).GetVisitorProperties().ForceVisit()),
                        ("Dispose", () => PacketEntityDispose.Send(Client.Instance, npc.RefId, Client.Instance.PlayerData))
                    );
                };
                return btn;
            });

            Func<VisitorProperties, bool>
                filterUndiscovered = i => !i.Discovered,
                filterVisiting = i => i.Actor.Exists,
                filterAway = i => !i.Actor.Exists && i.Discovered;

            var filters = list.CreateFilters(("All", null), ("Visiting", filterVisiting), ("Away", filterAway), ("Unknown", filterUndiscovered));

            list.Bind(this.ActorsAdventuring);
            box.AddControlsVertically(filters, list);
            return box;
        }

        public void ResolveReferences()
        {
            foreach (var props in this.ActorsAdventuring) // i added this to add visitor needs to existing visitors because I wasn't saving them in the needscomponent class
            {
                var actor = props.Actor;
                // TODO move this somewhere else
                if (this.World.Map.Net is Server)
                    if (!actor.GetNeeds(VisitorNeedsDefOf.NeedCategoryVisitor).Any())
                        MakeVisitor(actor);
                if (!actor.IsSpawned) // hacky. in process of finding best way to save unspawned actors
                    this.World.Map.Net.Instantiate(actor);
                if (actor.IsSpawned)
                    props.Discovered = true; // HACK
                props.OffsiteArea = OffsiteAreaDefOf.Forest; // HACK
                actor.ResolveReferences();
            }
        }
        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.Populated.Save(tag, "Populated");
            this.TickCount.Save(tag, "Tick");
            this.ActorsAdventuring.SaveNewBEST(tag, "Population");
            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            this.Populated.TryLoad(tag, "Populated");
            this.TickCount.TryLoad(tag, "Tick");
            this.ActorsAdventuring.TryLoad(tag, "Population", this);
            return this;
        }
        public void Write(BinaryWriter w)
        {
            this.ActorsAdventuring.Write(w);
        }

        public ISerializable Read(IDataReader r)
        {
            this.ActorsAdventuring.Initialize(r, this);
            return this;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public class VisitorProperties : Inspectable, ITooltippable, ISerializable, ISaveable, ISyncable
    {
        static readonly int PacketSyncAwardTownRating, PacketSync;
        static VisitorProperties()
        {
            PacketSyncAwardTownRating = Network.RegisterPacketHandler(SyncAwardTownRating);
            PacketSync = Network.RegisterPacketHandler(Sync);
        }

        public int ActorID;
        Actor CachedActor;
        public Actor Actor
        {
            get
            {
                return this.CachedActor ??= this.World.Map.Net.GetNetworkEntity(this.ActorID) as Actor;
            }
            set
            {
                this.CachedActor = value;
                this.ActorID = value.RefID;
            }
        }


        public bool Discovered;
        public void ResolveReferences()
        {
            this.CachedActor = this.World.Map.Net.GetNetworkEntity(this.ActorID) as Actor;
        }
        
        //public float TownApprovalRating;
        //int ApprovalMin = -100, ApprovalMax = 100;
        public TownApproval TownApprovalRating = new ();
        public HashSet<int> JunkItems = new();
        public IntVec3? HangAroundSpot;

        //public float TownRating => this.TownApprovalRating >= 0 ? this.TownApprovalRating / ApprovalMax : this.TownApprovalRating / ApprovalMin;
        public TimeSpan Timer = new();

        public OffsiteAreaDef OffsiteArea;
        static readonly int OffsiteTickLength = Ticks.PerSecond * 10;
        int OffsiteTick;
        float VisitChanceModifier;
        public HashSet<int> ShopBlacklist = new();
        public HashSet<int> RecentlyVisitedShops = new();
        public readonly ObservableCollection<QuestDef> Quests = new();
        public StaticWorld World;
        public VisitorProperties(BinaryReader r, PopulationManager manager)
        {
            this.World = manager.World;
            this.Read(r);
        }
        public VisitorProperties(SaveTag save, PopulationManager manager)
        {
            this.World = manager.World;
            this.Load(save);
        }
        public VisitorProperties(StaticWorld world, Actor actor, float townVisitChance, int townApprovalRating)
        {
            this.World = world;
            this.Timer = world.Clock;
            this.Actor = actor;
            //TownApprovalRating = townApprovalRating;
            this.TownApprovalRating.Value = townApprovalRating;
        }
        public void Tick()
        {
            var actor = this.Actor;
            if (actor.IsSpawned)
                return;
            if (this.OffsiteTick < OffsiteTickLength)
            {
                this.OffsiteTick++;
                return;
            }
            this.OffsiteTick = 0;

            this.TryVisitTown();
            if (actor.Exists) // return if the actor is now spawned as result of the tryvisit function
                return;

            var net = actor.Net;
            if (net is Server)
                this.OffsiteArea?.Tick(this);

            this.TickNeeds();
        }
        private void TickNeeds()
        {
            foreach (var n in this.Actor.GetNeeds())
                n.Tick(this.Actor);
        }
        void TryVisitTown()
        {
            var actor = this.Actor;
            var net = actor.Net;
            if (net is Client)
                return;


            var isVisiting = actor.Exists;
            var map = Net.Server.Instance.Map as StaticMap;
            var world = this.World;
            if (isVisiting)
            {
                throw new Exception(); // this shouldn't have been called if the actor is spawned
            }
            else
            {
                if (world.Random.Chance(this.GetVisitChance()))
                {
                    PopulationManager.Packets.SendNotifyVisit(actor);
                    Vector3 coords = map.GetRandomEdgeCell().Above();
                    map.SyncSpawn(actor, coords, Vector3.Zero);
                    this.Arrive();
                    this.ResetTimer(world.Clock);
                }
            }
            this.Sync();
        }
       
        private static void Sync(INetEndpoint net, BinaryReader r)
        {
            var actor = net.GetNetworkObject<Actor>(r.ReadInt32());
            actor.GetVisitorProperties().Sync(r);
        }

        private void Sync()
        {
            var net = this.Actor.Net;
            if (net is Client)
                throw new Exception();
            net.WriteToStream(PacketSync, this.Actor.RefID);
            this.Sync(net.GetOutgoingStream());
        }

        public void GetTooltipInfo(Control tooltip)
        {
        }

        internal VisitorProperties AddRecentlyVisitedShop(Workplace shop)
        {
            this.RecentlyVisitedShops.Add(shop.ID);
            return this;
        }
        internal bool HasRecentlyVisited(Workplace shop)
        {
            return this.RecentlyVisitedShops.Contains(shop.ID);
        }
        internal void ResetTimer(TimeSpan clock)
        {
            this.Timer = clock;
        }
        internal TimeSpan GetTimer()
        {
            return this.Timer;
        }
        
        internal TimeSpan GetTimeElapsed()
        {
            return this.World.Clock - this.Timer;
        }
        
        public double GetVisitChance()
        {
            if (this.GetQuests().Any(q => q.IsCompleted(this.Actor)))
                return 1;
            var fromTime = this.FromTimeElapsed();
            var fromNeeds = this.GetVisitChanceFromNeeds();
            var fromTownRating = (.5 + this.TownApprovalRating.Rating);
            return fromTime * fromNeeds * fromTownRating + this.VisitChanceModifier;
        }
        public double GetDepartChance()
        {
            if (this.GetQuests().Any(q => !q.IsCompleted(this.Actor)))
                return 1;
            return this.FromTimeElapsed();
        }
       
        double FromTimeElapsed()
        {
            var elapsed = this.GetTimeElapsed();
            var a = elapsed.TotalHours / 24;
            var fromElapsed = a * a;
            return fromElapsed;
        }
        double GetVisitChanceFromNeeds()
        {
            var value = this.Actor.GetNeeds(VisitorNeedsDefOf.NeedCategoryVisitor).Average(n => n.Percentage);
            return 1 - value;
        }
        public void ForceVisit()
        {
            this.VisitChanceModifier = 1;
            var debugmsg = $"{this.Actor.Name}'s visit chance modifier set to 1";
            Server.Instance.ConsoleBox.Write(debugmsg);
            DebugConsole.Write(DebugConsole.Debug, debugmsg);
        }
        internal void BlacklistShop(int shopID)
        {
            this.ShopBlacklist.Add(shopID);
            this.SyncAwardTownRating(-50);
            var shop = this.Actor.Town.ShopManager.GetShop(shopID);
            AILog.SyncWrite(this.Actor, $"Blacklisted {shop.Name} because of bad service");
        }
        internal void BlacklistShop(Workplace shop)
        {
            this.BlacklistShop(shop.ID);
        }

        internal bool IsBlacklisted(Workplace shop)
        {
            return this.ShopBlacklist.Contains(shop.ID);
        }

        internal void Arrive()
        {
            this.VisitChanceModifier = 0;
            this.ShopBlacklist.Clear();
        }
        public void SyncAwardTownRating(float value)
        {
            var net = this.Actor.Net;
            if (net is Client)
                return;
            this.AwardTownRating(value);
            net.WriteToStream(PacketSyncAwardTownRating, this.Actor.RefID, value);
        }
        private static void SyncAwardTownRating(INetEndpoint net, BinaryReader r)
        {
            if (net is Server)
                throw new Exception();
            var props = net.GetNetworkObject<Actor>(r.ReadInt32()).GetVisitorProperties();
            props.AwardTownRating(r.ReadSingle());
        }
        public void AwardTownRating(float value)
        {
            if (value == 0)
                return;
            this.TownApprovalRating.Value += value;
            FloatingText.Create(this.Actor, $"Town rating {value:+;-}", ft => { ft.Font = UIManager.FontBold; ft.TextColor = value > 0 ? Color.Lime : Color.Red; });
        }
        internal bool HasQuest(QuestDef quest)
        {
            return this.Quests.Contains(quest);
        }
        internal bool AcceptQuest(QuestDef quest)
        {
            var actor = this.Actor;
            this.Quests.Add(quest);
            actor.Net.EventOccured(Components.Message.Types.QuestReceived, actor, quest);
            AILog.SyncWrite(actor, $"Received quest [{quest}] from [{quest.Giver.Name}]");
            return true;
        }
        internal void AbandonQuest(QuestDef quest)
        {
            this.Quests.Remove(quest);
            this.Actor.Net.EventOccured(Components.Message.Types.QuestAbandoned, this.Actor, quest);
            this.Actor.Log.Write($"Abandoned quest [{quest.Name}]");
        }
        internal void CompleteQuest(QuestDef quest)
        {
            this.Quests.Remove(quest);
            this.Actor.Log.Write($"Received reward for completing quest [{quest.Name}]");
            this.AwardTownRating(quest.GetRewardRatio());
        }
        internal IEnumerable<QuestDef> GetQuests()
        {
            var manager = this.World.Map.Town.QuestManager;
            foreach(var qid in this.Quests)
            {
                yield return qid;
            }
        }
        public override string ToString()
        {
            return $"Visitor:{this.Actor.Name}";
        }
        public void Write(BinaryWriter w)
        {
            w.Write(this.Actor.RefID);
            w.Write(this.Actor.Exists);
            if (!this.Actor.Exists)
                this.Actor.Write(w);
            w.Write(this.TownApprovalRating.Value);
            w.Write(this.ShopBlacklist);
            w.Write(this.RecentlyVisitedShops);
            w.Write(this.Discovered);
            w.Write(this.Timer.TotalMilliseconds);
            w.Write(this.Quests.Select(q => q.ID).ToArray());
        }
        public ISerializable Read(BinaryReader r)
        {
            this.ActorID = r.ReadInt32();
            var isspawned = r.ReadBoolean();
            if (!isspawned)
                this.Actor = GameObject.Create(r) as Actor;
            this.TownApprovalRating.Value = r.ReadSingle();
            this.ShopBlacklist = new(r.ReadIntArray());
            this.RecentlyVisitedShops = new(r.ReadIntArray());
            this.Discovered = r.ReadBoolean();
            this.Timer = TimeSpan.FromMilliseconds(r.ReadDouble());
            r.ReadIntArray().ToList().ForEach(i => this.Quests.Add(this.World.Map.Town.QuestManager.GetQuest(i)));
            return this;
        }
        public ISyncable Sync(BinaryWriter w)
        {
            w.Write(this.Actor.RefID);
            w.Write(this.TownApprovalRating.Value);
            w.Write(this.ShopBlacklist);
            w.Write(this.RecentlyVisitedShops);
            w.Write(this.Discovered);
            w.Write(this.Timer.TotalMilliseconds);
            return this;
        }
        public ISyncable Sync(BinaryReader r)
        {
            this.ActorID = r.ReadInt32();
            this.TownApprovalRating.Value = r.ReadSingle();
            this.ShopBlacklist = new(r.ReadIntArray());
            this.RecentlyVisitedShops = new(r.ReadIntArray());
            this.Discovered = r.ReadBoolean();
            this.Timer = TimeSpan.FromMilliseconds(r.ReadDouble());
            return this;
        }
        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.Actor.RefID.Save(tag, "ActorID");
            if (!this.Actor.Exists)
                tag.Add(this.Actor.Save("ActorObject"));
            this.TownApprovalRating.Value.Save(tag, "TownApprovalRating");
            this.ShopBlacklist.Save(tag, "ShopBlacklist");
            this.RecentlyVisitedShops.Save(tag, "RecentlyVisitedShops");
            this.Discovered.Save(tag, "Discovered");
            this.Quests.TrySaveRefs(tag, "Quests");
            this.Timer.TotalMilliseconds.Save(tag, "Timer");
            return tag;
        }
        public ISaveable Load(SaveTag tag)
        {
            this.ActorID.TryLoad(tag, "ActorID");
            tag.TryGetTag("ActorObject", t => this.Actor = GameObject.Load(t) as Actor);
            this.TownApprovalRating.Value.TryLoad(tag, "TownApprovalRating");
            this.ShopBlacklist.TryLoad(tag, "ShopBlacklist");
            this.RecentlyVisitedShops.TryLoad(tag, "RecentlyVisitedShops");
            this.Discovered.TryLoad(tag, "Discovered");
            this.Quests.TryLoadRefs(tag, "Quests");
            tag.TryGetTagValue<double>("Timer", v => this.Timer = TimeSpan.FromMilliseconds(v));
            return this;
        }

        internal void ShowQuestsGUI()
        {
            this.Actor.Town.QuestManager.ShowActorGUI(this.Actor);
        }
        static Control GUI;
        internal void ShowGui()
        {
            Control[] tabs = new[] 
            {
                QuestsManager.ActorActiveQuestsGUI,
            };
            var gui = GUI ??= UIHelper.ToTabbedContainer(tabs).ToWindow().SetOnSelectedTargetChangedAction((c, t) =>
            {
                if (t.Object is Actor actor && actor.IsCitizen)
                    c.Hide();
                else if (!(t.Object is Actor))
                    c.Hide();
            });
            foreach (var t in tabs)
                t.GetData(this.Actor);

            gui.AddControlsBottomLeft(UIHelper.ToGroupBoxHorizontally(
                new Label("Town Rating:"), new BarSigned(this.TownApprovalRating) { TextFunc = () => this.TownApprovalRating.Value.ToString("##0") }));

            gui.GetWindow().SetTitle(this.Actor.Name).Show();
        }
    }
}

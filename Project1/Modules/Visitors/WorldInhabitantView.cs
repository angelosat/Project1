using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using Project1.Framework.StaticMaps;
using Project1.Framework.Net;

namespace Start_a_Town_
{
    public class WorldInhabitantView : Inspectable, ITooltippable
    {
        static readonly int PacketSyncAwardTownRating;
        static WorldInhabitantView()
        {
            PacketSyncAwardTownRating = Registry.PacketHandlers.Register(ReceiveAwardTownRating);
        }
     
        public int ActorID;
        public Actor Actor;
      

        public bool Discovered;
        
        public TownApproval TownApprovalRating = new ();
        public HashSet<int> JunkItems = new();
        public IntVec3? HangAroundSpot;

        public TimeSpan Timer = new();
        public FrontierDef CurrentWorldLocation => this.Actor.World.GetFrontierOf(this.Actor);
        public FrontierDef OffsiteArea;
        public HashSet<int> ShopBlacklist = new();
        public HashSet<int> RecentlyVisitedShops = new();
        public readonly ObservableCollection<QuestDef> Quests = new();
        public StaticWorld World;
        public WorldInhabitantView(Actor actor)
        {
            this.Actor = actor;
        }
        public WorldInhabitantView(StaticWorld world, Actor actor, float townVisitChance, int townApprovalRating)
        {
            this.World = world;
            this.Timer = world.Clock;
            this.Actor = actor;
            this.TownApprovalRating.Value = townApprovalRating;
        }
       
        public void GetTooltipInfo(Control tooltip)
        {
        }

        internal WorldInhabitantView AddRecentlyVisitedShop(Workplace shop)
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
        internal TimeSpan GetTimeElapsed()
        {
            return this.World.Clock - this.Timer;
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
       
        internal void BlacklistShop(int shopID)
        {
            this.ShopBlacklist.Add(shopID);
            this.SyncAwardTownRating(-50);
            var shop = this.Actor.Town.ShopManager.GetShop(shopID);
            this.Actor.AI.State.Log.Write($"Blacklisted {shop.Name} because of bad service");
        }
       
        internal bool IsBlacklisted(Workplace shop)
        {
            return this.ShopBlacklist.Contains(shop.ID);
        }

        public void SyncAwardTownRating(float value)
        {
            var net = this.Actor.Net;
            if (net is Client)
                return;
            this.AwardTownRating(value);
            net.BeginPacket(PacketSyncAwardTownRating)
                .Write(this.Actor.RefId)
                .Write(value);
        }
        private static void ReceiveAwardTownRating(NetEndpoint net, Packet packet)
        {
            var r = packet.PacketReader;
            if (net is Server)
                throw new Exception();
            var props = net.World.GetEntity<Actor>(r.ReadInt32()).GetVisitorProperties();
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
            //actor.Net.EventOccured((int)Components.Message.Types.QuestReceived, actor, quest);
            actor.AI.State.Log.Write($"Received quest [{quest}] from [{quest.Giver.Name}]");
            return true;
        }
        internal void AbandonQuest(QuestDef quest)
        {
            this.Quests.Remove(quest);
            //this.Actor.Net.EventOccured((int)Components.Message.Types.QuestAbandoned, this.Actor, quest);
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
                if (t.Object is Actor actor && actor.IsTownMember)
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

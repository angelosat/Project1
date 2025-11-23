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
    public class QuestDef : ISerializable, ISaveable, ILoadReferencable
    {
        readonly public QuestsManager Manager;
        public int ID;
        int _MaxConcurrent = -1;
        public int MaxConcurrent
        {
            get { return this._MaxConcurrent; }
            set { this._MaxConcurrent = Math.Max(-1, value); } // TODO maximum value affected by quest giving skill of quest giver
        }
        readonly List<QuestObjective> Objectives = new();
        readonly ObservableCollection<QuestReward> Rewards = new();

        int GiverID = -1;
        public Actor Giver
        {
            get
            {
                return this.Manager.Net.GetNetworkObject(this.GiverID) as Actor;
            }
            set
            {
                this.GiverID = value?.RefID ?? -1;
                this.Manager.Town.Map.EventOccured(Components.Message.Types.QuestDefAssigned, this);
            }
        }
        public bool IsValid { get { return this.Objectives.Any(); } }
        
        public QuestDef(QuestsManager manager, int id)
        {
            this.Manager = manager; 
            this.ID = id;
        }

        public QuestDef(QuestsManager manager)
        {
            this.Manager = manager;
        }
        
        public bool IsCompleted(Actor actor)
        {
            return this.Objectives.All(o => o.IsCompleted(actor));
        }
        internal bool CanGiveQuestTo(Actor actor)
        {
            return this.IsValid;
        }
        public IEnumerable<QuestObjective> GetObjectives()
        {
            foreach (var o in this.Objectives)
                yield return o;
        }
        public IEnumerable<QuestReward> GetRewards()
        {
            foreach (var o in this.Rewards)
                yield return o;
        }
        
        public QuestDef AddObjective(QuestObjective objective)
        {
            this.Objectives.Add(objective);
            this.Manager.Town.Net.EventOccured(Components.Message.Types.QuestObjectivesUpdated, new[] { objective }, new QuestObjective[] { });
            this.Manager.QuestModified(this);
            return this;
        }
        internal QuestDef RemoveObjective(QuestObjective objective)
        {
            this.Objectives.Remove(objective);
            this.Manager.Town.Net.EventOccured(Components.Message.Types.QuestObjectivesUpdated, new QuestObjective[] {  }, new[] { objective });
            this.Manager.QuestModified(this);
            return this;
        }
        public int GetBudgetTotal()
        {
            return this.Objectives.Sum(o => o.GetValue());
        }
        public int GetRewardTotal()
        {
            return this.Rewards.Sum(r => r.Budget);
        }
        public float GetRewardRatio()
        {
            return this.GetRewardTotal() / (float)this.GetBudgetTotal();
        }
        public override string ToString()
        {
            return string.Format(this.Name + (this.Giver != null ? $" (offered by {this.Giver.Name})" : ""));
        }
        public string Name => this.GetUniqueLoadID();
        private void AutoMatchBudget()
        {
            var budget = this.GetBudgetTotal();
            var reward = this.GetRewardTotal();
            var diff = budget - reward;
            var money = this.Rewards.OfType<QuestRewardMoney>().SingleOrDefault();
            if (money == null)
                this.Rewards.Add(new QuestRewardMoney(this, diff));
            else
                money.Amount += diff;
            this.Manager.QuestModified(this);

        }
        public void TryComplete(Actor actor, OffsiteAreaDef area)
        {
            if (this.IsCompleted(actor))
                return;
            for (int i = 0; i < this.Objectives.Count; i++)
            {
                this.Objectives[i].TryComplete(actor, area);
            }
            if (this.IsCompleted(actor))
                AILog.SyncWrite(actor, $"Received reward for completing [{this.Name}]");
        }
        public IEnumerable<ObjectAmount> GetQuestItemsInInventory(Actor actor)
        {
            foreach (var o in this.Objectives)
                foreach (var i in o.GetQuestItemsInInventory(actor))
                    yield return i;
            yield break;
        }
        internal void Deliver(Actor actor)
        {
            var qgiver = this.Giver;
            var qgiverInv = qgiver.Inventory;
            var items = this.GetQuestItemsInInventory(actor);
            if (items.Any())
            {
                var actorInv = actor.Inventory;
                foreach (var i in items)
                {
                    if (i.Object.StackSize == i.Amount)
                    {
                        actorInv.Remove(i.Object);
                        qgiverInv.Insert(i.Object);
                    }
                    else if (i.Amount < i.Object.StackSize)
                    {
                        var split = i.Object.Split(i.Amount);

                        if (actor.Net is Net.Server server)
                        {
                            split.SyncInstantiate(server);
                            qgiverInv.SyncInsert(split);
                        }
                    }
                    else
                        throw new Exception();
                }
            }

            foreach (var reward in this.Rewards)
                reward.Award(actor);
            actor.GetVisitorProperties().CompleteQuest(this);
        }
        public bool CanAward()
        {
            return this.Rewards.All(r => r.CanAward());
        }
        #region IO
        public void Write(BinaryWriter w)
        {
            w.Write(this.ID);
            this.Objectives.WriteAbstract(w);
            w.Write(this.GiverID);
        }
        public ISerializable Read(BinaryReader r)
        {
            this.ID = r.ReadInt32();
            this.Objectives.InitializeAbstract(r, this);
            this.GiverID = r.ReadInt32();
            return this;
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.ID.Save(tag, "ID");
            this.GiverID.Save(tag, "GiverID");
            this.Objectives.SaveAbstract(tag, "Objectives");
            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            this.ID = tag.GetValue<int>("ID");
            tag.TryGetTagValue("GiverID", out this.GiverID);
            tag.TryGetTag("Objectives", t=> this.Objectives.LoadVariableTypes(t, this));
            return this;
        }
        #endregion

        public string GetUniqueLoadID()
        {
            return $"Quest{this.ID}";
        }

        static Control GUI;
        public void ShowGUI()
        {
            var gui = GUI ??= CreateGUI();
            gui.GetData(this);
            gui.GetWindow().Show();
        }
        static Control CreateGUI()
        {
            var box = new ScrollableBoxNewNew(200, 200);
            QuestDef quest = null;
            ListBoxNoScroll<ItemMaterialAmount, Button> listAvailableReqs = null;
            var iconW = UIManager.Icon16Background.Width;

            void showAdjustObjectiveCountGui(QuestObjective qo)
            {
                DialogInput.ShowInputDialog<int>(
                    "Set value", 
                    value => confirm(() => Packets.SendAdjustObjectiveCount(quest.Manager.Net, quest.Manager.Net.GetPlayer(), qo, value)), 
                    int.TryParse, 
                    64, 
                    qo.Count.ToString());
            }
            void showAdjustRewardCountGui(QuestReward qo)
            {
                DialogInput.ShowInputDialog<int>(
                    "Set value",
                    value => confirm(() => Packets.SendAdjustRewardCount(quest.Manager.Net, quest.Manager.Net.GetPlayer(), qo, value)),
                    int.TryParse,
                    64,
                    qo.Count.ToString());
            }

            var reqList = new TableScrollableCompact<QuestObjective>(true)
                .AddColumn("quantity", "", 32, q => new Label(() => q.Count.ToString(), ()=> showAdjustObjectiveCountGui(q)), 0f)
                .AddColumn("minus", "", iconW, q => IconButton.CreateSmall('-', () => confirm(()=>Packets.SendAdjustObjectiveCount(quest.Manager.Net, quest.Manager.Net.GetPlayer(), q, q.Count - 1))), 0f)
                .AddColumn("plus", "", iconW, q => IconButton.CreateSmall('+', () => confirm(() => Packets.SendAdjustObjectiveCount(quest.Manager.Net, quest.Manager.Net.GetPlayer(), q, q.Count + 1))), 0f)
                .AddColumn("text", "", 200, q => new Label(q.Text), 0f)
                .AddColumn("budget", new Label(() => quest?.GetBudgetTotal().ToString()) { Font = UIManager.FontBold }, 32, qo => new Label(qo.GetValue().ToString), 0f)
                .AddColumn("remove", "", iconW, qo => IconButton.CreateCloseButton().SetLeftClickAction(btn =>
                {
                    var visitorsOnQuest = quest.Manager.GetAllVisitorsOnQuest(quest);
                    if (visitorsOnQuest.Any())
                    {
                        MessageBox.Create("Warning!", "Modifying this quest's objectives while heroes are actively pursuing it, will remove it from their active quests and yield negative effects. Are you sure you want to continue?" +
                            "\n\nHeroes currently on this quest:\n" + string.Join("\n", visitorsOnQuest.Select(v => v.Name).ToArray()), accept).ShowDialog();
                    }
                    else
                        accept();
                    void accept() { Packets.SendQuestObjectiveRemove(quest.Manager.Net, quest.Manager.Net.GetPlayer(), quest, qo); }
                }));

            void confirm(Action action)
            {
                var visitorsOnQuest = quest.Manager.GetAllVisitorsOnQuest(quest);
                if (visitorsOnQuest.Any())
                {
                    MessageBox.Create("Warning!", "Modifying this quest's objectives while heroes are actively pursuing it, will remove it from their active quests and yield negative effects. Are you sure you want to continue?" +
                        "\n\nHeroes currently on this quest:\n" + string.Join("\n", visitorsOnQuest.Select(v => v.Name).ToArray()), action).ShowDialog();
                }
                else
                    action();
            }

            reqList.ListenTo(Components.Message.Types.QuestObjectivesUpdated, args =>
            {
                var added = args[0] as QuestObjective[];
                var removed = args[1] as QuestObjective[];
                reqList.AddItems(added);
                reqList.RemoveItems(removed);
                listAvailableReqs.AddItems(removed.Select(o => (o as QuestObjectiveItem).Objective));
                listAvailableReqs.RemoveWhere(o => added.Any(oo =>
                {
                    var item = (oo as QuestObjectiveItem).Objective;
                    return item.Item == o.Item && item.Material == o.Material;
                }));
            });

            var rewardList = new TableScrollableCompact<QuestReward>()
                .AddColumn(new(), "quantity", 32, qr => new Label(() => qr.Count.ToString(), () => showAdjustRewardCountGui(qr)), 0f)
                .AddColumn("minus", "", iconW, q => IconButton.CreateSmall('-', () => confirm(() => Packets.SendAdjustRewardCount(quest.Manager.Net, quest.Manager.Net.GetPlayer(), q, q.Count - 1))), 0f)
                .AddColumn("plus", "", iconW, q => IconButton.CreateSmall('+', () => confirm(() => Packets.SendAdjustRewardCount(quest.Manager.Net, quest.Manager.Net.GetPlayer(), q, q.Count + 1))), 0f)
                .AddColumn(new(), "text", 200, qr => new Label(qr.Label), 0f)
                .AddColumn(new(), "remove", Icon.Cross.SourceRect.Width, qo => IconButton.CreateCloseButton().SetLeftClickAction(btn => { }));

            var addNewBtn = new Button("Add new", () => createNewObjective(quest));
            var btnRename = new Button("Rename");
            var btnDelete = new Button("Delete");
            box.SetGetDataAction(o =>
            {
                if (quest == o)
                    return;
                quest = o as QuestDef;
                reqList.ClearItems();
                reqList.AddItems(quest.GetObjectives());
                rewardList.Bind(quest.Rewards);
                box.GetWindow()?.SetTitle(quest.ToString());
            });
            var modMaxCurrentDialog = new DialogInput("Enter value", control =>
            {
                if (int.TryParse(control.Input, out int modValue))
                {
                    Packets.SendQuestModify(quest.Manager.Net, quest.Manager.Net.GetPlayer(), quest, modValue);
                    control.Hide();
                }
            }, 300, quest?.MaxConcurrent.ToString() ?? "this shouldn't happen");
            var boxMaxConcurrent = new GroupBox().AddControlsHorizontally(
                new Label(() => "Max Available:"),
                IconButton.CreateSmall('-', () => modMaxConcurrent(quest.MaxConcurrent - 1)),
                IconButton.CreateSmall('+', () => modMaxConcurrent(quest.MaxConcurrent + 1)),
                new Label(() =>
                {
                    if (quest == null)
                        return "";
                    var v = quest.MaxConcurrent;
                    return (v == -1 ? "unlimited" : v.ToString());
                }, () => modMaxCurrentDialog.SetText(quest?.MaxConcurrent.ToString()).ShowDialog())
                { Width = (int)UIManager.Font.MeasureString("unlimited").X },
                new Label(() => $"(Currently {quest?.Manager.Town.Map.World.Population.Find(v => v.HasQuest(quest)).Count()})")
                );

            var guiReward = new GroupBox().AddControlsLineWrap(new[] {
                new Label(() => $"Desirability"),
                new Label(() => $"{quest?.GetRewardRatio():0%}")
                {
                    TextColorFunc = () =>
                    {
                        var ratio = quest.GetRewardRatio();
                        if (ratio <= 1)
                            return Color.Lerp(Color.Yellow, Color.Red, (1f - ratio) * 2f);
                        else
                            return Color.Lerp(Color.Yellow, Color.Lime, (ratio - 1f) * 2f);

                    }
                } 
            });
            var btnMatchBudget = new Button("Match budget with currency", ()=>Packets.SendAutoMatchBudget(quest.Manager.Net, quest.Manager.Net.GetPlayer(), quest));

            box.AddControlsVertically(
               boxMaxConcurrent,
               new GroupBox().AddControlsHorizontally(addNewBtn, btnRename, btnDelete),
               reqList.ToPanelLabeled("Objectives"),
               rewardList.ToPanelLabeled("Rewards"),
               guiReward,
               btnMatchBudget);
            _ = box.ToWindow();
            return box;

            void modMaxConcurrent(int modValue)
            {
                Packets.SendQuestModify(quest.Manager.Net, quest.Manager.Net.GetPlayer(), quest, modValue);
            }
            void createNewObjective(QuestDef quest)
            {
                var box = new GroupBox();

                var items = Def.Database.Values.OfType<ItemDef>().Where(d => d.Category == ItemCategoryDefOf.RawMaterials).SelectMany(d => d.GenerateVariants());
                listAvailableReqs = new ListBoxNoScroll<ItemMaterialAmount, Button>(def => new(def.ToString(), () =>
                {
                    Packets.SendQuestCreateObjective(quest.Manager.Town.Net, quest.Manager.Town.Net.GetPlayer(), quest, new QuestObjectiveItem(quest, def));
                }));
                listAvailableReqs.AddItems(items);
                box.AddControlsVertically(listAvailableReqs);
                var contextLoc = new Microsoft.Xna.Framework.Vector2(reqList.Parent.Parent.BoundsScreen.Right, reqList.Parent.Parent.BoundsScreen.Top);
                var context = box.ToContextMenuClosable("Select item", contextLoc);

                context.Show();
            }
        }

        static QuestDef()
        {
            Packets.Init();
        }
        class Packets
        {
            static int PacketQuestCreateObjective, PacketQuestModify, PacketQuestRemoveObjective, PacketAutoMatchBudget, PacketAdjustObjectiveCount, PacketAdjustRewardCount;
            internal static void Init()
            {
                PacketQuestCreateObjective = Network.RegisterPacketHandler(ReceiveQuestCreateObjective);
                PacketQuestModify = Network.RegisterPacketHandler(ReceiveQuestModify);
                PacketQuestRemoveObjective = Network.RegisterPacketHandler(ReceiveQuestRemoveObjective);
                PacketAutoMatchBudget = Network.RegisterPacketHandler(HandleAutoMatchBudget);
                PacketAdjustObjectiveCount = Network.RegisterPacketHandler(HandleAdjustObjectiveCount);
                PacketAdjustRewardCount = Network.RegisterPacketHandler(HandleAdjustRewardCount);
            }

            internal static void SendAutoMatchBudget(INetwork net, PlayerData player, QuestDef quest)
            {
                if (net is Server)
                    quest.AutoMatchBudget();
                net.GetOutgoingStream().Write(PacketAutoMatchBudget, player.ID, quest.ID);
            }
            private static void HandleAutoMatchBudget(INetwork net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var quest = net.Map.Town.QuestManager.GetQuest(r.ReadInt32());
                if (net is Client)
                    quest.AutoMatchBudget();
                else
                    SendAutoMatchBudget(net, player, quest);
            }

            internal static void SendQuestCreateObjective(INetwork net, PlayerData player, QuestDef quest, QuestObjective qobj)
            {
                if (net is Server)
                {
                    quest.AddObjective(qobj);
                }
                var w = net.GetOutgoingStream();
                w.Write(PacketQuestCreateObjective);
                w.Write(player.ID);
                w.Write(quest.ID);
                w.Write(qobj.GetType().FullName);
                qobj.Write(w);
            }
            private static void ReceiveQuestCreateObjective(INetwork net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var quest = net.Map.Town.QuestManager.GetQuest(r.ReadInt32());
                var qObj = Activator.CreateInstance(Type.GetType(r.ReadString()), quest) as QuestObjective;
                qObj.Read(r);
                if (net is Server)
                {
                    SendQuestCreateObjective(net, player, quest, qObj);
                }
                else
                {
                    quest.AddObjective(qObj);
                }
            }

            internal static void SendQuestModify(INetwork net, PlayerData player, QuestDef quest, int maxConcurrentModValue)
            {
                if (net is Server)
                    quest.MaxConcurrent = maxConcurrentModValue;
                net.GetOutgoingStream().Write(PacketQuestModify, player.ID, quest.ID, maxConcurrentModValue);
            }
            private static void ReceiveQuestModify(INetwork net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var quest = net.Map.Town.QuestManager.GetQuest(r.ReadInt32());
                var maxConcurrentModValue = r.ReadInt32();
                if (net is Client)
                    quest.MaxConcurrent = maxConcurrentModValue;
                else
                    SendQuestModify(net, player, quest, maxConcurrentModValue);
            }

            internal static void SendQuestObjectiveRemove(INetwork net, PlayerData player, QuestDef quest, QuestObjective qobj)
            {
                var index = quest.GetObjectives().ToList().FindIndex(i => i == qobj);
                if (net is Server server)
                    quest.RemoveObjective(qobj);
                var w = net.GetOutgoingStream();
                w.Write(PacketQuestRemoveObjective);
                w.Write(player.ID);
                w.Write(quest.ID);
                w.Write(index);
            }
            private static void ReceiveQuestRemoveObjective(INetwork net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var quest = net.Map.Town.QuestManager.GetQuest(r.ReadInt32());
                var objectiveIndex = r.ReadInt32();
                var objective = quest.GetObjectives().ElementAt(objectiveIndex);
                if (net is Server)
                    SendQuestObjectiveRemove(net, player, quest, objective);
                else
                    quest.RemoveObjective(objective);
            }

            internal static void SendAdjustObjectiveCount(INetwork net, PlayerData player, QuestObjective objective, int count)
            {
                if (net is Server)
                    objective.Count = count;
                var q = objective.Parent;
                net.GetOutgoingStream().Write(PacketAdjustObjectiveCount, player.ID, q.ID, q.GetObjectives().ToList().FindIndex(i => i == objective), count);
            }
            private static void HandleAdjustObjectiveCount(INetwork net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var quest = net.Map.Town.QuestManager.GetQuest(r.ReadInt32());
                var objective = quest.GetObjectives().ElementAt(r.ReadInt32());
                var count = r.ReadInt32();
                if (net is Server)
                    SendAdjustObjectiveCount(net, player, objective, count);
                else
                    objective.Count = count;
            }

            internal static void SendAdjustRewardCount(INetwork net, PlayerData player, QuestReward reward, int count)
            {
                if (net is Server)
                    reward.Count = count;
                var q = reward.Parent;
                net.GetOutgoingStream().Write(PacketAdjustRewardCount, player.ID, q.ID, q.GetRewards().ToList().FindIndex(i => i == reward), count);
            }
            private static void HandleAdjustRewardCount(INetwork net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var quest = net.Map.Town.QuestManager.GetQuest(r.ReadInt32());
                var reward = quest.GetRewards().ElementAt(r.ReadInt32());
                var count = r.ReadInt32();
                if (net is Server)
                    SendAdjustRewardCount(net, player, reward, count);
                else
                    reward.Count = count;
            }
        }
    }
}

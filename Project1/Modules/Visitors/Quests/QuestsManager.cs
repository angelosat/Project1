using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using System.IO;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    public partial class QuestsManager : TownComponent
    {
        readonly Lazy<Window> UIWindowQuests;
        readonly Lazy<Control> UIAssignQuests;
      
        public override string Name => "Quests";
        int QuestGiverIDSequence = 1;
        readonly List<QuestDef> Quests = new();
        readonly Dictionary<int, QuestGiverProperties> QuestGiverProperties = new();
        readonly Dictionary<int, int> PendingQuestRequests = new();
        static QuestsManager()
        {
            Registry.GameEvents.Register<QuestDefAssignedEvent>();
            Registry.GameEvents.Register<QuestDefsUpdatedEvent>();
            Packets.Init();
        }
        public QuestsManager(Town town) : base(town)
        {
            this.UIWindowQuests = new Lazy<Window>(this.CreateUI);
            this.UIAssignQuests = new Lazy<Control>(this.CreateQuestGiverAssignmentWindow);
        }

        QuestDef CreateQuest()
        {
            var qg = new QuestDef(this, QuestGiverIDSequence++);
            return qg;
        }
        
        private void AddQuest(QuestDef qg)
        {
            this.Quests.Add(qg);
            this.ReportQuestsUpdated(new QuestDef[] { qg }, new QuestDef[] { });
        }
        private void AddQuest(int id)
        {
            if (this.Town.Map.Net is not Client)
                throw new Exception();
            this.AddQuest(new QuestDef(this, id));
        }
        private void RemoveQuest(int questID)
        {
            var qg = this.GetQuest(questID);
            this.Quests.Remove(qg);
            this.ReportQuestsUpdated(new QuestDef[] {  }, new QuestDef[] { qg });
        }
        void ReportQuestsUpdated(QuestDef[] added, QuestDef[] removed)
        {
            //this.Town.Net.EventOccured((int)Components.Message.Types.QuestDefsUpdated, added, removed);
            this.Town.Net.EventOccured(new QuestDefsUpdatedEvent(added, removed));// { Added = added, Removed = removed });
        }
        public QuestDef GetQuest(int id)
        {
            return this.Quests.FirstOrDefault(q => q.ID == id);
        }
        public IEnumerable<QuestDef> GetQuestDefs()
        {
            foreach (var g in this.Quests)
                yield return g;
        }
       
        internal void HandleQuestReceiver(Actor receiver, QuestDef q)
        {
            var giver = q.Giver;
            this.GetQuestGiverProperties(q.Giver).HandleReceiver(receiver);
            this.PendingQuestRequests.Add(giver.RefId, receiver.RefId);
        }
        public void RemoveQuestReceiver(QuestDef q)
        {
            var giver = q.Giver;
            this.GetQuestGiverProperties(q.Giver).RemoveReceiver();
            this.PendingQuestRequests.Remove(giver.RefId);
        }
        public void RemoveQuestReceiver(int qID)
        {
            var giver = this.GetQuest(qID).Giver;
            this.GetQuestGiverProperties(giver).RemoveReceiver();
            this.PendingQuestRequests.Remove(giver.RefId);
        }
        internal Actor GetNextQuestReceiver(Actor giver)
        {
            var id = this.QuestGiverProperties[giver.RefId].GetNextQuestReceiverID();
            return id != -1 ? this.Town.Map.World.GetEntity(id) as Actor : null;
        }
        public IEnumerable<Actor> GetAllVisitorsOnQuest(QuestDef quest)
        {
            return this.Town.Map.World.Population.Find(v => v.HasQuest(quest)).Select(v => v.Actor);
        }
        public void QuestModified(QuestDef quest)
        {
            foreach (var v in this.Town.Map.World.Population.Find(v => v.HasQuest(quest)))
                v.AbandonQuest(quest);
        }
        internal QuestGiverProperties GetQuestGiverProperties(Actor actor)
        {
            return this.QuestGiverProperties[actor.RefId];
        }
        
        internal override void OnCitizenAdded(int actorID)
        {
            this.QuestGiverProperties.Add(actorID, new QuestGiverProperties(actorID));
        }
        internal override void OnCitizenRemoved(int actorID)
        {
            this.QuestGiverProperties.Remove(actorID);
        }
        internal override IEnumerable<Tuple<Func<string>, Action>> OnQuickMenuCreated()
        {
            var win = this.UIWindowQuests;
            yield return new Tuple<Func<string>, Action>(()=>"Quests", () => win.Value.Toggle());
        }

        static readonly Button BtnQuests = new("Quests");
        internal override IEnumerable<Button> GetTabs(ISelectable selected)
        {
            if (selected is not TargetArgs target)
                yield break;
            if (target.Object is not Actor actor)
                yield break;
            if (actor.IsTownMember)
            {
                //yield return("Quests", () => ShowUI(this.UIAssignQuests, $"Assign quests to {actor.Name}"));
                yield return BtnQuests.SetLeftClickAction(() => ShowUI(this.UIAssignQuests, $"Assign quests to {actor.Name}")) as Button;
            }
            else
            {
            }
        }
        internal override void OnTargetSelected(IUISelection info, ISelectable selected)
        {
            if (selected is not TargetArgs target)
                return;
            if (target.Object is not Actor actor)
                return;
            if (actor.IsTownMember)
            {
                this.UIAssignQuests.Value.GetData(actor);
            }
            else
            {
            }
        }

        private void ShowUI(Lazy<Control> lazyUI, string title)
        {
            var window = lazyUI.Value.GetWindow() ?? lazyUI.Value.ToWindow(title);
            window.Toggle();
        }
        private void ShowUI(Control gui, string title)
        {
            var window = gui.GetWindow() ?? gui.ToPanelLabeled().ToWindow(title);
            window.Toggle();
        }
        class QuestDefsUpdatedEvent(QuestDef[] added, QuestDef[] removed) : EventPayloadBase
        {
            public QuestDef[] Added = added;
            public QuestDef[] Removed = removed;
        }
        class QuestDefAssignedEvent(QuestDef quest) : EventPayloadBase
        {
            public QuestDef Quest = quest;
        }
        Window CreateUI()
        {
            var box = new GroupBox();
            var qlist = new TableScrollableCompact<QuestDef>()
                .AddColumn(new(), "text", 150, qg => new Label(qg).SetLeftClickAction(lbl =>
                {
                    qg.ShowGUI();
                }))
                .AddColumn(new(), "text", Icon.X.AtlasToken.Rectangle.Width, (table, qg) => IconButton.CreateCloseButton().SetLeftClickAction(btn => Packets.RemoveQuest(this, this.Town.Net.GetPlayer().ID, qg)))
                ;

            var net = this.Town.Net;
            var btnCreate = new Button("Create", () => Packets.SendAddQuestGiver(net, net.GetPlayer().ID));
        
            qlist.AddItems(this.Quests);
            //qlist.ListenToOld((int)Components.Message.Types.QuestDefsUpdated, args =>
            //{
            //    var added = args[0] as QuestDef[];
            //    var removed = args[1] as QuestDef[];
            //    qlist.AddItems(added);
            //    qlist.RemoveItems(removed);
            //});
            qlist.ListenTo<QuestDefsUpdatedEvent>(args =>
            {
                qlist.AddItems(args.Added);
                qlist.RemoveItems(args.Removed);
            });
            box.AddControlsVertically(
                btnCreate,
                qlist.ToPanelLabeled("Quest list"));
            return box.ToWindow("Quests").MoveToScreenCenter() as Window;
        }
        

        Control CreateQuestGiverAssignmentWindow()
        {
            var box = new GroupBox();
            Actor actor = null;
            var questsAvailable = new TableScrollableCompact<QuestDef>()
                .AddColumn(new(), "questname", 200, q => new Label(q, () => edit(q)), 0)
                .AddColumn(new(), "assign", 16, q => IconButton.CreateSmall(Icon.ArrowRight, () => assign(q), $"Assign {q} to {actor.Name}"), 0);
            var editbtnwidth = Button.GetWidth(UIManager.Font, "Edit");
            var questsAssigned = new TableScrollableCompact<QuestDef>()
                .AddColumn(new(), "questname", 200, q => new Label(q, () => edit(q)), 0)
                .AddColumn(new(), "unassign", 16, q => IconButton.CreateSmall(Icon.Cross, () => unassign(q), $"Assign {q} to {actor.Name}"), 0);

            box.SetGetDataAction(o =>
            {
                if (o is not Actor a)
                    return;
                actor = a;
                questsAvailable.ClearItems();
                questsAssigned.ClearItems();

                var assigned = this.Quests.Where(q => q.Giver == a).ToList();
                questsAssigned.AddItems(assigned);
                questsAvailable.AddItems(this.Quests.Except(assigned));
            });

            box.AddControlsHorizontally(
                questsAvailable.ToPanelLabeled("Available")
                ,
                questsAssigned.ToPanelLabeled("Assigned")
                );
            //box.ListenToOld((int)Components.Message.Types.QuestDefAssigned, args =>
            //{
            //    var q = args[0] as QuestDef;
            //    if (q.Giver == actor)
            //    {
            //        questsAvailable.RemoveItems(q);
            //        questsAssigned.AddItems(q);
            //    }
            //    else
            //    {
            //        questsAssigned.RemoveItems(q);
            //        questsAvailable.AddItems(q);
            //    }
            //});
            box.ListenTo<QuestDefAssignedEvent>(args =>
            {
                var q = args.Quest;
                if (q.Giver == actor)
                {
                    questsAvailable.RemoveItems(q);
                    questsAssigned.AddItems(q);
                }
                else
                {
                    questsAssigned.RemoveItems(q);
                    questsAvailable.AddItems(q);
                }
            });
            return box;

            void edit(QuestDef q)
            {
                q.ShowGUI();
            }
            void assign(QuestDef q)
            {
                var net = this.Town.Map.Net;
                Packets.SendQuestGiverAssign(net, net.GetPlayer(), q, actor);
            }
            void unassign(QuestDef q)
            {
                var net = this.Town.Map.Net;
                Packets.SendQuestGiverAssign(net, net.GetPlayer(), q, null);
            }
        }
        
        static ListBoxObservable<QuestDef, Label> _ActorActiveQuestsGUI;
        static public ListBoxObservable<QuestDef, Label> ActorActiveQuestsGUI
        {
            get
            {
                return _ActorActiveQuestsGUI ??=
                    new ListBoxObservable<QuestDef, Label>(q => new Label(q.Name, q.ShowGUI))
                    { Name = "Active quests" }
                    .SetGetDataAction(o => ActorActiveQuestsGUI.Bind((o as Actor).GetVisitorProperties().Quests)) as ListBoxObservable<QuestDef, Label>;
            }
        }

        static Control ActorGUI;
        static Control CreateActorGUI()
        {
            var listQuests = ActorActiveQuestsGUI;
            var box = UIHelper.ToTabbedContainer(listQuests);

            box.SetGetDataAction(o =>
            {
                listQuests.GetData(o);
                var props = (o as Actor).GetVisitorProperties();
                box.GetWindow().SetTitle(props.Actor.Name);
            });
            box.ToWindow("Visitor Properties");
            return box;
        }
        static Control GetActorGUI()
        {
            return ActorGUI ??= CreateActorGUI();
        }
        internal void ShowActorGUI(Actor actor)
        {
            var gui = GetActorGUI();
            gui.GetData(actor);
            gui.GetWindow().Show();
        }
        public override void Write(BinaryWriter w)
        {
            w.Write(this.QuestGiverIDSequence);
            this.Quests.Write(w);
            this.PendingQuestRequests.Write(w);
        }
        public override void Read(IDataReader r)
        {
            this.QuestGiverIDSequence = r.ReadInt32();
            this.Quests.InitializeNew(r, this);
            this.PendingQuestRequests.Read(r);
        }
        protected override void AddSaveData(SaveTag tag)
        {
            this.QuestGiverIDSequence.Save(tag, "QuestIDSequence");
            this.Quests.SaveNewBEST(tag, "Quests");
        }
        public override void Load(SaveTag tag)
        {
            tag.TryGetTagValue("QuestIDSequence", out this.QuestGiverIDSequence);
            this.Quests.TryLoadList<List<QuestDef>, QuestDef>(tag, "Quests", this);
        }
    }
}

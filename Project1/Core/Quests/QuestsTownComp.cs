using Project1.Core.Entities;
using Project1.Core.Screens;
using Project1.Core.Systems.Materials;
using Project1.Core.Towns;
using Project1.Framework.Events;
using Project1.Framework.Helpers;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;

namespace Project1.Core.Quests
{
    record struct PlayerRequestQuestCreationEvent(MaterialRefinementDef RefinementDef, MaterialDef MaterialDef) : IEventPayload { }
    record struct PlayerRequestQuestDeletionEvent(QuestId Id) : IEventPayload { }
    public readonly record struct QuestId(int Value)
    {
        internal static readonly QuestId Null = new(0);
        public static implicit operator QuestId(int v) => new(v);
        public static implicit operator int(QuestId v) => v.Value;
    }
    internal sealed class CreateFetchQuestGui : GroupBox
    {
        MaterialRefinementDef refDef;
        MaterialTypeDef typeDef;
        MaterialDef materialDef;
        int Reward;
        readonly ChangeNotifier Notifier = new();
        readonly ButtonFinal BtnApply, BtnCancel;
    

        readonly ComboBoxFinal<MaterialRefinementDef> ComboRefinementDef;
        readonly ComboBoxFinal<MaterialDef> ComboMaterialDef;
        readonly LabelNew LabelReward;
        public CreateFetchQuestGui()
        {
            this.ComboRefinementDef = new ComboBoxFinal<MaterialRefinementDef>(() => Def.GetDefs<MaterialRefinementDef>(), 100, def => def?.LabelReadable, SetRefDef, () => refDef);
            this.ComboRefinementDef.InvalidateOn(this.Notifier);
            this.ComboMaterialDef = new ComboBoxFinal<MaterialDef>(() => this.refDef is not null ? RawMaterialSystem.GetMaterialsByType(this.refDef.MaterialType) : [], 100, def => def?.LabelReadable, SetMatDef, () => materialDef);
            this.ComboMaterialDef.InvalidateOn(this.Notifier);
            this.BtnApply = new(() => "Apply", Apply, 64);
            this.BtnCancel = new(() => "Cancel", () => { }, 64);
            this.LabelReward = new LabelNew(() => $"Reward: {this.Reward}") { Width = 100 };
            this.LabelReward.InvalidateOn(this.Notifier);
            var panelcbox  = new Panel() { AutoSize = true };
            panelcbox.AddControlsHorizontally(this.ComboRefinementDef, this.ComboMaterialDef);
            var panelbuttons = new Panel() { AutoSize = true };
            panelbuttons.AddControlsHorizontally(BtnApply, BtnCancel);
            //this.AddControlsHorizontally(this.ComboRefinementDef, this.ComboMaterialDef);
            //this.AddControlsBottomLeft([BtnApply, BtnCancel]);
            this.AddControlsVertically([panelcbox, this.LabelReward.ToPanel(), panelbuttons]);
        }

        private void Apply()
        {
            Ingame.Instance.Events.Post(new PlayerRequestQuestCreationEvent(this.refDef, this.materialDef));
        }

        void CalculateReward()
        {
            this.Reward = ItemDefOf.Ingredient.BaseValue * this.materialDef?.Value ?? 0;
        }

        void SetRefDef(MaterialRefinementDef def)
        {
            this.refDef = def;
            if(this.refDef.MaterialType != this.typeDef)
            {
                this.SetMatDef(null);
                this.typeDef = null;
            }
            else
                this.typeDef = this.refDef.MaterialType;
            this.Notifier.Notify();
        }
        void SetMatDef(MaterialDef def)
        {
            this.materialDef = def;
            this.CalculateReward();
            this.Notifier.Notify();
        }
    }
    internal sealed class CreateQuestGui : GroupBox
    {
        ItemDef itemDef = null;
        Def profileDef = null;
        Type profileDefType;
        MaterialDef materialDef = null;
        public CreateQuestGui()
        {

            var comboItemDef = new ComboBoxFinal<ItemDef>(Def.GetDefs<ItemDef>(), 100, def => def?.LabelReadable, def => setItem(def), () => itemDef);
            //var comboProfileDef = new ComboBoxNewNew<Def>(Def.GetDefs(this.profileDefType), 100, def => def?.LabelReadable, def => profileDef = def, () => profileDef);
            var comboProfileDef = new ComboBoxFinal<Def>(() => Def.GetDefs(this.profileDefType), 100, def => def?.LabelReadable, def => profileDef = def, () => profileDef);
            var comboMaterialDef = new ComboBoxFinal<MaterialDef>(Def.GetDefs<MaterialDef>(), 100, def => def?.LabelReadable, def => materialDef = def, () => materialDef);

            this.AddControlsHorizontally(comboItemDef, comboProfileDef, comboMaterialDef);
        }

        void setItem(ItemDef def)
        {
            this.itemDef = def;
            this.profileDefType = def?.ProfileType;
        }
    }
    internal sealed class QuestsGuiNew : GroupBox
    {
        readonly ListBoxNoScroll<QuestRuntime> ListQuests;
        readonly Table<QuestRuntime> TableQuests;
        readonly QuestsTownComp Comp;
        public QuestsGuiNew(QuestsTownComp comp)
        {
            var btn = new Button("Create Quest", 100) { LeftClickAction = () => new CreateFetchQuestGui().ToWindow("Create Quest").Show() };
            this.Comp = comp;
            comp.Added += OnQuestAdded;
            comp.Removed += OnQuestRemoved;
            this.ListQuests = new((q) => new LabelNew(() => $"{q.LabelReadable}"));
            var w = 256;
            this.TableQuests = new Table<QuestRuntime>()
                //.AddColumn("label", 256, q => new LabelNew(() => q.LabelReadable) { Active = true })
                .AddColumn("label", 256, q => new LabelNew(q) { MouseThrough = false })
                .AddColumn("reward", 96, q => new LabelNew($"Reward: {q.Reward}") { MouseThrough = false })
                .AddColumn("delete", 16 /*Icon.Cross.Width*/, q => IconButton.CreateSmall(Icon.Cross, () => Delete(q), "Delete").ShowOnParentFocus(true))
                ;

            //this.Controls.Add(btn);
            //this.AddControlsVertically(this.ListQuests.ToScrollableBox(300,300, ScrollModes.Vertical).ToPanel(), btn.ToPanel());
            //this.AddControlsVertically(this.TableQuests.ToScrollableBox(this.TableQuests.RowWidth, 300, ScrollModes.Vertical).ToPanel(), btn.ToPanel());
            this.AddControlsVertically(ScrollableBoxNewNewNew.FromWidth(this.TableQuests, this.TableQuests.RowWidth, 300).ToPanel(), btn.ToPanel());
        }

        private static void Delete(QuestRuntime q)
        {
            Ingame.Instance.Events.Post(new PlayerRequestQuestDeletionEvent(q.Id));
        }

        private void OnQuestRemoved(QuestRuntime q)
            //=> this.ListQuests.RemoveItems([q]);
            => this.TableQuests.RemoveItem(q);

        private void OnQuestAdded(QuestRuntime q)
            //=> this.ListQuests.AddItems([q]);
            => this.TableQuests.AddItem(q);

        protected override void OnHidden()
        {
            this.Comp.Added -= OnQuestAdded;
            this.Comp.Removed -= OnQuestRemoved;

            base.OnHidden();
        }
    }
    public sealed class QuestsTownComp : TownComponent
    {
        public override string Name => "Quests";
        //readonly ChangeNotifier Notifier = new();
        QuestId _nextQuestId = 1;
        QuestId GetNextQuestId() => this._nextQuestId++;
        readonly Dictionary<QuestId, QuestRuntime> AllQuests = [];
        readonly Dictionary<(MaterialRefinementDef, MaterialDef), QuestId> FetchQuests = [];
        readonly Dictionary<QuestId, FetchQuestRuntime> FetchQuestsById = [];
        public Action<QuestRuntime> Added, Removed;
        public QuestsTownComp(Town town) : base(town)
        {
        }

        internal override IEnumerable<(Func<string>, Action)> OnQuickMenuCreated()
        {
            yield return (() => "QuestsNew", () => new QuestsGuiNew(this).ToWindow("Quests").Show());
        }
       
        internal bool TryCreateQuest(MaterialRefinementDef refdef, MaterialDef matdef)
        {
            var key = (refdef, matdef);
            if (this.FetchQuests.TryGetValue(key, out _))
                return false;
            var reward = ItemDefOf.Ingredient.BaseValue * matdef.Value;
            var quest = new FetchQuestRuntime(this.GetNextQuestId(), reward, refdef, matdef);
            this.AllQuests.Add(quest.Id, quest);
            this.FetchQuests[key] = quest.Id;
            this.FetchQuestsById[quest.Id] = quest;
            //this.Notifier.Notify();
            this.Added?.Invoke(quest);
            return true;
        }
        internal void DeleteQuest(QuestId id)
        {
            var q = this.AllQuests[id];
            switch (q)
            {
                case FetchQuestRuntime:
                    var fq = this.FetchQuestsById[id];
                    this.FetchQuests.Remove((fq.Refinement, fq.Material));
                    this.FetchQuestsById.Remove(id);
                    break;
            }
            this.AllQuests.Remove(id);
            this.Removed?.Invoke(q);
        }
    }
}

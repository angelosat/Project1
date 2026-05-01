using Project1.Core.Entities;
using Project1.Core.Screens;
using Project1.Core.Systems.Materials;
using Project1.Framework.Events;
using Project1.Framework.Helpers;
using Project1.Framework.UI;
using System;
using System.Linq;

namespace Project1.Core.Systems.Quests;


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
        this.ComboRefinementDef = new ComboBoxFinal<MaterialRefinementDef>(() => Def.Get<MaterialRefinementDef>(), 100, def => def?.LabelReadable, SetRefDef, () => refDef);
        this.ComboRefinementDef.InvalidateOn(this.Notifier);
        this.ComboMaterialDef = new ComboBoxFinal<MaterialDef>(() => this.refDef is not null ? MaterialSystem.GetMaterialsByType(this.refDef.MaterialType) : [], 100, def => def?.LabelReadable, SetMatDef, () => materialDef);
        this.ComboMaterialDef.InvalidateOn(this.Notifier);
        this.BtnApply = new(() => "Apply", Apply, 64);
        this.BtnCancel = new(() => "Cancel", () => { }, 64);
        this.LabelReward = new LabelNew(() => $"Reward: {this.Reward}") { Width = 100 };
        this.LabelReward.InvalidateOn(this.Notifier);
        var panelcbox = new Panel() { AutoSize = true };
        panelcbox.AddControlsHorizontally(this.ComboRefinementDef, this.ComboMaterialDef);
        var panelbuttons = new Panel() { AutoSize = true };
        panelbuttons.AddControlsHorizontally(BtnApply, BtnCancel);
        //this.AddControlsHorizontally(this.ComboRefinementDef, this.ComboMaterialDef);
        //this.AddControlsBottomLeft([BtnApply, BtnCancel]);
        this.AddControlsVertically([panelcbox, this.LabelReward.ToPanel(), panelbuttons]);
    }

    private void Apply()
    {
        Ingame.Instance.Events.Post(new PlayerRequestQuestCreationEvent(Ingame.Net.MainView.Map.ID, this.refDef, this.materialDef));
    }

    void CalculateReward()
    {
        this.Reward = ItemDefOf.Ingredient.BaseValue * this.materialDef?.Value ?? 0;
    }

    void SetRefDef(MaterialRefinementDef def)
    {
        this.refDef = def;
        if (this.refDef.MaterialType != this.typeDef)
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

        var comboItemDef = new ComboBoxFinal<ItemDef>(Def.Get<ItemDef>(), 100, def => def?.LabelReadable, def => SetItem(def), () => itemDef);
        //var comboProfileDef = new ComboBoxNewNew<Def>(Def.GetDefs(this.profileDefType), 100, def => def?.LabelReadable, def => profileDef = def, () => profileDef);
        var comboProfileDef = new ComboBoxFinal<Def>(() => Def.Get(this.profileDefType), 100, def => def?.LabelReadable, def => profileDef = def, () => profileDef);
        var comboMaterialDef = new ComboBoxFinal<MaterialDef>(Def.Get<MaterialDef>(), 100, def => def?.LabelReadable, def => materialDef = def, () => materialDef);

        this.AddControlsHorizontally(comboItemDef, comboProfileDef, comboMaterialDef);
    }

    void SetItem(ItemDef def)
    {
        this.itemDef = def;
        this.profileDefType = def?.ProfileType;
    }
}
internal sealed class QuestsGuiNew : GroupBox
{
    readonly ListBoxNoScroll<QuestRuntime> ListQuests;
    readonly Table<QuestRuntime> TableQuests;
    readonly TownComp_Quests Comp;
    public QuestsGuiNew(TownComp_Quests comp)
    {
        var btn = new Button("Create Quest", 100) { LeftClickAction = () => new CreateFetchQuestGui().ToWindow("Create Quest").Show() };
        this.Comp = comp;
        comp.Added += OnQuestAdded;
        comp.Removed += OnQuestRemoved;
        this.ListQuests = new((q) => new LabelNew(() => $"{q.LabelReadable}"));
        var w = 256;
        this.TableQuests = new Table<QuestRuntime>()
            .AddColumn("label", 256, q => new LabelNew(q) { MouseThrough = false })
            .AddColumn("reward", 96, q => new LabelNew($"Reward: {q.Reward}") { MouseThrough = false })
            .AddColumn("assigned", 64, q => new LabelNew(() => $"Assigned: {comp.GetAssignedActorsByQuest(q.Id).Count}")
            {
                MouseThrough = false,
                HoverFunc = () => string.Join(Environment.NewLine, comp.GetAssignedActorsByQuest(q.Id).Select(id => comp.Map.World.Get<Entity>(id).LabelReadable))
            }.InvalidateOn(comp.Notifier))
            .AddColumn("delete", 16 /*Icon.Cross.Width*/, q => IconButton.CreateSmall(Icon.Cross, () => Delete(q), "Delete").ShowOnParentFocus(true))
            ;
        this.TableQuests.AddItems(comp.AllQuests);
        this.AddControlsVertically(ScrollableBoxNewNewNew.FromWidth(this.TableQuests, this.TableQuests.RowWidth, 300).ToPanel(), btn.ToPanel());
    }

    private static void Delete(QuestRuntime q)
    {
        Ingame.Instance.Events.Post(new PlayerRequestQuestDeletionEvent(Ingame.Net.MainView.Map.ID, q.Id));
    }

    private void OnQuestRemoved(QuestRuntime q)
        => this.TableQuests.RemoveItem(q);

    private void OnQuestAdded(QuestRuntime q)
        => this.TableQuests.AddItem(q);

    protected override void OnHidden()
    {
        this.Comp.Added -= OnQuestAdded;
        this.Comp.Removed -= OnQuestRemoved;

        base.OnHidden();
    }
}

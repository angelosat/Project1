using Project1.Core.Blocks;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Core.Systems.Crafting;
using Project1.Core.Towns.Stockpiles;
using Project1.Framework.Helpers;
using Project1.Framework.UI;
using System;
using System.Linq;

namespace Project1.Core.UI;

class WorkstationGuiNew : SelectionBoundControl
{
    Panel PanelReactions;
    readonly ListBoxNoScroll<CraftingOrder, Gui_CraftingOrderContainer> ListOrdersNew;
    Table<(string label, Func<ZoneId> zoneIdGetter, WorkstationIOType iotype)> IOTable;
    BlockWorkstationComp Workstation;

    public WorkstationGuiNew()
    {
        this.ListOrdersNew = new(s => new Gui_CraftingOrderContainer(s, s => this.MoveUp(s), s => this.MoveDown(s)));

    }
    class Gui_CraftingOrderContainer : GroupBox
    {
        public readonly ButtonIcon Up, Down;
        readonly Control ItemControl;
        public Gui_CraftingOrderContainer(CraftingOrder s, Action<CraftingOrder> moveUp, Action<CraftingOrder> modeDown)
        {
            this.Up = new ButtonIcon(Icon.ArrowUp, () => moveUp(s));
            this.Down = new ButtonIcon(Icon.ArrowDown, () => modeDown(s));
            this.ItemControl = s.GetListControlGui();
            this.AddControlsVertically(this.Up, this.Down)
                .AddControlsTopRight(this.ItemControl);
        }
        public override void OnLayout(int availableWidth, int availableHeight)
        {
            this.Width = availableWidth;
            this.ItemControl.OnLayout(this.Width - this.Up.Width, this.Height);
        }
    }
    void Build(BlockWorkstationComp workstation)
    {
        this.Workstation = workstation;
        var btnAddOrder = new Button("Add Order", this.OnAddOrderClick);

        this.PanelReactions = new Panel() { AutoSize = true };
        this.PanelReactions.HideOnAnyClick();
        var manager = workstation.Parent.Map.Town.CraftingManager;
        //var availableRefinementsControl = new ListBoxNoScroll<AddOrderRequest>(r => new Label(r.GetLabel(), () => this.PlaceOrderNew(r)));
        var availableRecipesNew = workstation.WorkstationType.Capabilities.SelectMany(cap => cap.Worker.GetAddOrderRequests(workstation));
        var availableRefinementsControl =
            new ListBoxNoScroll<AddOrderRequest>(r =>
                new LabelNew(r.GetLabel(), () => this.PlaceOrderNew(r))
                {
                    TextColorFunc = () => r.IsAvailable(workstation).Result ? UIManager.DefaultTextColor : Microsoft.Xna.Framework.Color.Red,
                    HoverFunc = () => r.IsAvailable(workstation).Message,
                });
        availableRefinementsControl.AddItems(availableRecipesNew);
        var reactionsListContainer = availableRefinementsControl.ToScrollableBox(200, 400);
        this.PanelReactions.AddControls(reactionsListContainer);

        var scrollableContainer = new ScrollableBoxNewNewNew(300, 400, ScrollModes.Vertical);
        scrollableContainer.AddControls(this.ListOrdersNew);

        var panell = scrollableContainer.ToPanelLabeled("Orders");

        this.ListOrdersNew.Clear();
        this.ListOrdersNew.AddItems(workstation.Orders);

        UpdateArrows();

        var map = workstation.Parent.Map;
        var zonemanager = map.Town.ZoneManager;
        var stockpiles = zonemanager.GetZones<Stockpile>().Prepend(null);

        this.IOTable = new Table<(string label, Func<ZoneId> zoneIdGetter, WorkstationIOType iotype)>()
            .AddColumn("iotype", 100, item => new LabelNew(item.label), anchorX: 1)
            .AddColumn("control", 200, item => new ComboBoxFinal<Stockpile>(stockpiles, 200, s => s?.Name ?? "-None-", s => select(item.iotype, s), () => zonemanager.GetZone<Stockpile>(item.zoneIdGetter())));

        this.IOTable.AddItems([
            ("Input", ()=>workstation.Input, WorkstationIOType.Input),
            ("Output", ()=>workstation.Output, WorkstationIOType.Output)
            ]);
        var linkedStockpiledPanel = this.IOTable.ToPanelLabeled("Linked Stockpiles");

        void select(WorkstationIOType iotype, Stockpile stockpile) =>
            Ingame.Instance.Events.Post(new PlayerSetWorkstationZoneEvent(workstation, iotype, stockpile));
        this.Controls.Clear();
        this.AddControls(
            panell,
            btnAddOrder, linkedStockpiledPanel
            );
        this.AlignTopToBottom();

        var mapEvents = this.Workstation.Parent.Map.Events;
        mapEvents.ListenTo<CellsInvalidatedEvent>(OnBlocksUpdated);
        mapEvents.ListenTo<CraftOrderAddedEvent>(OnCraftOrderAdded);
        mapEvents.ListenTo<CraftOrderRemovedEvent>(OnCraftOrderRemoved);
        mapEvents.ListenTo<CraftOrderReorderedEvent>(OnOrderReordered);

        mapEvents.ListenTo<WorkstationUpdatedEvent>(OnWorkstationUpdated);
    }

    private void OnWorkstationUpdated(WorkstationUpdatedEvent e)
    {
        if (e.Comp != this.Workstation)
            return;
        this.IOTable.Invalidate(true);
    }

    private void OnOrderReordered(CraftOrderReorderedEvent e)
    {
        if (e.Order.Workstation != this.Workstation)
            return;
        var newindex = e.Order.Workstation.Orders.IndexOf(e.Order);
        this.ListOrdersNew.Move(e.Order, newindex);
        UpdateArrows();

    }
    private void MoveDown(CraftingOrder s)
    {
        // local ui prediction
        var newindex = s.Workstation.Orders.IndexOf(s) + 1;
        this.ListOrdersNew.Move(s, newindex);
        UpdateArrows();
        Packets_Crafting.SendPlayerModifiedOrder(s.Workstation.Parent.Map, s, 1, 0, s.Mode);
    }

    private void MoveUp(CraftingOrder s)
    {
        // local ui prediction
        var newindex = s.Workstation.Orders.IndexOf(s) - 1;
        this.ListOrdersNew.Move(s, newindex);
        UpdateArrows();
        Packets_Crafting.SendPlayerModifiedOrder(s.Workstation.Parent.Map, s, -1, 0, s.Mode);
    }
    void UpdateArrows()
    {
        if (this.ListOrdersNew.Count == 0)
            return;
        this.ListOrdersNew[0].Up.RemoveFromParent();
        this.ListOrdersNew[this.ListOrdersNew.Count - 1].Down.RemoveFromParent();
        if (this.ListOrdersNew.Count > 1)
        {
            this.ListOrdersNew[0].Down.AddToParent();
            this.ListOrdersNew[this.ListOrdersNew.Count - 1].Up.AddToParent();
        }
        for (int i = 1; i < this.ListOrdersNew.Count - 1; i++)
        {
            this.ListOrdersNew[i].Up.AddToParent();
            this.ListOrdersNew[i].Down.AddToParent();
        }
    }
    private void OnCraftOrderRemoved(CraftOrderRemovedEvent e)
    {
        if (e.Comp != this.Workstation)
            return;
        this.ListOrdersNew.RemoveItems(e.Order);
        UpdateArrows();
    }

    public override bool Show()
    {
        this.Workstation.Map.Events.ListenTo<CraftOrderAddedEvent>(OnCraftOrderAdded);
        return base.Show();
    }

    private void OnCraftOrderAdded(CraftOrderAddedEvent e)
    {
        if (this.Workstation != e.Comp)
            return;
        this.ListOrdersNew.AddItems(e.Order);
        Gui_CraftingOrderContainer cntr = this.ListOrdersNew.GetControlFor(e.Order);
        UpdateArrows();

    }
    private void OnAddOrderClick()
    {
        this.PanelReactions.SnapToMouse();
        this.PanelReactions.Show();
    }
   
    private void PlaceOrderNew(AddOrderRequest orderRequest)
    {
        if (!orderRequest.IsAvailable(this.Workstation).Result)
            return;
        this.PanelReactions.Hide();
        Ingame.Instance.Events.Post(new PlayerIssuedCraftOrderEventNew(this.Workstation, orderRequest));
    }
    void OnBlocksUpdated(CellsInvalidatedEvent e)
    {
        if (e.Positions.Contains(this.Workstation.Global))
            this.GetWindow().Hide();
    }

    protected internal override void OnBind(ISelectable selectable)
    {
        if (selectable is BlockEntity bEntity &&
            bEntity.TryGetComp<BlockWorkstationComp>(out var comp))
            this.Build(comp);
        else
            this.Window.Hide();
    }

    protected override void RegisterInvalidations()
    {
        if (this.CurrentSelection is not BlockEntity entity)
            return;
        this.InvalidateOn<BlockEntityRemovedEvent>(e => e.Entity == entity);
    }
}

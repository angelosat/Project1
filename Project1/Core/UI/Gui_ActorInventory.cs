using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Input;
using Project1.Core.Screens;
using Project1.Core.Systems.Inventory;
using Project1.Core.Towns.Storage;
using Project1.Framework.UI;

namespace Project1.Core.UI;

internal sealed class Gui_ActorInventory : Table<Entity>
{
    Actor Actor;

    ContainerList Container;

    public Gui_ActorInventory()
    {
        this
                //.AddColumn("name", 164, o => new Label(() => o.Name, () => Inspector.Refresh(o)) { TooltipFunc = o.GetInventoryTooltip })
                //.AddColumn("name", 164, o => new Label(() => o.Name, () => Inspector.Refresh(o)) { TooltipFunc = o.GetTooltipInfo })
                .AddColumn("name", 164, o => new LabelNew(() => o.Name, () => Inspector.Refresh(o)) { TooltipTargetFunc = () => o })
                .AddColumn("preference", 92, o => this.Actor.ItemPreferences.GetListControl(o), .5f)
                .AddColumn("weight", 32, o => new LabelNew(() => o.TotalWeight.ToString("0.# kg")), 1f)
                .AddColumn("drop", Icon.Cross.Width, o => IconButton.CreateSmall(Icon.Cross, () => drop(o), "Drop").ShowOnParentFocus(true));
        this.RowFunc = (o, row) => row.InvalidateOn(o.StackNotifier);
        void drop(GameObject o)
        {
            if (o == this.Actor)
                if (this.Actor.IsSpawned && this.Actor.IsTownMember)
                    Ingame.Instance.Events.Post(new PlayerForcedDropInventoryItemEvent(this.Actor, o as Entity, o.StackSize));
        }
    }

    public void Build(BlockEntity block)
    {
        var inve = block.GetComp<BlockInventoryComp>();
    }

    public void Build(Actor actor)
    {
        this.Actor = actor;
        this.Container = actor.Inventory.Contents;
        this.ClearControls();
        this.AddItems(this.Container);
        this.Container.ItemAdded += Container_ItemAdded;
        this.Container.ItemRemoved += Container_ItemRemoved;
    }
    
    protected override void OnDetached()
    {
        this.Container.ItemAdded -= Container_ItemAdded;
        this.Container.ItemRemoved -= Container_ItemRemoved;
    }

    private void Container_ItemAdded(Entity obj) => this.AddItem(obj);

    private void Container_ItemRemoved(Entity obj) => this.RemoveItem(obj);
}

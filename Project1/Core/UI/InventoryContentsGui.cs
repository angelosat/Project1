using Project1.Core.Entities;
using Project1.Core.Inventory;
using Project1.Core;
using Project1.Core.Entities.Actors;
using Project1.Core.Screens;
using Project1.Core.UI;

namespace Project1.Core.UI
{
    internal class InventoryContentsGui : GroupBox
    {
        Table<Entity> TableContents;
        Actor Actor;
        ContainerList Container;
        public InventoryContentsGui()
        {
            this.TableContents = new Table<Entity>()
                    .AddColumn("name", 96, o => new Label(() => o.Name, () => Inspector.Refresh(o)) { TooltipFunc = o.GetInventoryTooltip })
                    .AddColumn("preference", 96, o => this.Actor.ItemPreferences.GetListControl(o))
                    .AddColumn("weight", 32, o => new Label(() => o.TotalWeight.ToString("0.# kg")))
                    .AddColumn("drop", Icon.Cross.Width, o => IconButton.CreateSmall(Icon.Cross, () => drop(o), "Drop").ShowOnParentFocus(true));
            void drop(GameObject o)
            {
                if (this.Actor.IsSpawned && this.Actor.IsTownMember)
                    Ingame.Instance.Events.Post(new PlayerForcedDropInventoryItemEvent(this.Actor, o as Entity, o.StackSize));
            }
        }
        public void Build(Actor actor)
        {
            this.Actor = actor;
            this.Container = actor.Inventory.Contents;
            this.TableContents.ClearControls();
            this.TableContents.AddItems(this.Container);
            this.Container.ItemAdded += Container_ItemAdded;
            this.Container.ItemRemoved += Container_ItemRemoved;
            this.ClearControls();
            this.AddControls(this.TableContents);
        }
        protected override void OnHidden()
        {
            this.Container.ItemAdded -= Container_ItemAdded;
            this.Container.ItemRemoved -= Container_ItemRemoved;
            base.OnHidden();
        }
        
        private void Container_ItemAdded(Entity obj) => this.TableContents.AddItem(obj);
        private void Container_ItemRemoved(Entity obj) => this.TableContents.RemoveItem(obj);
    }
}

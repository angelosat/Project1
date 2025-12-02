using Start_a_Town_.Components;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class EquipmentUI : ScrollableBoxNewNew
    {
        readonly Panel PanelList;
        readonly ListBoxNoScroll<Entity, Button> GearList;
        const int MaxRows = 8;
        Actor Actor;
        public EquipmentUI()
            : base(200, Button.DefaultHeight * MaxRows)
        {
            this.PanelList = new Panel() { AutoSize = true };
            this.GearList = new ListBoxNoScroll<Entity, Button>(o =>
            {
                var btn = ButtonHelper.CreateFromItemCompact(o);
                btn.LeftClickAction = () => ContextMenuManager.PopUp(new ContextAction(() => "Unequip", () => PacketInventoryEquip.Send(this.Actor.Net, this.Actor.RefId, o.RefId)));
                return btn;
            });
            this.PanelList.AddControls(this.GearList);
            this.AddControls(this.PanelList);
        }

        public void Refresh(Actor actor)
        {
            this.Actor = actor;
            this.GearList.Clear();
            if (actor == null)
                return;
            var gear = actor.GetGear();
            this.GearList.Clear();
            this.GearList.AddItems(gear);
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Message.Types.ActorGearUpdated:
                    var actor = e.Parameters[0] as Actor;
                    if (actor == this.Actor)
                    {
                        var newItem = e.Parameters[1] as Entity;
                        var prevItem = e.Parameters[2] as Entity;
                        this.GearList.AddItems(newItem);
                        this.GearList.RemoveItems(prevItem);
                    }
                    break;

                default:
                    break;
            }
        }
    }
}

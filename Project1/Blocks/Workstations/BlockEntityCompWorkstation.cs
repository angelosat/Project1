using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
namespace Start_a_Town_
{
    public class BlockEntityCompWorkstation : BlockEntityComp
    {
        public BlockEntityCompWorkstation(WorkstationDef def)
        {
            this.Type = def;
        }
        public override string Name => "WorkstationComp";
        public WorkstationDef Type;
        public List<OrderSettings> Orders = [];
        //public ObservableCollection<OrderSettings> Orders = [];

        internal override void GetQuickButtons(SelectionManager uISelectedInfo, MapBase map, IntVec3 vector3)
        {
            uISelectedInfo.AddTabAction("Orders", this.ShowUI);
        }

        public void ShowUI()
        {
            UIManager.ToggleUnique<WorkstationGuiNew>(new TargetArgs(this.Parent.Map, this.Parent.OriginGlobal));
        }

        internal void MoveUp(OrderSettings orderSettings)
        {
            var currentIndex = this.Orders.IndexOf(orderSettings);
            if (currentIndex == 0)
                return;
            this.Orders.RemoveAt(currentIndex);
            this.Orders.Insert(currentIndex - 1, orderSettings);
            this.Map.Events.Post(new CraftOrderReorderedEvent(orderSettings));
        }

        internal void MoveDown(OrderSettings orderSettings)
        {
            var currentIndex = this.Orders.IndexOf(orderSettings);
            if (currentIndex == this.Orders.Count - 1)
                return;
            this.Orders.RemoveAt(currentIndex);
            this.Orders.Insert(currentIndex + 1, orderSettings);
            this.Map.Events.Post(new CraftOrderReorderedEvent(orderSettings));
        }
    }
}

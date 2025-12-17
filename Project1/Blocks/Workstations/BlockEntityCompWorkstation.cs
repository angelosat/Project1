using Start_a_Town_.UI;
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
    }
}

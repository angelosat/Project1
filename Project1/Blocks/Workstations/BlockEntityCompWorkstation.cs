using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
namespace Start_a_Town_
{
    internal class BlockEntityCompWorkstation : BlockEntityComp
    {
        public BlockEntityCompWorkstation(WorkstationDef def)
        {
            this.Type = def;
        }
        public override string Name => "WorkstationComp";
        public WorkstationDef Type;
        public List<CraftOrderNew> Orders = [];

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

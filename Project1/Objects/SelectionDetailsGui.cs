using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class SelectionDetailsGui : GroupBox
    {
        ISelectable CurrentSelection;
        IEnumerable<Control> Contents;
        PanelWithVerticalTabs<Label> PanelMain;
        public SelectionDetailsGui()
        {
            this.PanelMain = new();
            this.AddControls(this.PanelMain);
        }
        public SelectionDetailsGui Refresh(ISelectable target)
        {
            this.CurrentSelection = target;
            this.Contents = target.GetSelectionDetails();
            this.PanelMain.InitTabs(this.Contents.ToArray());
            this.Validate(true);
            return this;
        }
        internal override void OnSelectedTargetChanged(TargetArgs target)
        {
            this.Refresh(target);
            base.OnSelectedTargetChanged(target);   
        }
    }
}

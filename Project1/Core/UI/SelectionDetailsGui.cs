using System.Collections.Generic;
using System.Linq;
using Project1.Framework.UI;

namespace Project1.Core.UI
{
    public class SelectionDetailsGui : GroupBox
    {
        IEnumerable<Control> Contents;
        PanelWithVerticalTabs<Label> PanelMain;
        public SelectionDetailsGui()
        {
            this.PanelMain = new();
            this.AddControls(this.PanelMain);
        }
        public SelectionDetailsGui Refresh(ISelectable target)
        {
            this.Contents = target.GetSelectionDetails();
            this.PanelMain.InitTabs(this.Contents.ToArray());
            this.Validate(true);
            return this;
        }
        internal override void OnSelectedTargetChanged(ISelectable target)
        {
            this.Refresh(target);
            base.OnSelectedTargetChanged(target);   
        }
    }
}

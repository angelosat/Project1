using System.Collections.Generic;
using System.Linq;
using Project1.Framework.Base;
using Project1.Framework.UI;
using Start_a_Town_;

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
        internal override void OnSelectedTargetChanged(TargetArgs target)
        {
            this.Refresh(target);
            base.OnSelectedTargetChanged(target);   
        }
    }
}

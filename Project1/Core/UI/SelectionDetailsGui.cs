using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.UI;

public class SelectionDetailsGui : SelectionBoundControl
{
    IEnumerable<Control> Contents;
    PanelWithVerticalTabs<Label> PanelMain;
    public SelectionDetailsGui()
    {
        this.PanelMain = new();
        this.AddControls(this.PanelMain);
    }

    protected internal override void OnBind(ISelectable selectable)
    {
        //this.Contents = selectable.GetSelectionDetails();
        //foreach(var (label, type) in selectable.GetSelectionTabs())
        //{
        //    var ctrl = Activator.CreateInstance(type) as SelectionBoundControl;
        //    ctrl.Name = label;
        //}
        this.Contents = selectable.GetInspectorTabs().Select(c => {
            var ctrl = Activator.CreateInstance(c.type) as SelectionBoundControl;
            ctrl.Name = c.label;
            ctrl.Bind(selectable);
            return ctrl;
            });
        this.PanelMain.InitTabs([.. this.Contents]);
        this.Validate();
    }
}
//public class SelectionDetailsGui : GroupBox
//{
//    IEnumerable<Control> Contents;
//    PanelWithVerticalTabs<Label> PanelMain;
//    public SelectionDetailsGui()
//    {
//        this.PanelMain = new();
//        this.AddControls(this.PanelMain);
//    }
//    public SelectionDetailsGui Refresh(ISelectable target)
//    {
//        this.Contents = target.GetSelectionDetails();
//        this.PanelMain.InitTabs(this.Contents.ToArray());
//        this.Validate(true);
//        return this;
//    }
//    internal override void OnSelectedTargetChanged(ISelectable target)
//    {
//        this.Refresh(target);
//        base.OnSelectedTargetChanged(target);   
//    }
//}

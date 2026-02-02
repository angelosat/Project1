using Microsoft.Xna.Framework;
using System;

namespace Start_a_Town_.UI
{
    [Obsolete]
    class PanelTabs : GroupBox
    {
        Panel PanelClient;
        GroupBox BoxTabs, Selected;
        public PanelTabs(int w, int h)
        {
            this.BoxTabs = new GroupBox()
            {
                AutoSize = false,
                Size = new Rectangle(0, 0, w, Button.DefaultHeight)
            };
            this.PanelClient = new Panel(new Rectangle(0, 0, w, h - this.BoxTabs.Height)) { Location = this.BoxTabs.BottomLeft };
            this.AddControls(this.BoxTabs, this.PanelClient);
        }
        public PanelTabs AddTab(string label, GroupBox tab, out Button btn)
        {
            btn = new Button(label) { Tag = tab, LeftClickAction = () => ShowTab(tab) };
            this.BoxTabs.AddControlsTopRight(btn);
            return this;
        }
        public PanelTabs AddTab(string label, GroupBox tab)
        {
            return this.AddTab(label, tab, out Button btn);
        }
        public PanelTabs RemoveTab(Button btn)
        {
            this.BoxTabs.RemoveControls(btn);
            this.BoxTabs.AlignLeftToRight();
            return this;
        }
        private void ShowTab(GroupBox tab)
        {
            if (tab == this.Selected)
                return;
            this.PanelClient.ClearControls();
            this.PanelClient.AddControls(tab);
            this.Selected = tab;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class PanelWithTabs : GroupBox
    {
        public PanelWithTabs(int w, int h, IEnumerable<Control> namedControls)
        {
            Panel panel = null;
            ScrollableBoxTest panelClient = null;
            var groupboxTabs = UIHelper.Wrap(namedControls.Select(tab => new Button(tab.Name, () => selectTab(tab))), w);
            panelClient = new ScrollableBoxTest(w + ScrollbarV.DefaultWidth, h - groupboxTabs.Height, ScrollModes.Vertical);
            panel = new() { AutoSize = true };
            panel.AddControls(panelClient);
            Control selectedTab = null;

            var tabs = UIHelper.Wrap(namedControls.Select(tab => new Button(tab.Name, () => selectTab(tab))), w);

            selectTab(namedControls.First());

            this.AddControlsVertically(//0, Alignment.Horizontal.Right,
                tabs,
                panel);
           
            void selectTab(Control tab)
            {
                selectedTab = tab;
                panelClient.ClearControls();
                panelClient.AddControls(tab);
            }
        }
    }
    public class PanelWithVerticalTabs : GroupBox
    {
        public static readonly int DefaultSize = 256, DefaultTabsListSize = 96; //128
        Control SelectedTab;
        readonly ScrollableBoxTest PanelClient;
        readonly ListBoxNoScroll<Control> TabsList;
        public PanelWithVerticalTabs() : this(DefaultSize, DefaultSize)
        {
            
        }
        public PanelWithVerticalTabs(int w, int h)
        {
            this.TabsList = new(c => new Button(c.Name, () => SelectTab(c), DefaultTabsListSize) { IsToggledFunc = () => this.SelectedTab == c });
            Panel panel = null, panelTabs;
            ScrollableBoxTest 
                panelClientTabs = null;
            panelClientTabs = new ScrollableBoxTest(DefaultTabsListSize + ScrollbarV.DefaultWidth, h, ScrollModes.Vertical);
            panelClientTabs.AddControls(this.TabsList);
            this.PanelClient = new ScrollableBoxTest(w + ScrollbarV.DefaultWidth, h, ScrollModes.Vertical);
            panel = new() { AutoSize = true };
            panelTabs = new() { AutoSize = true };
            panel.AddControls(this.PanelClient);
            panelTabs.AddControls(panelClientTabs);

            this.AddControlsHorizontally(//0, Alignment.Horizontal.Right,
                panelTabs,
                panel);
        }
        public PanelWithVerticalTabs InitTabs(params Control[] namedControls)
        {
            this.TabsList.Clear();
            this.TabsList.AddItems(namedControls);
            if (!namedControls.Contains(this.SelectedTab))
                SelectTab(namedControls.FirstOrDefault());
            return this;
        }
        void SelectTab(Control tab)
        {
            SelectedTab = tab;
            this.PanelClient.ClearControls();
            if(tab != null) 
                this.PanelClient.AddControls(tab);
        }
        
    }

    public class PanelWithVerticalTabs<T> : GroupBox where T : ButtonBase, new()
    {
        public static readonly int DefaultSize = 256;
        Control SelectedTab;
        readonly ScrollableBoxTest PanelClient;
        readonly ListBoxNoScroll<Control> TabsList;
        public PanelWithVerticalTabs() : this(DefaultSize, DefaultSize)
        {

        }
        public PanelWithVerticalTabs(int w, int h)
        {
            //this.TabsList = new(c => new Button(c.Name, () => SelectTab(c), 128) { IsToggledFunc = () => this.SelectedTab == c });
            this.TabsList = new(c => new T() { Text = c.Name, Active = true, LeftClickAction = () => SelectTab(c), Width = 128, IsToggledFunc = () => this.SelectedTab == c });
            Panel panel = null, panelTabs;
            ScrollableBoxTest
                panelClientTabs = null;
            panelClientTabs = new ScrollableBoxTest(128 + ScrollbarV.DefaultWidth, h, ScrollModes.Vertical);
            panelClientTabs.AddControls(this.TabsList);
            this.PanelClient = new ScrollableBoxTest(w + ScrollbarV.DefaultWidth, h, ScrollModes.Vertical);
            panel = new() { AutoSize = true };
            panelTabs = new() { AutoSize = true };
            panel.AddControls(this.PanelClient);
            panelTabs.AddControls(panelClientTabs);

            this.AddControlsHorizontally(//0, Alignment.Horizontal.Right,
                panelTabs,
                panel);
        }
        public PanelWithVerticalTabs<T> InitTabs(params Control[] namedControls)
        {
            this.TabsList.Clear();
            this.TabsList.AddItems(namedControls);
            if (!namedControls.Contains(this.SelectedTab))
                SelectTab(namedControls.FirstOrDefault());
            return this;
        }
        void SelectTab(Control tab)
        {
            SelectedTab = tab;
            this.PanelClient.ClearControls();
            if (tab != null)
                this.PanelClient.AddControls(tab);
        }

    }
}
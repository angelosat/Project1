using System;
using System.Collections.Generic;
using Start_a_Town_.UI;

namespace Start_a_Town_.Core
{
    class GrowingZoneUI : GroupBox
    {
        static Dictionary<GrowingZone, Window> OpenWindows = new Dictionary<GrowingZone, Window>();

        GrowingZone Farmland;
        Panel PanelSeeds, PanelButtons;
        GameObjectSlot Seed;
        CheckBoxNew ChkHarvesting, ChkPlanting;

        public GrowingZoneUI(GrowingZone farmland)
        {
            this.Farmland = farmland;
            this.PanelSeeds = new Panel() { AutoSize = true };

            this.Seed = GameObjectSlot.Empty;
            var slotseed = new Slot() { Tag = this.Seed };
            
            slotseed.LeftClickAction = SelectSeed;
            this.PanelSeeds.AddControls(slotseed);

            var paneloptions = new Panel() { Location = this.PanelSeeds.TopRight, AutoSize = true };
            this.ChkHarvesting = new CheckBoxNew("Harvesting");
            this.ChkHarvesting.TickedFunc = () => this.Farmland.Harvesting;
            this.ChkHarvesting.LeftClickAction = () => SetHarvesting(this.Farmland, !this.ChkHarvesting.Value);

            this.ChkPlanting = new CheckBoxNew("Planting") { Location = this.ChkHarvesting.BottomLeft };
            this.ChkPlanting.TickedFunc = () => this.Farmland.Planting;
            this.ChkPlanting.LeftClickAction = () => SetPlanting(this.Farmland, !this.ChkPlanting.Value);

            paneloptions.AddControls(this.ChkHarvesting, this.ChkPlanting);

            this.PanelButtons = new Panel(paneloptions.BottomLeft) { AutoSize = true };

            this.Controls.Add(this.PanelButtons, this.PanelSeeds, paneloptions);
        }

        private void SetPlanting(GrowingZone farm, bool p)
        {
            throw new Exception();
        }

        private void SetHarvesting(GrowingZone farm, bool p)
        {
            throw new Exception();
        }

        private void Rename(DialogInput dialog)
        {
            throw new Exception();
        }

        private void SelectSeed()
        {
        }

        private Window ToWindow(string name = "")
        {
            if (OpenWindows.TryGetValue(this.Farmland, out Window existing))
                return existing;

            var win = base.ToWindow(name);
            win.AutoSize = true;
            win.Movable = true;
            win.Client.Controls.Add(this);


            var dialogRename = new DialogInput("Rename " + this.Farmland.Name, Rename, 16, this.Farmland.Name);
            win.Label_Title.LeftClickAction = () => dialogRename.ShowDialog();
            win.Label_Title.MouseThrough = false;
            win.Label_Title.Active = true;

            win.HideAction = () => OpenWindows.Remove(this.Farmland);
            win.ShowAction = () => OpenWindows[this.Farmland] = win;

            return win;
        }

        internal static Window GetWindow(GrowingZone farmland)
        {
            if (OpenWindows.TryGetValue(farmland, out Window existing))
                return existing;
            return new GrowingZoneUI(farmland).ToWindow(farmland.Name);
        }
    }
}

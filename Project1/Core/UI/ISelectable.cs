using Microsoft.Xna.Framework;
using Project1.Core.Simulation;
using Project1.Core.UI.Hud;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;

namespace Project1.Core.UI
{
    public interface ISelectable
    {
        string Name { get; }
        void GetSelectionInfo(IUISelection panel);
        void GetSelectionInfo(SelectionManager info);
        IEnumerable<(string name, Action action)> GetInfoTabs();
        IEnumerable<Control> GetSelectionDetails();
        void GetQuickButtons(SelectionManager panel);
        bool Exists { get; }
        Vector3 Global { get; }
        MapBase Map { get; }
        IEnumerable<(string Label, Type GuiType)> GetTabs() { yield break; }
    }
}

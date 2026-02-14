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
        void TabGetter(Action<string, Action> getter);
        IEnumerable<(string Label, Type GuiType)> GetTabs() { yield break; }
    }
}

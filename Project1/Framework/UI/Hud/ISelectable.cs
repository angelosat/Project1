using System;
using System.Collections.Generic;
using Project1.Core.Interfaces;
using Project1.Core.UI;
using Project1.Core.UI;

namespace Project1.Core
{
    public interface ISelectable
    {
        string Name { get; }
        //string GetName();
        void GetSelectionInfo(IUISelection panel);
        void GetSelectionInfo(SelectionManager info);
        IEnumerable<(string name, Action action)> GetInfoTabs();
        IEnumerable<Control> GetSelectionDetails();
        void GetQuickButtons(SelectionManager panel);
        bool Exists { get; }
        void TabGetter(Action<string, Action> getter);
        //IEnumerable<(string Label, T GuiType)> GetTabs<T>() where T : ISelectionBound { yield break; }
        IEnumerable<(string Label, Type GuiType)> GetTabs() { yield break; }
    }
}

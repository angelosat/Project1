using System;
using System.Collections.Generic;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public interface ISelectable
    {
        string GetName();
        void GetSelectionInfo(IUISelection panel);
        void GetSelectionInfo(SelectionManagerNew info);
        IEnumerable<(string name, Action action)> GetInfoTabs();
        IEnumerable<Control> GetSelectionDetails();
        void GetQuickButtons(SelectionManager panel);
        void GetQuickButtons(SelectionManagerNew panel);
        bool Exists { get; }
        void TabGetter(Action<string, Action> getter);
    }
}

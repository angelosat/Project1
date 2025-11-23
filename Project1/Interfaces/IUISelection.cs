using System;

namespace Start_a_Town_.UI
{
    public interface IUISelection
    {
        void AddInfo(Control control);
        void AddTabAction(string label, Action action);
        void AddIcon(IconButton button);
    }
}

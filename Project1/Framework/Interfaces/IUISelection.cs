using Project1.Framework.UI;
using Start_a_Town_.UI;
using System;

namespace Project1.Framework.Interfaces
{
    public interface IUISelection
    {
        void AddInfo(Control control);
        void AddTabAction(string label, Action action);
        void AddIcon(IconButton button);
    }
}

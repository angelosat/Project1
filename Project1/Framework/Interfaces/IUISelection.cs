using Project1.Core.UI;
using Project1.Core.UI;
using System;

namespace Project1.Core.Interfaces
{
    public interface IUISelection
    {
        void AddInfo(Control control);
        void AddTabAction(string label, Action action);
        void AddIcon(IconButton button);
    }
}

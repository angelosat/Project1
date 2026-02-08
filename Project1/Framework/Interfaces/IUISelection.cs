using System;

namespace Project1.Framework.UI
{
    public interface IUISelection
    {
        void AddInfo(Control control);
        void AddTabAction(string label, Action action);
        void AddIcon(IconButton button);
    }
}

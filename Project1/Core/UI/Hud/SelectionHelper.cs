using Project1.Core.Input;
using Project1.Framework.Input;
using System.Windows.Forms;

namespace Project1.Core.UI.Hud
{
    static class SelectionHelper
    {
        public static SelectionOp GetSelectionOp()
        {
            if (InputState.IsKeyDown(Keys.LShiftKey))
                return SelectionOp.Add;
            else if (InputState.IsKeyDown(Keys.LControlKey))
                return SelectionOp.Remove;
            return default;
        }
    }
}

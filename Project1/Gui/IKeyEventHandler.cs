using System.Windows.Forms;

namespace Start_a_Town_
{
    public interface IKeyEventHandler
    {
        void HandleKeyPress(KeyPressEventArgs e);
        void HandleKeyDown(KeyEventArgs e);
        void HandleKeyUp(KeyEventArgs e);
        void HandleMouseMove(HandledMouseEventArgs e);
        void HandleLButtonDown(HandledMouseEventArgs e);
        void HandleLButtonUp(HandledMouseEventArgs e);
        void HandleRButtonDown(HandledMouseEventArgs e);
        void HandleRButtonUp(HandledMouseEventArgs e);
        void HandleMiddleUp(HandledMouseEventArgs e);
        void HandleMiddleDown(HandledMouseEventArgs e);
        void HandleMouseWheel(HandledMouseEventArgs e);
        void HandleLButtonDoubleClick(HandledMouseEventArgs e);
    }
}

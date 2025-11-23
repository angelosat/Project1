using System.Windows.Forms;

namespace Start_a_Town_
{
    public class InputHandler
    {
        public virtual void HandleKeyPress(System.Windows.Forms.KeyPressEventArgs e) { }
        public virtual void HandleKeyDown(System.Windows.Forms.KeyEventArgs e) { }
        public virtual void HandleKeyUp(System.Windows.Forms.KeyEventArgs e) { }
        public virtual void HandleMouseMove(System.Windows.Forms.HandledMouseEventArgs e) { }
        public virtual void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e) { }
        public virtual void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e) { }
        public virtual void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e) { }
        public virtual void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e) { }
        public virtual void HandleMiddleUp(System.Windows.Forms.HandledMouseEventArgs e) { }
        public virtual void HandleMiddleDown(System.Windows.Forms.HandledMouseEventArgs e) { }
        public virtual void HandleMouseWheel(HandledMouseEventArgs e) { }
        public virtual void HandleLButtonDoubleClick(HandledMouseEventArgs e) { }
    }
}

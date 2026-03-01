using Project1.Framework.Input;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Project1.Core.Input
{
    internal class InputRouter
    {
        readonly Stack<IInputEventHandler> Handlers = [];

        internal void Add(IInputEventHandler handler)
        {
            this.Handlers.Push(handler);
        }

        internal virtual void HandleKeyDown(KeyEventArgs e)
        {
            foreach (var handler in this.Handlers)
                handler.HandleKeyDown(e);
        }

        internal virtual void HandleKeyPress(KeyPressEventArgs e)
        {
            foreach (var handler in this.Handlers)
                if (!e.Handled)
                    handler.HandleKeyPress(e);
        }

        internal virtual void HandleKeyUp(KeyEventArgs e)
        {
            foreach (var handler in this.Handlers)
                handler.HandleKeyUp(e);
        }

        internal virtual void HandleMouseMove(HandledMouseEventArgs e)
        {
            foreach (var handler in this.Handlers)
                handler.HandleMouseMove(e);
        }

        internal virtual void HandleLButtonDown(HandledMouseEventArgs e)
        {
            foreach (var handler in this.Handlers)
                handler.HandleLButtonDown(e);
        }

        internal virtual void HandleLButtonUp(HandledMouseEventArgs e)
        {
            foreach (var handler in this.Handlers)
                handler.HandleLButtonUp(e);
        }

        internal virtual void HandleRButtonDown(HandledMouseEventArgs e)
        {
            foreach (var handler in this.Handlers)
                handler.HandleRButtonDown(e);
        }

        internal virtual void HandleRButtonUp(HandledMouseEventArgs e)
        {
            foreach (var handler in this.Handlers)
                handler.HandleRButtonUp(e);
        }

        internal virtual void HandleMiddleUp(HandledMouseEventArgs e)
        {
            foreach (var handler in this.Handlers)
                handler.HandleMiddleUp(e);
        }

        internal virtual void HandleMiddleDown(HandledMouseEventArgs e)
        {
            foreach (var handler in this.Handlers)
                handler.HandleMiddleDown(e);
        }

        internal virtual void HandleMouseWheel(HandledMouseEventArgs e)
        {
            foreach (var handler in this.Handlers)
                if (!e.Handled)
                    handler.HandleMouseWheel(e);
        }
        internal void HandleLButtonDoubleClick(HandledMouseEventArgs e)
        {
            foreach (var handler in this.Handlers)
                if (!e.Handled)
                    handler.HandleLButtonDoubleClick(e);
        }
    }
}

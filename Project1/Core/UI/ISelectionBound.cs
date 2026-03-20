using Project1.Framework.Events;
using Project1.Framework.UI;
using System;

namespace Project1.Core.UI
{
    public interface ISelectionBound
    {
        void OnBind(ISelectable selectable);
        public ISelectable CurrentSelection { get; set; }
    }

    public abstract class SelectionBoundControl : GroupBox
    {
        Action _unsub;
        public ISelectable CurrentSelection { get; set; }

        internal void Bind(ISelectable selectable)
        {
            this.CurrentSelection = selectable;

            _unsub?.Invoke();
            _unsub = null;

            this.OnBind(selectable);
            this.RegisterInvalidations();
        }

        protected void InvalidateOn<TEvent>(Func<TEvent, bool> predicate) where TEvent : IEventPayload
        {
            var unsub = CurrentSelection.Map.Events.ListenTo<TEvent>(e =>
            {
                if (predicate(e))
                {
                    _unsub?.Invoke();
                    Window.Hide();
                }
            });

            _unsub += unsub;
        }
        protected virtual void RegisterInvalidations() { }
        protected internal abstract void OnBind(ISelectable selectable);
    }
}

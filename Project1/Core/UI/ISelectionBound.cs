using Project1.Framework.UI;

namespace Project1.Core.UI
{
    public interface ISelectionBound
    {
        private void Bind(ISelectable selectable)
        {
            this.CurrentSelection = selectable;
            this.OnBind(selectable);
        }
        void OnBind(ISelectable selectable);
        public ISelectable CurrentSelection { get; set; }
    }

    public abstract class SelectionBoundControl : GroupBox
    {
        public ISelectable CurrentSelection { get; set; }
        internal void Bind(ISelectable selectable)
        {
            this.CurrentSelection = selectable;
            this.OnBind(selectable);
        }
        protected abstract void OnBind(ISelectable selectable);
    }
}

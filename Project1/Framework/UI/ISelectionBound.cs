namespace Project1.Core
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
}

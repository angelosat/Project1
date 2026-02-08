namespace Project1.Framework.UI
{
    public interface IGui
    {
        void NewGui(GroupBox box);

        public Control NewGui()
        {
            var box = new GroupBox();
            this.NewGui(box);
            return box;
        }
    }
}

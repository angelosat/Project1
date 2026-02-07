using Project1.Core.UI;

namespace Project1.Core.UI
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

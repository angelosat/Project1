using Project1.Framework.UI;

namespace Start_a_Town_.UI
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

using Project1.Core.UI;

namespace Project1.Core.UI
{
    public static class GuiExtensions
    {
        public static Control NewGui(this IGui gui)
        {
            var box = new GroupBox();
            gui.NewGui(box);
            return box;
        }
    }
}

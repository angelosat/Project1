namespace Project1.Framework.UI
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

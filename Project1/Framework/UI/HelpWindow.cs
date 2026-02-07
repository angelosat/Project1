using Project1.Core.UI;
using System;

namespace Project1.Core.UI
{
    class HelpWindow : Window
    {
        static HelpWindow _Instance;
        static public HelpWindow Instance => _Instance ??= new HelpWindow();

        public HelpWindow()
        {
            Title = "Help";
            AutoSize = true;

            Panel panel = new Panel();
            panel.AutoSize = true;
            Label text = new Label(text:
                "Left click to walk" +
            "\nRight click to interact" +
            "\nControl + Right click to dig" +
            "\nMousewheel to zoom in/out" +
            "\nControl + Mousewheel to raise/lower draw level" +
            "\nZ, X to rotate the map" +
            "\nH to hide walls and trees"
            );

            panel.Controls.Add(text);

            Controls.Add(panel);
            this.SnapToScreenCenter();
        }
    }
}

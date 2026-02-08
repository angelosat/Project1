using Project1.Framework.UI;

namespace Project1.Core.UI
{
    class TerrainWindow : Window
    {
        static TerrainWindow _Instance;
        public static TerrainWindow Instance => _Instance ??= new TerrainWindow();

        TerrainWindow()
        {
            this.Title = "Block Browser";
            this.AutoSize = true;
            this.Movable = true;
            this.Client.Controls.Add(new BlockBrowserEditor());
        }
    }
}

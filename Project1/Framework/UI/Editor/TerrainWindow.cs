namespace Start_a_Town_.UI.Editor
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

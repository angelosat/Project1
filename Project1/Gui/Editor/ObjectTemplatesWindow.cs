namespace Start_a_Town_.UI.Editor
{
    class ObjectTemplatesWindow : Window
    {
        static ObjectTemplatesWindow _Instance;
        public static ObjectTemplatesWindow Instance => _Instance ??= new ObjectTemplatesWindow();

        ObjectTemplatesWindow()
        {
            Title = "Objects";
            AutoSize = true;
            Movable = true;

            var items = GameObject.Templates;

            var list = new ListBoxNoScroll<int, Label>(id =>
            {
                var obj = items[id];
                return new Label(obj.Name, () => ScreenManager.CurrentScreen.ToolManager.ActiveTool = new ObjectSpawnToolFromTemplate(obj, id));
            });
            list.AddItems(items.Keys);
            var box = new ScrollableBoxTest(list, 200, 400, ScrollModes.Vertical) { SmallStep = Label.DefaultHeight + list.Spacing };
            //var box = new ScrollableBoxNewNew(200, 400, ScrollModes.Vertical);
            //box.AddControls(list);
            this.Client.AddControlsBottomLeft(new SearchBarNew<int>(200, id => items[id].Name).BindTo(list).ToPanelLabeled("Search"));
            this.Client.AddControlsBottomLeft(box.ToPanelLabeled("Templates"));
        }
    }
}

using Project1.Core.Entities;
using Project1.Core.Base;
using Project1.Core.Helpers;
using Project1.Core.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using Project1.Framework.UI;

namespace Project1.Core.UI
{
    static class Inspector
    {
        static Inspector()
        {
        }
        static Control DefDirectory, ObjectsDirectory;
        static readonly int MaxHeight = 512, Width = 384; // table columns TODO
        static Window WindowHelp;
        static readonly List<Control> History = new();
        static int HistoryIndex = -1;
        static IconButton BtnBack, BtnForward, BtnRefresh;
        static Control Container;

        public static void Refresh(Inspectable obj)
        {
            if (WindowHelp is null)
            {
                WindowHelp = new Window() { Title = "Details", Movable = true, Closable = true, AutoSize = true };//.MoveToScreenCenter() as Window;
                var toolbar = new GroupBox();
                BtnBack = IconButton.CreateSmall(Icon.ArrowLeft, Back).SetHoverText("Back") as IconButton;
                BtnForward = IconButton.CreateSmall(Icon.ArrowRight, Forward).SetHoverText("Forward") as IconButton;
                BtnRefresh = IconButton.CreateSmall(Icon.Replace, Refresh).SetHoverText("Refresh") as IconButton;

                toolbar.AddControlsHorizontally(
                    BtnBack,
                    BtnForward,
                    BtnRefresh,
                    new Button("Defs", () => ShowContainer(DefDirectory)),
                    new Button("Refs", () => ShowContainer(ObjectsDirectory))
                    );
                Container = new GroupBox();
                WindowHelp.AddControlsVertically(toolbar, Container);
                WindowHelp.HideAction = () =>
                {
                    History.Clear();
                    HistoryIndex = -1;
                };
                InitDefDirectory();
                InitRefDirectory();
            }
            Container.ClearControls();
            var table = new Table<(string item, object value)>()
                .AddColumn("name", Width / 3, i => new Label(i.item + ": "), 1)
                .AddColumn("value", 2 * Width / 3, i => new GroupBox().AddControlsLineWrap(256, Label.ParseNewNew(i.value).ToArray()));

            PopulateTable(obj, table);

            table.Tag = obj;
            var box = table.ToScrollableBox(table.Width, MaxHeight);
            var panel = box.ToPanel();
            panel.Tag = table;

            if (HistoryIndex < History.Count - 1)
                History.RemoveRange(HistoryIndex + 1, History.Count - HistoryIndex - 1);
            History.Add(panel);
            HistoryIndex++;

            Container.AddControls(panel);
            BtnBack.Active = HistoryIndex > 0;
            BtnForward.Active = HistoryIndex < History.Count - 1;
            BtnRefresh.Active = true;
            WindowHelp.Validate(true);
            Show();
        }

        private static void PopulateTable(Inspectable obj, Table<(string item, object value)> table)
        {
            table.AddItems(obj.Inspect()
                            .Prepend((nameof(obj.LabelReadable), obj.LabelReadable))
                            .Prepend((nameof(Type), obj.GetType())));
        }

        private static void Back()
        {
            var prev = History[--HistoryIndex];
            Container.ClearControls();
            Container.AddControls(prev);
            BtnBack.Active = HistoryIndex > 0;
            BtnForward.Active = HistoryIndex < History.Count - 1;
            BtnRefresh.Active = prev.Tag is Inspectable;
            WindowHelp.Validate(true);
        }
        private static void Forward()
        {
            var next = History[++HistoryIndex];
            Container.ClearControls();
            Container.AddControls(next);
            BtnBack.Active = HistoryIndex > 0;
            BtnForward.Active = HistoryIndex < History.Count - 1;
            BtnRefresh.Active = next.Tag is Inspectable;
            WindowHelp.Validate(true);
        }
        private static void Refresh()
        {
            var current = Container.Controls.Single();
            if (current.Tag is not Table<(string item, object value)> table)
                return;
            var obj = table.Tag as Inspectable;
            table.ClearControls();
            PopulateTable(obj, table);
        }
        static void ShowContainer(Control container)
        {
            if (Container.Controls.Contains(container))
                return;
            Container.ClearControls();
            Container.AddControls(container);
            if (HistoryIndex < History.Count - 1)
                History.RemoveRange(HistoryIndex + 1, History.Count - HistoryIndex - 1);
            History.Add(container);
            HistoryIndex++;
            BtnBack.Active = HistoryIndex > 0;
            BtnForward.Active = HistoryIndex < History.Count - 1;
            BtnRefresh.Active = container.Tag is Inspectable;
        }
        internal static void Show()
        {
            if (!WindowHelp.IsOpen)
                WindowHelp.Show();
        }
        static void InitDefDirectory()
        {
            var list = new ListBoxNoScroll<Def>(d => new Label(d.Name, () => Refresh(d)));
            list.AddItems(Def.Database.Values.OrderBy(d => d.Name));
            var search = new SearchBarNew<Def>(Width, id => id.Name).BindTo(list).ToPanelLabeled("Search");
            var scrollablebox = new ScrollableBoxNewNewNew(list, Width, MaxHeight - search.Height, ScrollModes.Vertical) { SmallStep = Label.DefaultHeight + list.Spacing };
            var box = new GroupBox().AddControlsVertically(search, scrollablebox.ToPanel());
            DefDirectory = box;
        }
        static void InitRefDirectory()
        {
            var net = Client.Instance;
            var list = new ListBoxReadOnlyObservable<Entity, Label>(net.World.EntitiesObservable, o => new Label(o.DebugName, () => Refresh(o)));
            var search = new SearchBarNew<Entity>(Width, o => o.Name).BindTo(list).ToPanelLabeled("Search");
            var scrollablebox = new ScrollableBoxNewNewNew(list, Width, MaxHeight - search.Height, ScrollModes.Vertical) { SmallStep = Label.DefaultHeight + list.Spacing };
            var box = new GroupBox().AddControlsVertically(search, scrollablebox.ToPanel());
            ObjectsDirectory = box;
        }
    }
}

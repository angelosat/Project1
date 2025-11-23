using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.UI
{
    class ComboBoxNewNew<T> : GroupBox
    {
        readonly Button Button;
        readonly ListBoxNoScroll<T, Button> ListControl;
        readonly Func<T> CurrentlySelectedGetter;
        readonly Func<IEnumerable<T>> ItemsGetter;
        public ComboBoxNewNew(IEnumerable<T> list, int width, Func<T, string> nameGetter, Action<T> callBack, Func<T> currentlySelectedGetter)
        {
            this.CurrentlySelectedGetter = currentlySelectedGetter;
            this.Button = new Button(() => this.CurrentlySelectedGetter != null ? nameGetter(this.CurrentlySelectedGetter()) : "undefined", BtnPress, width);
            var maxVisibleItems = list.Count();
            this.ListControl = new ListBoxNoScroll<T, Button>(i => CreateButton(i, nameGetter, callBack, width))
                .AddItems(list);
            this.ListControl.ToPanel()
                .HideOnAnyClick();

            this.Controls.Add(this.Button);
        }
        public ComboBoxNewNew(IEnumerable<T> list, int width, Func<T, string> labelGetter, Func<T, string> nameGetter, Action<T> callBack, Func<T> currentlySelectedGetter)
        {
            this.CurrentlySelectedGetter = currentlySelectedGetter;
            this.Button = new Button(() => this.CurrentlySelectedGetter != null ? labelGetter(this.CurrentlySelectedGetter()) : "undefined", BtnPress, width);
            this.ListControl = new ListBoxNoScroll<T, Button>(i => CreateButton(i, nameGetter, callBack, width))
                .AddItems(list);
            this.ListControl.ToPanel()
                .HideOnAnyClick();

            this.Controls.Add(this.Button);
        }
        public ComboBoxNewNew(int width, string label, Func<T, string> nameGetter, Action<T> callBack, Func<T> currentlySelectedGetter, Func<IEnumerable<T>> itemsGetter)
        {
            this.CurrentlySelectedGetter = currentlySelectedGetter;
            this.ItemsGetter = itemsGetter;
            this.Button = new Button(() =>
                //$"{label}: {(this.CurrentlySelectedGetter() is T item ? nameGetter(item) : "none")}", BtnPress, width);
                $"{label}: {nameGetter(this.CurrentlySelectedGetter())}", BtnPress, width);

            var itemwidth = width - (int)this.Button.Font.MeasureString(label).X;
            this.ListControl = new ListBoxNoScroll<T, Button>(i => CreateButton(i, nameGetter, callBack, itemwidth));
            this.ListControl.ToPanel()
                .HideOnAnyClick();

            this.Controls.Add(this.Button);
        }

        public ComboBoxNewNew(IEnumerable<T> list, int width, string label, Func<T, string> nameGetter, Func<string> currentlySelectedGetter, Action<T> callBack)
        {
            this.Button = new Button(() =>
                $"{label}: {currentlySelectedGetter?.Invoke() ?? "undefined"}", BtnPress, width);

            var itemwidth = width - (int)this.Button.Font.MeasureString(label).X;
            this.ListControl = new ListBoxNoScroll<T, Button>(i => CreateButton(i, nameGetter, callBack, itemwidth))
                .AddItems(list);
            this.ListControl.ToPanelScrollable(ScrollModes.Vertical)
                .HideOnAnyClick();

            this.Controls.Add(this.Button);
        }
        public ComboBoxNewNew(IEnumerable<T> list, int width, string label, Func<T, string> nameGetter, Func<T> currentlySelectedGetter, Action<T> callBack)
        {
            this.Button = new Button(() =>
                $"{label}: {(currentlySelectedGetter() is T item ? nameGetter(item) : "none")}", BtnPress, width);

            var itemwidth = width - (int)this.Button.Font.MeasureString(label).X;
            this.ListControl = new ListBoxNoScroll<T, Button>(i => CreateButton(i, nameGetter, callBack, itemwidth)).AddItems(list);
            this.ListControl.ToPanelScrollable(ScrollModes.Vertical)
                .HideOnAnyClick();
            this.Controls.Add(this.Button);
        }
        Button CreateButton(T i, Func<T, string> labelGetter, Action<T> callBack, int width)
        {
            return new Button(labelGetter(i), () => onSelect(i), width);
            void onSelect(T i)
            {
                callBack(i);
                this.ListControl.TopLevelControl.Hide();
            }
        }
        private void BtnPress()
        {
            if (this.ItemsGetter is not null)
                this.Initialize(this.ItemsGetter());
            var panel = this.ListControl.TopLevelControl as PanelScrollable;
            panel.Layer = this.TopLevelControl.Layer;
            //if(panel.Height > UIManager.Height)
            //{
            //    // shorten list and enable scrollbars
            //    panel.Height = UIManager.Height;    
            //}
            panel.Refresh();
            panel.SnapToMouse();
            panel.Show();
            panel.BringToFront();
            
        }
        //public override void OnUIScaleChanged(float oldScale, float newScale)
        //{
        //    base.OnUIScaleChanged(oldScale, newScale);
        //    this.OnResolutionChanged();
        //}
        //internal override void OnResolutionChanged()
        //{
        //    base.OnResolutionChanged();
        //    var panel = this.ListControl.TopLevelControl as PanelScrollable;
        //    //panel.Height = Math.Min(UIManager.Height, this.ListControl.Height + 2 * panel.Padding);
        //    panel.Refresh();
        //}
        public ComboBoxNewNew<T> Initialize(IEnumerable<T> items)
        {
            this.ListControl.Clear();
            this.ListControl.AddItems(items);
            return this;
        }
    }
}

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Start_a_Town_.UI
{
    class TableScrollableCompact<TObject> : GroupBox where TObject : class
    {
        readonly List<Column> Columns = new();
        readonly GroupBox ColumnLabels;
        ListBoxNoScroll<TObject, GroupBox> BoxItems;
        readonly Dictionary<TObject, Dictionary<object, Control>> Rows = new();
        public bool ShowColumnLabels = true;
        public Color ClientBoxColor = Color.Black * .5f;
        ObservableCollection<TObject> BoundCollection;
        public TableScrollableCompact(bool showColumnLabels = false)
        {
            this.ShowColumnLabels = showColumnLabels;
            this.ColumnLabels = new GroupBox() { AutoSize = true, BackgroundColor = Color.SlateGray * .5f };
        }
        public TableScrollableCompact<TObject> Bind(ObservableCollection<TObject> collection)
        {
            if (this.BoundCollection != null)
            {
                this.BoundCollection.CollectionChanged -= Collection_CollectionChanged;
            }

            collection.CollectionChanged += Collection_CollectionChanged;
            this.BoundCollection = collection;
            this.ClearItems();
            this.AddItems(collection);
            return this;
        }

        private void Collection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.AddItems(e.NewItems?.Cast<TObject>());
            this.RemoveItems(e.OldItems?.Cast<TObject>());
        }

        public TableScrollableCompact<TObject> AddColumn(object index, Control columnHeader, int width, Func<TObject, Control> control, float anchor = .5f)
        {
            this.Columns.Add(new Column(index, columnHeader, width, control, anchor));
            this.Build();
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="width"></param>
        /// <param name="control"></param>
        /// <param name="anchor">Value between 0 and 1 for horizontal alignment</param>
        /// <returns></returns>
        public TableScrollableCompact<TObject> AddColumn(object tag, string type, int width, Func<TObject, Control> control, float anchor = 0)//, bool showColumnLabels = true)//float anchor = .5f, bool showColumnLabels = true)
        {
            this.Columns.Add(new Column(tag, type, width, control, anchor));
            this.Build();
            return this;
        }
        public TableScrollableCompact<TObject> AddColumn(object tag, string type, int width, Func<TableScrollableCompact<TObject>, TObject, Control> control, float anchor = 0)//, bool showColumnLabels = true)//float anchor = .5f, bool showColumnLabels = true)
        {
            this.Columns.Add(new Column(tag, type, width, item => control(this, item), anchor));
            this.Build();
            return this;
        }
        public TableScrollableCompact<TObject> Build()
        {
            return this.Build(new List<TObject>());
        }
        public TableScrollableCompact<TObject> Build(IEnumerable<TObject> items)
        {
            this.Rows.Clear();
            this.Controls.Clear();
            this.ColumnLabels.Controls.Clear();
            int offset = 0;
            foreach (var c in this.Columns)
            {
                if (c.ColumnHeader is not null)
                {
                    c.ColumnHeader.Location = new Vector2(offset + c.Width * c.Anchor, 0);
                    c.ColumnHeader.Anchor = new Vector2(c.Anchor, 0);

                    offset += c.Width;
                    this.ColumnLabels.AddControls(c.ColumnHeader);
                }
                else
                {
                    var label = new Label(new Vector2(offset, 0), c.Label);
                    offset += c.Width;
                    label.TextHAlign = Alignment.Horizontal.Center;
                    this.ColumnLabels.AddControls(label);
                }
            }
            // HACK
            this.ColumnLabels.AutoSize = false;
            this.ColumnLabels.ClientSize = new Rectangle(0, 0, offset, this.ColumnLabels.ClientSize.Height);
            this.ColumnLabels.Width = offset;

            if (this.ShowColumnLabels)
                this.Controls.Add(this.ColumnLabels);

            this.ColumnLabels.Controls.AlignCenterHorizontally();
            this.BoxItems = new((TObject item) =>
            {
                var panel = new GroupBox() { BackgroundColor = Color.SlateGray * .2f };// this.ClientBoxColor };
                panel.Tag = item;
                var offset = 0;
                Dictionary<object, Control> controls = new();
                foreach (var c in this.Columns)
                {
                    var control = c.ControlGetter(item);
                    control.Location = new Vector2(offset + c.Width * c.Anchor, 0);
                    control.Anchor = new Vector2(c.Anchor, 0);
                    offset += c.Width;
                    panel.AddControls(control);
                    controls.Add(c.Tag ?? c.Label, control);
                }
                panel.Size = new Rectangle(0, 0, this.ColumnLabels.Width, panel.Height);
                panel.Controls.AlignCenterHorizontally();

                this.Rows.Add(item, controls);
                panel.AutoSize = false;

                return panel;
            });
            if (this.ShowColumnLabels)
                this.BoxItems.Location = this.ColumnLabels.BottomLeft;// + Vector2.UnitY; //spacing between column labels box and items box

            this.AddItems(items.ToArray());

            this.Controls.Add(this.BoxItems);

            return this;
        }

        public Control GetItem(TObject row, object column)
        {
            return this.Rows[row][column];
        }

        public TableScrollableCompact<TObject> AddItems(params TObject[] items)
        {
            return this.AddItems(items as IEnumerable<TObject>);
        }
        public TableScrollableCompact<TObject> AddItems(IEnumerable<TObject> items)
        {
            if (items is null)
                return this;

            this.BoxItems.AddItems(items);
            this.Validate(true);
            return this;
        }
        public void RemoveItems(params TObject[] items)
        {
            foreach (var item in items)
            {
                this.RemoveItem(item);
            }
        }
        public TableScrollableCompact<TObject> RemoveItems(IEnumerable<TObject> items)
        {
            if (items != null)
            {
                foreach (var item in items)
                {
                    this.RemoveItem(item);
                }
            }

            return this;
        }
        public void RemoveItem(TObject item)
        {
            this.Rows.Remove(item);
            this.BoxItems.RemoveItems(item);
        }

        public TableScrollableCompact<TObject> ClearItems()
        {
            this.BoxItems?.ClearControls();
            this.Rows.Clear();
            return this;
        }
        public TableScrollableCompact<TObject> Clear()
        {
            this.ClearItems();
            this.Columns.Clear();
            this.Controls.Clear();
            this.ColumnLabels.Controls.Clear();
            return this;
        }

        class Column
        {
            public object Tag;
            public string Label;
            public int Width;
            public Func<TObject, Control> ControlGetter;
            public float Anchor;
            public Control ColumnHeader;

            public Column(object tag, string obj, int width, Func<TObject, Control> control, float anchor)
            {
                this.Tag = tag;
                this.Label = obj;
                this.Width = width;
                this.ControlGetter = control;
                this.Anchor = anchor;
            }
            public Column(object tag, Control columnHeader, int width, Func<TObject, Control> control, float anchor)
            {
                this.Tag = tag;
                this.ColumnHeader = columnHeader;
                this.Width = width;
                this.ControlGetter = control;
                this.Anchor = anchor;
            }
        }
    }
}

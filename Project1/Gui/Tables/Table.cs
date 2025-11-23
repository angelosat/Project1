using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    public class Table<TObject> : GroupBox, IListSearchable<TObject>
    {
        public int Spacing = 1;
        readonly List<Column> Columns = new();
        public Action<TObject> ItemChangedFunc = (item) => { };
        public ObservableCollection<TObject> List;
        static readonly Color DefaultRowColor = Color.SlateGray * .2f;
        public Color RowColor = DefaultRowColor;

        public int RowWidth;// => this.Columns.Sum(c => c.Width);
        //public int RowHeight => Label.DefaultHeight + Spacing;
        public int RowHeight;// => Label.DefaultHeight;
      
        public int GetHeightFromRowCount(int rowCount)
        {
            return rowCount * (this.RowHeight + Spacing) - Spacing;
        }

        Table<TObject> Clear()
        {
            this.Controls.Clear();
            return this;
        }

        public Table<TObject> Bind(ObservableCollection<TObject> collection)
        {
            if (collection == this.List)
                return this;
            if (this.List != null)
                this.List.CollectionChanged -= this.List_CollectionChanged;
            this.List = collection;
            this.List.CollectionChanged += this.List_CollectionChanged;
            this.Clear();
            this.AddItems(collection);
            return this;
        }

        public Table<TObject> AddColumn(string label, int width, Func<TObject, Control> controlGetter, float anchorX = 0)
        {
            this.Columns.Add(new(label, width, controlGetter, anchorX));
            this.RowWidth += width;
            return this;
        }

        private void List_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.AddItems(e.NewItems?.Cast<TObject>());
            this.RemoveItems(e.OldItems?.Cast<TObject>());
        }

        public void AddItems(IEnumerable<TObject> items)
        {
            if (items is null)
                return;
            var currentY = this.Controls.Any() ? this.BottomLeft.Y + Spacing : 0;
            var newControls = items.Select(item =>
            //this.AddItem(i);
            {
                var row = new GroupBox() { BackgroundColor = RowColor };
                int currentX = 0;
                foreach (var col in this.Columns)
                {
                    var ctrl = col.ControlGetter(item);
                    //ctrl.SetLocation(currentX, 0);
                    //ctrl.SetLocation(currentX + (int)((col.AnchorX + ctrl.Anchor.X) * (col.Width - ctrl.Width)), 0); // addede + ctrl.Anchor.X as a workaround

                    if (ctrl.Anchor.X > 0)   /// HACK workaround
                        ctrl.SetLocation(currentX + col.Width / 2, 0);
                    else
                        ctrl.SetLocation(currentX + (int)(col.AnchorX * (col.Width - ctrl.Width)), 0);

                    row.AddControls(ctrl);
                    currentX += col.Width;
                }
                row.Controls.AlignCenterHorizontally();
                row.Tag = item;
                this.RowHeight = row.Height;
                row.Width = this.RowWidth;
                row.AutoSize = false;
                row.Location.Y = currentY;
                currentY += row.Height + Spacing;
                return row;
            });
            this.AddControls(newControls.ToArray());
        }
        //Control AddItem(TObject item)
        //{
        //    //var row = new GroupBox(this.RowWidth, Label.DefaultHeight) { BackgroundColor = DefaultRowColor };
        //    var row = new GroupBox() { BackgroundColor = DefaultRowColor };
        //    int currentX = 0;
        //    foreach (var col in this.Columns)
        //    {
        //        row.AddControls(col.ControlGetter(item).SetLocation(currentX, 0));
        //        currentX += col.Width;
        //    }
        //    row.Controls.AlignCenterHorizontally();
        //    this.RowHeight = row.Height;
        //    this.AddControlsBottomLeft(row);
        //    row.Location.Y += Spacing;
        //    row.Tag = item;
        //    return row;
        //}
        void RemoveItems(IEnumerable<TObject> items)
        {
            if (items is null)
                return;
            foreach (var i in items)
                this.RemoveItem(i);
        }
        public void RemoveItem(TObject item)
        {
            if (item is null)
                return;
            var listControls = this.Controls;
            var removedItemIndex = listControls.FindIndex(c => c.Tag.Equals(item));
            var prevY = listControls[removedItemIndex].Location.Y;
            for (int i = removedItemIndex + 1; i < listControls.Count; i++)
            {
                var r = listControls[i];
                r.Location.Y = prevY;
                prevY = r.Bottom + Spacing;
            }
            listControls.RemoveAt(removedItemIndex);
        }

        public void Filter(Func<TObject, bool> filter)
        {
            this.Controls.Clear();
            var validControls = this.Controls.Where(c => filter((TObject)c.Tag)).ToArray();
            this.AddControlsBottomLeft(validControls);
        }

        class Column
        {
            public string Label;
            public int Width;
            public Func<TObject, Control> ControlGetter;
            public float AnchorX;
            public Column(string label, int width, Func<TObject, Control> controlGetter, float anchorx)
            {
                this.Label = label;
                this.Width = width;
                this.ControlGetter = controlGetter;
                this.AnchorX = anchorx;
            }
        }
    }
}


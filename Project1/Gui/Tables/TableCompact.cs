using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Start_a_Town_.UI
{
    class TableCompact<TObject> : GroupBox where TObject : class
    {
        readonly List<Column> Columns = new();
        readonly GroupBox ColumnLabels;
        ListBoxNoScroll<TObject, GroupBox> BoxItems;
        readonly Dictionary<TObject, Dictionary<object, Control>> Rows = new();
        public bool ShowColumnLabels = true;
        public Color ClientBoxColor = Color.Black * .5f;
        INotifyCollectionChanged BoundCollection;
        INotifyCollectionChanged IBoundCollection;
        Func<NotifyCollectionChangedEventArgs, TObject[]> NewItemsGetter;
        Func<NotifyCollectionChangedEventArgs, TObject[]> OldItemsGetter;
        /// <summary>
        /// incase of an observabledictionary, we need a helper dic to store 
        /// keys of removed items, because the notify event only provides keys
        /// and the items get removed from the collection before we process the event
        /// </summary>
        readonly Dictionary<object, TObject> HelperDic = new();
        public TableCompact(bool showColumnLabels = false)
        {
            this.ShowColumnLabels = showColumnLabels;
            this.ColumnLabels = new GroupBox() { AutoSize = true, BackgroundColor = Color.SlateGray * .5f };
        }
        //public TableCompact<TObject> Bind<T, TSource>(T collection, Func<TSource, TObject> converter) where T : INotifyCollectionChanged, ICollection<TSource>
        //{
        //    if (this.BoundCollection != null)
        //    {
        //        this.BoundCollection.CollectionChanged -= Collection_CollectionChanged;
        //    }

        //    collection.CollectionChanged += Collection_CollectionChanged;
        //    this.BoundCollection = collection;
        //    this.ClearItems();
        //    this.AddItems(collection.Select(converter));
        //    return this;
        //}
        public TableCompact<TObject> Bind<T>(T collection) where T : INotifyCollectionChanged, ICollection<TObject>
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
        /// <summary>
        /// use bind(ICollection<TObject> collection) instead
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        [Obsolete]
        public TableCompact<TObject> Bind(ObservableCollection<TObject> collection)
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
        public TableCompact<TObject> Bind<TKey, TO>(ObservableDictionary<TKey, TO> icollection) where TO : class
        {
            if (this.IBoundCollection != null)
            {
                this.IBoundCollection.CollectionChanged -= Collection_CollectionChanged;
            }
            icollection.CollectionChanged += Collection_CollectionChanged;
            this.IBoundCollection = icollection;
            this.ClearItems();
            //this.AddItems(icollection.Values.Where(i => i is TObject).Cast<TObject>());
            this.AddItems(icollection.Where(i => i.Value is TObject).Select(i =>
            {
                var val = i.Value as TObject;
                this.HelperDic[i.Key] = val;
                return val;
            }).ToArray());
            this.NewItemsGetter = a => new List<TKey>(a.NewItems.OfType<TKey>()).Select(k =>
            {
                var item = icollection[k];
                this.HelperDic[k] = item as TObject;
                return item;
            }).Where(i => i is TObject).Cast<TObject>().ToArray();
            //this.OldItemsGetter = a => new List<TKey>(a.OldItems.OfType<TKey>()).Select(k => icollection[k]).Where(i => i is TObject).Cast<TObject>().ToArray();
            this.OldItemsGetter = a => new List<TKey>(a.OldItems.OfType<TKey>()).Select(k =>
            {
                var item = HelperDic[k];
                this.HelperDic.Remove(k);
                return item;
            }).ToArray();
            return this;
        }
       
        private void Collection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //this.AddItems(e.NewItems?.Cast<TObject>());
            //this.RemoveItems(e.OldItems?.Cast<TObject>());

            if (this.NewItemsGetter != null && e.NewItems != null)
                this.AddItems(this.NewItemsGetter(e));
            else
                this.AddItems(e.NewItems?.Cast<TObject>());
            if(this.OldItemsGetter != null && e.OldItems != null)
                this.RemoveItems(this.OldItemsGetter(e));
            else
                this.RemoveItems(e.OldItems?.Cast<TObject>());
        }

        public TableCompact<TObject> AddColumn(object index, Control columnHeader, int width, Func<TObject, Control> control, float anchor = .5f)
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
        public TableCompact<TObject> AddColumn(object tag, string type, int width, Func<TObject, Control> control, float anchor = 0)//, bool showColumnLabels = true)//float anchor = .5f, bool showColumnLabels = true)
        {
            this.Columns.Add(new Column(tag, type, width, control, anchor));
            this.Build();
            return this;
        }
        public TableCompact<TObject> AddColumn(object tag, string type, int width, Func<TableCompact<TObject>, TObject, Control> control, float anchor = 0)//, bool showColumnLabels = true)//float anchor = .5f, bool showColumnLabels = true)
        {
            this.Columns.Add(new Column(tag, type, width, item => control(this, item), anchor));
            this.Build();
            return this;
        }
        public TableCompact<TObject> Build()
        {
            return this.Build(new List<TObject>());
        }
        public TableCompact<TObject> Build(IEnumerable<TObject> items)
        {
            if (this.Columns.Count == 0)
                throw new Exception();
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
            this.Width = this.ColumnLabels.Width; //hack because if the table is built with no items, no column labels and autosize is true, then width is zero
            return this;
        }

        public Control GetItem(TObject row, object column)
        {
            return this.Rows[row][column];
        }

        public TableCompact<TObject> AddItems(params TObject[] items)
        {
            return this.AddItems(items as IEnumerable<TObject>);
        }
        public TableCompact<TObject> AddItems(IEnumerable<TObject> items)
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
        public TableCompact<TObject> RemoveItems(IEnumerable<TObject> items)
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

        public TableCompact<TObject> ClearItems()
        {
            this.BoxItems?.ClearControls();
            this.Rows.Clear();
            return this;
        }
        public TableCompact<TObject> Clear()
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

    class TableCompact<TSource, TObject> : GroupBox where TObject : class
    {
        readonly List<Column> Columns = new();
        readonly GroupBox ColumnLabels;
        ListBoxNoScroll<TObject, GroupBox> BoxItems;
        readonly Dictionary<TObject, Dictionary<object, Control>> Rows = new();
        public bool ShowColumnLabels = true;
        public Color ClientBoxColor = Color.Black * .5f;
        INotifyCollectionChanged BoundCollection;
        INotifyCollectionChanged IBoundCollection;
        Func<NotifyCollectionChangedEventArgs, TSource[]> NewItemsGetter;
        Func<NotifyCollectionChangedEventArgs, TSource[]> OldItemsGetter;
        Func<TSource, TObject> Converter;
        /// <summary>
        /// incase of an observabledictionary, we need a helper dic to store 
        /// keys of removed items, because the notify event only provides keys
        /// and the items get removed from the collection before we process the event
        /// </summary>
        readonly Dictionary<object, TObject> HelperDic = new();
        public TableCompact(Func<TSource, TObject> converter, bool showColumnLabels = false)
        {
            this.Converter = converter;
            this.ShowColumnLabels = showColumnLabels;
            this.ColumnLabels = new GroupBox() { AutoSize = true, BackgroundColor = Color.SlateGray * .5f };
        }
        public TableCompact<TSource, TObject> Bind<T>(T collection) where T : INotifyCollectionChanged, ICollection<TSource>
        {
            if (this.BoundCollection == collection as INotifyCollectionChanged)
                return this;
            if (this.BoundCollection != null)
                this.BoundCollection.CollectionChanged -= Collection_CollectionChanged;

            collection.CollectionChanged += Collection_CollectionChanged;
            this.BoundCollection = collection;
            this.ClearItems();
            this.AddItems(collection);
            return this;
        }

        /// <summary>
        /// use bind(ICollection<TObject> collection) instead
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
    
        public TableCompact<TSource, TObject> Bind<TKey, TO>(ObservableDictionary<TKey, TSource> icollection)
        {
            if (this.IBoundCollection != null)
            {
                this.IBoundCollection.CollectionChanged -= Collection_CollectionChanged;
            }
            icollection.CollectionChanged += Collection_CollectionChanged;
            this.IBoundCollection = icollection;
            this.ClearItems();
            this.AddItems(icollection.Values);
            return this;
        }

        private void Collection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.AddItems(e.NewItems?.Cast<TSource>());
            this.RemoveItems(e.OldItems?.Cast<TSource>());
        }

        public TableCompact<TSource, TObject> AddColumn(object index, Control columnHeader, int width, Func<TObject, Control> control, float anchor = .5f)
        {
            this.Columns.Add(new Column(index, columnHeader, width, control, anchor));
            this.Build();
            return this;
        }
        public TableCompact<TSource, TObject> ClearColumns()
        {
            this.Columns.Clear();
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
        public TableCompact<TSource, TObject> AddColumn(object tag, string type, int width, Func<TObject, Control> control, float anchor = 0)//, bool showColumnLabels = true)//float anchor = .5f, bool showColumnLabels = true)
        {
            this.Columns.Add(new Column(tag, type, width, control, anchor));
            this.Build();
            return this;
        }
        public TableCompact<TSource, TObject> AddColumn(object tag, string type, int width, Func<TableCompact<TSource, TObject>, TObject, Control> control, float anchor = 0)//, bool showColumnLabels = true)//float anchor = .5f, bool showColumnLabels = true)
        {
            this.Columns.Add(new Column(tag, type, width, item => control(this, item), anchor));
            this.Build();
            return this;
        }
        public TableCompact<TSource, TObject> Build()
        {
            return this.Build(new List<TSource>());
        }
        public TableCompact<TSource, TObject> Build(IEnumerable<TSource> items)
        {
            if (this.Columns.Count == 0)
                throw new Exception();
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
            this.Width = this.ColumnLabels.Width; //hack because if the table is built with no items, no column labels and autosize is true, then width is zero
            return this;
        }

        public Control GetItem(TObject row, object column)
        {
            return this.Rows[row][column];
        }

        public TableCompact<TSource, TObject> AddItems(params TSource[] items)
        {
            return this.AddItems(items as IEnumerable<TSource>);
        }
        public TableCompact<TSource, TObject> AddItems(IEnumerable<TSource> items)
        {
            if (items is null)
                return this;
            List<TObject> newItems = new();
            foreach(var item in items)
            {
                var val = this.Converter(item);
                this.HelperDic[item] = val;
                newItems.Add(val);
            }
            this.BoxItems.AddItems(newItems);
            this.Validate(true);
            return this;
        }
        public void RemoveItems(params TSource[] items)
        {
            foreach (var item in items)
            {
                this.RemoveItem(item);
            }
        }
        public TableCompact<TSource, TObject> RemoveItems(IEnumerable<TSource> items)
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
        public void RemoveItem(TSource item)
        {
            var val = this.HelperDic[item];
            this.HelperDic.Remove(item);
            this.Rows.Remove(val);
            this.BoxItems.RemoveItems(val);
        }

        public TableCompact<TSource, TObject> ClearItems()
        {
            this.BoxItems?.ClearControls();
            this.Rows.Clear();
            return this;
        }
        public TableCompact<TSource, TObject> Clear()
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

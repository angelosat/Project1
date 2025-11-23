using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Policy;

namespace Start_a_Town_.UI
{
    public class ListBoxObservable<TObject> : GroupBox, IListSearchable<TObject>
      where TObject : IListable
    {
        const int Spacing = 1;
        static readonly Func<TObject, bool> DefaultFilter = i => true;
        Func<TObject, bool> CurrentFilter = DefaultFilter;
        public TObject SelectedItem => this.SelectedControl == null ? default : (TObject)this.SelectedControl.Tag;
        Control SelectedControl;
        public Action<TObject> ItemChangedFunc = (item) => { };
        public ObservableCollection<TObject> List;
        /// <summary>
        /// A list containing all item controls, not just the currently displayed ones. Used for filtering.
        /// </summary>
        readonly List<Control> Items = new();

        public ListBoxObservable(ObservableCollection<TObject> objects)
        {
            this.Bind(objects);
        }

        public void SelectItem(TObject obj)
        {
            this.SelectedControl = this.Controls.FirstOrDefault(i => i.Tag.Equals(obj));
        }
        
        ListBoxObservable<TObject> Clear()
        {
            this.Controls.Clear();
            return this;
        }
        
        ListBoxObservable<TObject> Bind(ObservableCollection<TObject> collection)
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

        private void List_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.AddItems(e.NewItems?.Cast<TObject>(), e.NewStartingIndex);
            this.RemoveItems(e.OldItems?.Cast<TObject>());
        }

        void AddItems(IEnumerable<TObject> items)
        {
            if (items == null)
                return;
            foreach (var i in items)
                this.AddItem(i);
        }
        void AddItems(IEnumerable<TObject> items, int index)
        {
            if (items == null)
                return;
            var newControls = items.Select(i =>
            {
                var gui = i.GetListControlGui();
                gui.Tag = i;
                this.Items.Add(gui);
                return gui;
            });
            this.Controls.Insert(index, newControls);
            this.Controls.AlignVertically(Spacing);
        }

        Control AddItem(TObject item)
        {
            var control = item.GetListControlGui();
            control.Tag = item;
            this.Items.Add(control);
            if (this.Controls.Any())
                this.AddControlsBottomLeft(Spacing, control);
            else
                this.AddControls(control);
            return control;
        }

        void RemoveItems(IEnumerable<TObject> items)
        {
            if (items is null)
                return;
            foreach (var i in items)
                this.RemoveItem(i);
        }
        void RemoveItem(TObject item)
        {
            if (item is null)
                return;
            this.Items.Remove(this.Items.First(c => c.Tag.Equals(item)));//
            var listControls = this.Controls;
            var removedItemIndex = listControls.FindIndex(c => c.Tag.Equals(item));//
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
            this.CurrentFilter = filter ?? DefaultFilter;
            this.Controls.Clear();
            var validControls = this.Items.Where(c => this.CurrentFilter((TObject)c.Tag)).ToArray();
            this.AddControlsVertically(Spacing, validControls);
        }

        public Control CreateFilters(params (string name, Func<TObject, bool> filter)[] filters)
        {
            Func<TObject, bool> selectedFilter = null;

            return new GroupBox().AddControlsLineWrap(filters.Select(f => new Button(f.name, () => selectFilter(f.filter)) { IsToggledFunc = () => selectedFilter == f.filter }));

            void selectFilter(Func<TObject, bool> filter)
            {
                selectedFilter = filter;
                this.Filter(filter);
            }
        }
    }

    public class ListBoxObservable<TObject, TControl> : GroupBox, IListSearchable<TObject>
        where TControl : Control, new()
    {
        public int Spacing = 1;
        static readonly Func<TObject, bool> DefaultFilter = i => true;
        Func<TObject, bool> CurrentFilter = DefaultFilter;
        public TObject SelectedItem => this.SelectedControl == null ? default : (TObject)this.SelectedControl.Tag;
        TControl SelectedControl;

        public void SelectItem(TObject obj)
        {
            this.SelectedControl = this.Controls.FirstOrDefault(i => i.Tag.Equals(obj)) as TControl;
        }

        /// <summary>
        /// A list containing all item controls, not just the currently displayed ones. Used for filtering.
        /// </summary>
        readonly List<TControl> Items = new();

        public Action<TObject> ItemChangedFunc = (item) => { };
        public ObservableCollection<TObject> List;
        readonly Func<TObject, TControl> ControlFactory;
        ListBoxObservable<TObject, TControl> Clear()
        {
            this.Controls.Clear();
            return this;
        }
        public ListBoxObservable(ObservableCollection<TObject> collection, Func<TObject, TControl> controlFactory)
            : this(controlFactory)
        {
            this.Bind(collection);
        }
        public ListBoxObservable(Func<TObject, TControl> controlFactory)
        {
            this.ControlFactory = controlFactory;
        }
        public ListBoxObservable<TObject, TControl> Bind(ObservableCollection<TObject> collection)
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

        private void List_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.AddItems(e.NewItems?.Cast<TObject>());
            this.RemoveItems(e.OldItems?.Cast<TObject>());
        }

        void AddItems(IEnumerable<TObject> items)
        {
            if (items == null)
                return;
            foreach (var i in items)
                this.AddItem(i);
        }

        TControl AddItem(TObject item)
        {
            var control = this.ControlFactory(item);
            control.Tag = item;
            this.Items.Add(control);
            if (this.Controls.Any())
            {
                this.AddControlsBottomLeft(control);
                control.Location.Y += Spacing;
            }
            else
                this.AddControls(control);
            return control;
        }
        void RemoveItems(IEnumerable<TObject> items)
        {
            if (items is null)
                return;
            foreach (var i in items)
                this.RemoveItem(i);
        }
        void RemoveItem(TObject item)
        {
            if (item is null)
                return;
            this.Items.Remove(this.Items.First(c => c.Tag.Equals(item)));
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
            this.CurrentFilter = filter ?? DefaultFilter;
            this.Controls.Clear();
            var validControls = this.Items.Where(c => this.CurrentFilter((TObject)c.Tag)).ToArray();
            this.AddControlsVertically(Spacing, validControls);
        }

        public Control CreateFilters(params (string name, Func<TObject, bool> filter)[] filters)
        {
            Func<TObject, bool> selectedFilter = null;

            return new GroupBox().AddControlsLineWrap(filters.Select(f => new Button(f.name, () => selectFilter(f.filter)) { IsToggledFunc = () => selectedFilter == f.filter }));

            void selectFilter(Func<TObject, bool> filter)
            {
                selectedFilter = filter;
                this.Filter(filter);
            }
        }
    }

    public class ListBoxObservable<TSource, TObject, TControl> : GroupBox, IListSearchable<TObject>
        where TControl : Control, new()
    {
        public int Spacing = 1;
        static readonly Func<TObject, bool> DefaultFilter = i => true;
        Func<TObject, bool> CurrentFilter = DefaultFilter;
        public TObject SelectedItem => this.SelectedControl == null ? default : (TObject)this.SelectedControl.Tag;
        TControl SelectedControl;
        Func<TSource, TObject> Converter;

        public void SelectItem(TObject obj)
        {
            this.SelectedControl = this.Controls.FirstOrDefault(i => i.Tag.Equals(obj)) as TControl;
        }

        /// <summary>
        /// A list containing all item controls, not just the currently displayed ones. Used for filtering.
        /// </summary>
        readonly List<TControl> Items = new();

        public Action<TObject> ItemChangedFunc = (item) => { };
        public INotifyCollectionChanged List;
        readonly Func<TObject, TControl> ControlFactory;
        readonly Dictionary<TSource, TObject> HelperDic = new();
        ListBoxObservable<TSource, TObject, TControl> Clear()
        {
            this.Controls.Clear();
            return this;
        }
       
        public ListBoxObservable(Func<TObject, TControl> controlFactory)
        {
            this.ControlFactory = controlFactory;
        }
        //INotifyCollectionChanged, ICollection<TSource>
        //public ListBoxObservable<TSource, TObject, TControl> Bind(ObservableCollection<TSource> collection, Func<TSource, TObject> converter) 
        public ListBoxObservable<TSource, TObject, TControl> Bind<T>(T collection, Func<TSource, TObject> converter) where T : INotifyCollectionChanged, ICollection<TSource>
        {
            this.Converter = converter;
            if (collection as INotifyCollectionChanged == this.List)
                return this;
            if (this.List != null)
                this.List.CollectionChanged -= this.List_CollectionChanged;
            this.List = collection;
            this.List.CollectionChanged += this.List_CollectionChanged;
            this.Clear();
            this.AddItems(collection);
            return this;
        }

        private void List_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.AddItems(e.NewItems?.Cast<TSource>());
            this.RemoveItems(e.OldItems?.Cast<TSource>());
        }

        void AddItems(IEnumerable<TSource> items)
        {
            if (items == null)
                return;
            foreach (var i in items)
                this.AddItem(i);
        }

        TControl AddItem(TSource itemSource)
        {
            var item = this.Converter(itemSource);
            this.HelperDic[itemSource] = item;
            var control = this.ControlFactory(item);
            control.Tag = item;
            this.Items.Add(control);
            if (this.Controls.Any())
            {
                this.AddControlsBottomLeft(control);
                control.Location.Y += Spacing;
            }
            else
                this.AddControls(control);
            return control;
        }
        void RemoveItems(IEnumerable<TSource> items)
        {
            if (items is null)
                return;
            foreach (var i in items)
                this.RemoveItem(i);
        }
        void RemoveItem(TSource item)
        {
            if (item is null)
                return;
            this.HelperDic.Remove(item);
            this.Items.Remove(this.Items.First(c => c.Tag.Equals(item)));
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
            this.CurrentFilter = filter ?? DefaultFilter;
            this.Controls.Clear();
            var validControls = this.Items.Where(c => this.CurrentFilter((TObject)c.Tag)).ToArray();
            this.AddControlsVertically(Spacing, validControls);
        }

        public Control CreateFilters(params (string name, Func<TObject, bool> filter)[] filters)
        {
            Func<TObject, bool> selectedFilter = null;

            return new GroupBox().AddControlsLineWrap(filters.Select(f => new Button(f.name, () => selectFilter(f.filter)) { IsToggledFunc = () => selectedFilter == f.filter }));

            void selectFilter(Func<TObject, bool> filter)
            {
                selectedFilter = filter;
                this.Filter(filter);
            }
        }
    }
}


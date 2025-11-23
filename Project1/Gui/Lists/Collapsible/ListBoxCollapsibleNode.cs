using System;
using System.Collections.Generic;

namespace Start_a_Town_.UI
{
    public class ListBoxCollapsibleNode
    {
        public List<Control> Leafs = new();
        public List<ILabeled> CustomLeafs = new();
        public Control Control;
        public List<Control> LeafControls = new();
        public List<Control> ChildControls = new();
        public ListBoxCollapsibleNode Parent;
        public List<ListBoxCollapsibleNode> Children = new();
        public Func<Control> ControlGetter;
        public PictureBox Arrow;
        public GroupBox ChildGroupBox = new();// { BackgroundColor = Color.Red * .2f };
        public static readonly int IndentWidth = UIManager.ArrowRight.Rectangle.Width;

        public string Name;
        public bool Expanded;
        public ListBoxCollapsibleNode(IListCollapsibleDataSource node, Action<int[], int[]> callback)
        {
            throw new NotImplementedException();
        }
        public ListBoxCollapsibleNode(IListCollapsibleDataSource node)
        {
            this.Name = node.Label;
            this.ControlGetter = () => node.GetListControlGui();

            foreach (var child in node.ListBranches)
                this.AddNode(new ListBoxCollapsibleNode(child));
            foreach (var leaf in node.ListLeafs)
                this.AddLeaf(leaf.GetListControlGui());
        }
        public ListBoxCollapsibleNode(string name)
        {
            this.Name = name;
            this.ChildGroupBox.Name = name;
        }
        public ListBoxCollapsibleNode(string name, Func<ListBoxCollapsibleNode, Control> controlGetter) : this(name)
        {
            this.ControlGetter = () => controlGetter(this);
        }
        public ListBoxCollapsibleNode(string name, Func<Control> controlGetter) : this(name)
        {
            this.ControlGetter = controlGetter;
        }
        public ListBoxCollapsibleNode AddNode(ListBoxCollapsibleNode node)
        {
            this.Children.Add(node);
            node.Parent = this;
            return this;
        }
        public ListBoxCollapsibleNode AddLeaf(ILabeled leaf)
        {
            this.CustomLeafs.Add(leaf);
            return this;
        }
        public ListBoxCollapsibleNode AddLeaf(Control leaf)
        {
            //leaf.BackgroundColor = UIManager.DefaultListItemBackgroundColor;
            this.LeafControls.Add(leaf);
            this.ChildGroupBox.AddControlsBottomLeft(leaf);
            leaf.Location.X = IndentWidth;
            leaf.Validate(true);
            return this;
        }
        public ListBoxCollapsibleNode Clear()
        {
            this.Children.Clear();
            return this;
        }

        public void Build<C>(Action<ILabeled, C> onControlInit, Action<ILabeled[]> onLeafSelect) where C : ButtonBase, new()
        {
            foreach (var item in this.CustomLeafs)
            {
                var checkbox = new C();
                checkbox.Tag = item;
                var label = new Label(item.Label);
                var box = new GroupBox();
                box.AddControlsHorizontally(checkbox, label);
                onControlInit(item, checkbox);
                checkbox.LeftClickAction = () => onLeafSelect(new ILabeled[] { item });
                this.LeafControls.Add(box);
            }
        }
        public int GetDepth()
        {
            var depth = 0;
            var parent = this.Parent;
            while (parent is not null)
            {
                depth++;
                parent = parent.Parent;
            }
            return depth;
        }
        public override string ToString()
        {
            return this.Name;
        }

        internal void FindLeafIndex(ButtonBase c, ref int i)
        {
            foreach (var leaf in this.Leafs)
            {
                if (leaf == c)
                    return;
                i++;
            }
            foreach (var child in this.Children)
                child.FindLeafIndex(c, ref i);
        }

        internal ButtonBase GetLeaf(int i)
        {
            throw new NotImplementedException();
        }
    }
}


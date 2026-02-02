using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    public partial class ListCollapsibleObservable 
    {
        public class Node
        {
            public List<Control> Leafs = new();
            public List<ILabeled> CustomLeafs = new();
            public Control Control;
            public List<Control> LeafControls = new();
            public List<Control> ChildControls = new();
            public Node ParentNode;
            public List<Node> ChildrenNodes = new();
            public Func<Control> ControlGetter;
            public PictureBox Arrow;
            public GroupBox ChildGroupBox = new() { BackgroundColor = Color.Black * .2f };
            public static readonly int IndentWidth = UIManager.ArrowRight.Rectangle.Width;
            public readonly IListCollapsibleDataSourceObservable Source;
            public string Name;
            public bool Expanded;
            
            public Node(IListCollapsibleDataSourceObservable node)
            {
                this.Name = node.Label;
                this.ControlGetter = () => node.GetListControlGui();
                this.Source = node;
                this.Bind(node);

                foreach (var child in node.ListBranches)
                    this.AddNode(new Node(child));
                foreach (var leaf in node.ListLeafs)
                    this.AddLeaf(leaf.GetListControlGui());
            }
            void Bind(IListCollapsibleDataSourceObservable source)
            {
                if (this.Source is not null)
                {
                    this.Source.ListBranches.CollectionChanged -= ListBranches_CollectionChanged1;
                    this.Source.ListLeafs.CollectionChanged -= ListLeafs_CollectionChanged1;
                }
                source.ListBranches.CollectionChanged += this.ListBranches_CollectionChanged1;
                source.ListLeafs.CollectionChanged += this.ListLeafs_CollectionChanged1;
            }
            private void ListBranches_CollectionChanged1(object sender, NotifyCollectionChangedEventArgs e)
            {
                var added = e.NewItems?.Cast<IListCollapsibleDataSourceObservable>();
                var removed = e.OldItems?.Cast<IListCollapsibleDataSourceObservable>();
                if(added is not null)
                    foreach(var item in added)
                    this.AddNode(new Node(item));
                if(removed is not null)
                    foreach (var item in removed)
                    this.RemoveNode(this.ChildrenNodes.Find(c=>c.Source == item));
            }
            private void ListLeafs_CollectionChanged1(object sender, NotifyCollectionChangedEventArgs e)
            {
                var added = e.NewItems?.Cast<IListable>();
                var removed = e.OldItems?.Cast<IListable>();
                if(added is not null)
                    foreach (var item in added)
                        this.AddLeaf(item);
                if(removed is not null)
                    foreach (var item in removed)
                        this.RemoveLeaf(this.LeafControls.Find(c => c.Tag == item));
            }

            public Node(string name)
            {
                this.Name = name;
                this.ChildGroupBox.Name = name;
            }
            public Node(string name, Func<Node, Control> controlGetter) : this(name)
            {
                this.ControlGetter = () => controlGetter(this);
            }
            public Node(string name, Func<Control> controlGetter) : this(name)
            {
                this.ControlGetter = controlGetter;
            }
            Node AddNode(Node node)
            {
                this.ChildrenNodes.Add(node);
                node.ParentNode = this;
                return this;
            }
            bool RemoveNode(Node node)
            {
                return this.ChildrenNodes.Remove(node);
            }
            Node AddLeaf(ILabeled leaf)
            {
                this.CustomLeafs.Add(leaf);
                return this;
            }
            Node AddLeaf(IListable leaf)
            {
                var leafcontrol = leaf.GetListControlGui();
                leafcontrol.Tag = leaf;
                return this.AddLeaf(leafcontrol);
            }
            Node AddLeaf(Control leaf)
            {
                //leaf.BackgroundColor = UIManager.DefaultListItemBackgroundColor;
                //leaf.Location.X = IndentWidth;

                this.LeafControls.Add(leaf);
                this.ChildGroupBox.AddControlsBottomLeft(leaf);
                leaf.Validate(true);
                return this;
            }
            bool RemoveLeaf(Control leaf)
            {
                this.ChildGroupBox.RemoveControls(leaf);
                //this.ChildGroupBox.Controls.AlignVertically();
                this.ChildGroupBox.AlignTopToBottom();
                return this.LeafControls.Remove(leaf);
            }
            Node Clear()
            {
                this.ChildrenNodes.Clear();
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
                var parent = this.ParentNode;
                while (parent is not null)
                {
                    depth++;
                    parent = parent.ParentNode;
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
                foreach (var child in this.ChildrenNodes)
                    child.FindLeafIndex(c, ref i);
            }

            internal ButtonBase GetLeaf(int i)
            {
                throw new NotImplementedException();
            }
           
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    public class ListCollapsibleNew : GroupBox
    {
        public ListBoxCollapsibleNode ARoot = new("root");
        int Spacing = 0;
        public ListCollapsibleNew()
        {

        }
        public ListCollapsibleNew(IListCollapsibleDataSource dataSource, Action<int[], int[]> callback)
        {
            throw new NotImplementedException();
        }
        public ListCollapsibleNew(IListCollapsibleDataSource dataSource)
        {
            var node = new ListBoxCollapsibleNode(dataSource);
            this.ARoot.AddNode(node);
            this.Build();
        }
        public ListCollapsibleNew AddNode(ListBoxCollapsibleNode node)
        {
            this.ARoot.AddNode(node);
            return this;
        }
        public ListCollapsibleNew Build()
        {
            this.ClearControls();
            var queue = new Queue<ListBoxCollapsibleNode>(this.ARoot.Children);

            while (queue.Any())
            {
                var node = queue.Dequeue();
                var nodeContainer = new GroupBox() { Name = "container", BackgroundColor = UIManager.DefaultListItemBackgroundColor };
                var nodeItem = new GroupBox() { Name = "item" };
                node.Arrow = new PictureBox(UIManager.ArrowRight) { LeftDownAction = expand };// { LeftClickAction = expand };
                var label = new Label(node.Name) { Active = true };
                var control = node.ControlGetter?.Invoke();
                if (control is not null)
                    nodeItem.AddControlsHorizontally(node.Arrow, control, label);
                else
                    nodeItem.AddControlsHorizontally(node.Arrow, label);
                nodeItem.CenterControlsAlignmentVertically();
                nodeItem.Validate(true);
                nodeContainer.AddControls(nodeItem);

                node.Parent?.ChildControls.Add(nodeContainer);
                node.Parent?.ChildGroupBox.Controls.Insert(0, nodeContainer);

                label.LeftClickAction = expand;
                void expand()
                {
                    if (!node.Expanded)
                    {
                        node.Expanded = true;
                        node.Arrow.SetTexture(UIManager.ArrowDown);
                        node.ChildGroupBox.Location = nodeItem.BottomLeft + new Vector2(ListBoxCollapsibleNode.IndentWidth, Spacing);
                        nodeContainer.AddControls(node.ChildGroupBox);
                    }
                    else
                    {
                        node.Expanded = false;
                        node.Arrow.SetTexture(UIManager.ArrowRight);
                        nodeContainer.RemoveControls(node.ChildGroupBox);
                    }
                    var parent = node;
                    while (parent is not null)
                    {
                        parent.ChildGroupBox?.AlignTopToBottom(this.Spacing);
                        parent = parent.Parent;
                    }
                    this.AlignTopToBottom(this.Spacing);
                };
                node.Control = nodeContainer;

                foreach (var child in node.Children)
                    queue.Enqueue(child);
            }

            foreach (var child in this.ARoot.Children)
                this.AddControlsBottomLeft(child.Control);
            return this;
        }

        public void Clear()
        {
            this.Controls.Clear();
            this.ARoot.Clear();
        }

        internal bool FindLeafIndex(Control c, out int i)
        {
            i = 0;
            foreach (var item in this.GetEnumerable())
            {
                if (c == item)
                    return true;
                i++;
            }
            return false;
        }
        internal Control GetLeafByIndex(int i)
        {
            var n = 0;
            var enumerator = this.GetEnumerable().GetEnumerator();
            do { enumerator.MoveNext(); } while (n++ != i);
            return enumerator.Current;
        }
        IEnumerable<Control> GetEnumerable()
        {
            var queue = new Queue<ListBoxCollapsibleNode>();
            queue.Enqueue(this.ARoot);
            while (queue.Any())
            {
                var current = queue.Dequeue();
                foreach (var leaf in current.Leafs)
                    yield return leaf;
                foreach (var child in current.Children)
                    queue.Enqueue(child);
            }
        }
    }
}


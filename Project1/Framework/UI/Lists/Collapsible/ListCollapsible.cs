using Microsoft.Xna.Framework;
using Project1.Framework;
using Project1.Framework.UI;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.UI;

public class ListCollapsible<T> : GroupBox
{
    public ListBoxCollapsibleNode ARoot = new("root");
    int Spacing = 0;
    public ListCollapsible()
    {

    }
    Dictionary<ICollapsibleNode<T>, ListBoxCollapsibleNode> _map = [];
    Dictionary<ListBoxCollapsibleNode, ICollapsibleNode<T>> _mapByWrapper = [];
    public void Build(IEnumerable<ICollapsibleNode<T>> rootNodes)
    {
        this.ClearControls();
        foreach (var node in rootNodes)
            BuildRecursive(node);
    }
    ListBoxCollapsibleNode BuildRecursive(ICollapsibleNode<T> node)
    {
        var nodeWrapper = new ListBoxCollapsibleNode(node.GetControl());
        this._map.Add(node, nodeWrapper);
        this._mapByWrapper.Add(nodeWrapper, node);
        node.ChildAdded += Node_ChildAdded;
        node.ChildRemoved += Node_ChildRemoved;
        this.Build(nodeWrapper);
        foreach (var child in node.Children)
        {
            var childNode = BuildRecursive(child);
            childNode.Parent = nodeWrapper;
            nodeWrapper.AddLeaf(childNode);
        }
        this.AddControlsBottomLeft(nodeWrapper.Control);
        return nodeWrapper;
    }

    private void Node_ChildRemoved(ICollapsibleNode<T> node)
    {
        var nodeWrapper = this._map[node];
        var parentNode = node.Parent;
        var parentWrapper = this._map[parentNode];
        parentWrapper.RemoveChild(nodeWrapper);
        this.ResetLayoutFrom(parentWrapper);
    }

    private void Node_ChildAdded(ICollapsibleNode<T> node)
    {
        var nodeControl = node.GetControl();
        node.ChildAdded += Node_ChildAdded;
        node.ChildRemoved += Node_ChildRemoved;
        var nodeWrapper = new ListBoxCollapsibleNode(nodeControl);
        this._map.Add(node, nodeWrapper);
        this._mapByWrapper.Add(nodeWrapper, node);
        this.Build(nodeWrapper);
        var parent = node.Parent;
        var parentWrapper = this._map[parent];
        parentWrapper.AddLeaf(nodeWrapper);
        this.ResetLayoutFrom(parentWrapper);
    }

    private void ResetLayoutFrom(ListBoxCollapsibleNode parentControl)
    {
        var currentParent = parentControl;
        while (currentParent is not null)
        {
            currentParent.ChildrenGroupBox.AlignTopToBottom(this.Spacing);
            currentParent = currentParent.Parent;
        }
        this.AlignTopToBottom(this.Spacing);
    }

    void Expand(ListBoxCollapsibleNode node)
    {
        if (!node.Expanded)
        {
            node.Expanded = true;
            node.Arrow.SetTexture(UIManager.ArrowDown);
            node.ChildrenGroupBox.Location = new Vector2(ListBoxCollapsibleNode.IndentWidth, node.Control.Height + Spacing);
            node.Control.AddControls(node.ChildrenGroupBox);
        }
        else
        {
            node.Expanded = false;
            node.Arrow.SetTexture(UIManager.ArrowRight);
            node.Control.RemoveControls(node.ChildrenGroupBox);
        }
        this.ResetLayoutFrom(node);
    }
    public ListCollapsible<T> AddNode(ListBoxCollapsibleNode node)
    {
        this.ARoot.AddNode(node);
        return this;
    }
    Control Build(ListBoxCollapsibleNode nodeWrapper)
    {
        var node = this._mapByWrapper[nodeWrapper];
        var nodeContainer = new GroupBox() { Name = nodeWrapper.Name, BackgroundColor = UIManager.DefaultListItemBackgroundColor };
        var nodeItem = new GroupBox() { Name = $"{nodeWrapper.Name} content" };
        nodeWrapper.Arrow = new PictureBox(UIManager.ArrowRight) { LeftDownAction = () => Expand(nodeWrapper) };// { LeftClickAction = expand };
        var label = new Label(nodeWrapper.Name) { Active = true };
        var control = nodeWrapper.Control;
            nodeItem.AddControls(nodeWrapper.Arrow);
        if (control is not null)
            nodeItem.AddControlsHorizontally(control, label);
        else
            nodeItem.AddControlsHorizontally(label);
        nodeItem.CenterControlsAlignmentVertically();
        nodeItem.Validate(true);
        nodeContainer.AddControls(nodeItem);
        nodeWrapper.Parent?.ChildControls.Add(nodeContainer);
        nodeWrapper.Parent?.ChildrenGroupBox.Controls.Insert(0, nodeContainer);
        label.LeftClickAction = () => Expand(nodeWrapper);
        nodeWrapper.Control = nodeContainer;
        return nodeContainer;
    }
    //public ListCollapsible<T> Build()
    //{
    //    this.ClearControls();
    //    var queue = new Queue<ListBoxCollapsibleNode>(this.ARoot.Children);

    //    while (queue.Any())
    //    {
    //        var node = queue.Dequeue();
    //        var nodeContainer = new GroupBox() { Name = "container", BackgroundColor = UIManager.DefaultListItemBackgroundColor };
    //        var nodeItem = new GroupBox() { Name = "item" };
    //        node.Arrow = new PictureBox(UIManager.ArrowRight) { LeftDownAction = expand };// { LeftClickAction = expand };
    //        var label = new Label(node.Name) { Active = true };
    //        var control = node.ControlGetter?.Invoke();
    //        if (control is not null)
    //            nodeItem.AddControlsHorizontally(node.Arrow, control, label);
    //        else
    //            nodeItem.AddControlsHorizontally(node.Arrow, label);
    //        nodeItem.CenterControlsAlignmentVertically();
    //        nodeItem.Validate(true);
    //        nodeContainer.AddControls(nodeItem);

    //        node.Parent?.ChildControls.Add(nodeContainer);
    //        node.Parent?.ChildrenGroupBox.Controls.Insert(0, nodeContainer);

    //        label.LeftClickAction = expand;
    //        void expand()
    //        {
    //            if (!node.Expanded)
    //            {
    //                node.Expanded = true;
    //                node.Arrow.SetTexture(UIManager.ArrowDown);
    //                node.ChildrenGroupBox.Location = nodeItem.BottomLeft + new Vector2(ListBoxCollapsibleNode.IndentWidth, Spacing);
    //                nodeContainer.AddControls(node.ChildrenGroupBox);
    //            }
    //            else
    //            {
    //                node.Expanded = false;
    //                node.Arrow.SetTexture(UIManager.ArrowRight);
    //                nodeContainer.RemoveControls(node.ChildrenGroupBox);
    //            }
    //            var parent = node;
    //            while (parent is not null)
    //            {
    //                parent.ChildrenGroupBox?.AlignTopToBottom(this.Spacing);
    //                parent = parent.Parent;
    //            }
    //            this.AlignTopToBottom(this.Spacing);
    //        }
    //        ;
    //        node.Control = nodeContainer;

    //        foreach (var child in node.Children)
    //            queue.Enqueue(child);
    //    }

    //    foreach (var child in this.ARoot.Children)
    //        this.AddControlsBottomLeft(child.Control);
    //    return this;
    //}

    //public void Clear()
    //{
    //    this.Controls.Clear();
    //    this.ARoot.Clear();
    //}

    //internal bool FindLeafIndex(Control c, out int i)
    //{
    //    i = 0;
    //    foreach (var item in this.GetEnumerable())
    //    {
    //        if (c == item)
    //            return true;
    //        i++;
    //    }
    //    return false;
    //}
    //internal Control GetLeafByIndex(int i)
    //{
    //    var n = 0;
    //    var enumerator = this.GetEnumerable().GetEnumerator();
    //    do { enumerator.MoveNext(); } while (n++ != i);
    //    return enumerator.Current;
    //}
    //IEnumerable<Control> GetEnumerable()
    //{
    //    var queue = new Queue<ListBoxCollapsibleNode>();
    //    queue.Enqueue(this.ARoot);
    //    while (queue.Any())
    //    {
    //        var current = queue.Dequeue();
    //        foreach (var leaf in current.Leafs)
    //            yield return leaf;
    //        foreach (var child in current.Children)
    //            queue.Enqueue(child);
    //    }
    //}
}

//public class ListCollapsible<T> : GroupBox
//{
//    public ListBoxCollapsibleNode ARoot = new("root");
//    int Spacing = 0;
//    public ListCollapsible()
//    {

//    }
//    Dictionary<ICollapsibleNode<T>, ListBoxCollapsibleNode> _map = [];
//    public void Build(IEnumerable<ICollapsibleNode<T>> rootNodes)
//    {
//        this.ClearControls();
//        foreach (var node in rootNodes)
//            BuildRecursive(node);
//    }
//    ListBoxCollapsibleNode BuildRecursive(ICollapsibleNode<T> node)
//    {
//        var nodeControl = new ListBoxCollapsibleNode(node.GetControl());
//        node.ChildAdded += Node_ChildAdded;
//        node.ChildRemoved += Node_ChildRemoved;
//        this.Build(nodeControl);
//        foreach (var child in node.Children)
//        {
//            if (child.Children.Any())
//            {
//                var childNode = BuildRecursive(child);
//                childNode.Parent = nodeControl;
//                nodeControl.Children.Add(childNode);
//                nodeControl.AddLeaf(childNode.Control);
//            }
//            else
//                nodeControl.AddLeaf(child.GetControl());
//        }
//        this.AddControlsBottomLeft(nodeControl.Control);
//        this._map.Add(node, nodeControl);
//        return nodeControl;
//    }

//    private void Node_ChildRemoved(ICollapsibleNode<T> node)
//    {
//        var nodeControl = this._map[node];
//        var parent = node.Parent;
//        var parentControl = this._map[parent];
//        if (node.Children.Any())
//            parentControl.RemoveNode(nodeControl);
//        this.ResetLayoutFrom(parentControl);

//    }

//    private void Node_ChildAdded(ICollapsibleNode<T> node)
//    {
//        var nodeControl = node.GetControl();
//        var parent = node.Parent;
//        var parentControl = this._map[parent];
//        if (node.Children.Any())
//        {
//            var nodeControNode = new ListBoxCollapsibleNode(nodeControl);
//            parentControl.AddNode(nodeControNode);// new(nodeControl));
//            this._map.Add(node, nodeControNode);
//        }
//        else
//            parentControl.AddLeaf(nodeControl);

//        this.ResetLayoutFrom(parentControl);
//    }

//    private void ResetLayoutFrom(ListBoxCollapsibleNode parentControl)
//    {
//        var currentParent = parentControl;
//        while (currentParent is not null)
//        {
//            currentParent.ChildrenGroupBox.AlignTopToBottom(this.Spacing);
//            currentParent = currentParent.Parent;
//        }
//        this.AlignTopToBottom(this.Spacing);
//    }

//    void Expand(ListBoxCollapsibleNode node)
//    {
//        if (!node.Expanded)
//        {
//            node.Expanded = true;
//            node.Arrow.SetTexture(UIManager.ArrowDown);
//            node.ChildrenGroupBox.Location = new Vector2(ListBoxCollapsibleNode.IndentWidth, node.Control.Height + Spacing);
//            node.Control.AddControls(node.ChildrenGroupBox);
//        }
//        else
//        {
//            node.Expanded = false;
//            node.Arrow.SetTexture(UIManager.ArrowRight);
//            node.Control.RemoveControls(node.ChildrenGroupBox);
//        }
//        this.ResetLayoutFrom(node);

//        //var parent = node;
//        //while (parent is not null)
//        //{
//        //    parent.ChildrenGroupBox.AlignTopToBottom(this.Spacing);
//        //    parent = parent.Parent;
//        //}
//        //this.AlignTopToBottom(this.Spacing);

//    }
//    public ListCollapsible<T> AddNode(ListBoxCollapsibleNode node)
//    {
//        this.ARoot.AddNode(node);
//        return this;
//    }
//    Control Build(ListBoxCollapsibleNode node)
//    {
//        var nodeContainer = new GroupBox() { Name = node.Name, BackgroundColor = UIManager.DefaultListItemBackgroundColor };
//        var nodeItem = new GroupBox() { Name = $"{node.Name} content" };
//        node.Arrow = new PictureBox(UIManager.ArrowRight) { LeftDownAction = () => Expand(node) };// { LeftClickAction = expand };
//        var label = new Label(node.Name) { Active = true };
//        var control = node.Control;
//        if (control is not null)
//            nodeItem.AddControlsHorizontally(node.Arrow, control, label);
//        else
//            nodeItem.AddControlsHorizontally(node.Arrow, label);
//        nodeItem.CenterControlsAlignmentVertically();
//        nodeItem.Validate(true);
//        nodeContainer.AddControls(nodeItem);
//        node.Parent?.ChildControls.Add(nodeContainer);
//        node.Parent?.ChildrenGroupBox.Controls.Insert(0, nodeContainer);
//        label.LeftClickAction = () => Expand(node);
//        node.Control = nodeContainer;
//        return nodeContainer;
//    }
//    public ListCollapsible<T> Build()
//    {
//        this.ClearControls();
//        var queue = new Queue<ListBoxCollapsibleNode>(this.ARoot.Children);

//        while (queue.Any())
//        {
//            var node = queue.Dequeue();
//            var nodeContainer = new GroupBox() { Name = "container", BackgroundColor = UIManager.DefaultListItemBackgroundColor };
//            var nodeItem = new GroupBox() { Name = "item" };
//            node.Arrow = new PictureBox(UIManager.ArrowRight) { LeftDownAction = expand };// { LeftClickAction = expand };
//            var label = new Label(node.Name) { Active = true };
//            var control = node.ControlGetter?.Invoke();
//            if (control is not null)
//                nodeItem.AddControlsHorizontally(node.Arrow, control, label);
//            else
//                nodeItem.AddControlsHorizontally(node.Arrow, label);
//            nodeItem.CenterControlsAlignmentVertically();
//            nodeItem.Validate(true);
//            nodeContainer.AddControls(nodeItem);

//            node.Parent?.ChildControls.Add(nodeContainer);
//            node.Parent?.ChildrenGroupBox.Controls.Insert(0, nodeContainer);

//            label.LeftClickAction = expand;
//            void expand()
//            {
//                if (!node.Expanded)
//                {
//                    node.Expanded = true;
//                    node.Arrow.SetTexture(UIManager.ArrowDown);
//                    node.ChildrenGroupBox.Location = nodeItem.BottomLeft + new Vector2(ListBoxCollapsibleNode.IndentWidth, Spacing);
//                    nodeContainer.AddControls(node.ChildrenGroupBox);
//                }
//                else
//                {
//                    node.Expanded = false;
//                    node.Arrow.SetTexture(UIManager.ArrowRight);
//                    nodeContainer.RemoveControls(node.ChildrenGroupBox);
//                }
//                var parent = node;
//                while (parent is not null)
//                {
//                    parent.ChildrenGroupBox?.AlignTopToBottom(this.Spacing);
//                    parent = parent.Parent;
//                }
//                this.AlignTopToBottom(this.Spacing);
//            }
//            ;
//            node.Control = nodeContainer;

//            foreach (var child in node.Children)
//                queue.Enqueue(child);
//        }

//        foreach (var child in this.ARoot.Children)
//            this.AddControlsBottomLeft(child.Control);
//        return this;
//    }

//    public void Clear()
//    {
//        this.Controls.Clear();
//        this.ARoot.Clear();
//    }

//    internal bool FindLeafIndex(Control c, out int i)
//    {
//        i = 0;
//        foreach (var item in this.GetEnumerable())
//        {
//            if (c == item)
//                return true;
//            i++;
//        }
//        return false;
//    }
//    internal Control GetLeafByIndex(int i)
//    {
//        var n = 0;
//        var enumerator = this.GetEnumerable().GetEnumerator();
//        do { enumerator.MoveNext(); } while (n++ != i);
//        return enumerator.Current;
//    }
//    IEnumerable<Control> GetEnumerable()
//    {
//        var queue = new Queue<ListBoxCollapsibleNode>();
//        queue.Enqueue(this.ARoot);
//        while (queue.Any())
//        {
//            var current = queue.Dequeue();
//            foreach (var leaf in current.Leafs)
//                yield return leaf;
//            foreach (var child in current.Children)
//                queue.Enqueue(child);
//        }
//    }
//}

using Microsoft.Xna.Framework;
using Project1.Core.Systems.Crafting;
using Project1.Core.Towns.Stockpiles;
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
    public void Build(IEnumerable<ICollapsibleNode<T>> rootNodes)
    {
        this.ClearControls();
        foreach (var node in rootNodes)
            BuildRecursive(node);
    }
    ListBoxCollapsibleNode BuildRecursive(ICollapsibleNode<T> node)
    {
        var nodeControl = new ListBoxCollapsibleNode(node.GetControl());
        node.ChildAdded += Node_ChildAdded;
        this.Build(nodeControl);
        foreach (var child in node.Children)
        {
            if (child.Children.Any())
            {
                var childNode = BuildRecursive(child);
                childNode.Parent = nodeControl;
                nodeControl.Children.Add(childNode);
                nodeControl.AddLeaf(childNode.Control);
            }
            else
                nodeControl.AddLeaf(child.GetControl());// new CheckBoxFinal(child.Label, child.Toggle, child.IsAllowed));
        }
        this.AddControlsBottomLeft(nodeControl.Control);
        this._map.Add(node, nodeControl);
        return nodeControl;
    }

    private void Node_ChildAdded(ICollapsibleNode<T> node)
    {
        var nodeControl = node.GetControl();// new ListBoxCollapsibleNode();
        var parent = node.Parent;
        var parentControl = this._map[parent];
        if (node.Children.Any())
        {
            //var childNode = BuildRecursive(node);
            //childNode.Parent = nodeControl;
            //nodeControl.Children.Add(childNode);
            //nodeControl.AddLeaf(childNode.Control);
            parentControl.AddNode(new(nodeControl));
        }
        else
            parentControl.AddLeaf(nodeControl);// new CheckBoxFinal(child.Label, child.Toggle, child.IsAllowed));
    }

    public void Build(List<IngredientGroup> groups)
    {
        this.ClearControls();
        foreach (var group in groups)
        {
            var groupNode = new ListBoxCollapsibleNode(group.Label);
            foreach (var entry in group.Entries)
            {
                //var entryNode = new ListBoxCollapsibleNode(entry.Label, new CheckBoxNew() { TickedFunc = entry.IsAllowed, LeftClickAction = entry.Toggle });
                var entryNode = new ListBoxCollapsibleNode(entry.Label, new CheckBoxFinal(entry.Toggle, entry.IsAllowed));
                entryNode.Control = Build(entryNode);
                foreach (var child in entry.Children)
                {
                    //var chk = new CheckBoxNew(child.Label) { TickedFunc = child.IsAllowed, LeftClickAction = child.Toggle };
                    var chk = new CheckBoxFinal(child.Label, child.Toggle, child.IsAllowed);
                    entryNode.AddLeaf(chk);
                }
                entryNode.ChildrenGroupBox.AlignTopToBottom();
                entryNode.Parent = groupNode;
                groupNode.Children.Add(entryNode);
                groupNode.AddLeaf(entryNode.Control);
            }
            groupNode.Control = Build(groupNode);
            this.AddControlsBottomLeft(groupNode.Control);
        }
    }
    public void BuildNew(List<IngredientGroup> groups)
    {
        foreach (var group in groups)
            foreach (var entry in group.Entries)
                BuildRecursive(entry);
    }
    ListBoxCollapsibleNode BuildRecursive(IngredientGroupEntry entry)
    {
        var entryNode = new ListBoxCollapsibleNode(entry.Label, new CheckBoxFinal(entry.Toggle, entry.IsAllowed));
        //entryNode.Control = 
        Build(entryNode);
        foreach (var child in entry.Children)
        {
            if (child.Children.Count > 0)
            {
                var childNode = BuildRecursive(child);
                childNode.Parent = entryNode;
                entryNode.Children.Add(childNode);
                entryNode.AddLeaf(childNode.Control);
            }
            else
                entryNode.AddLeaf(new CheckBoxFinal(child.Label, child.Toggle, child.IsAllowed));
        }
        this.AddControlsBottomLeft(entryNode.Control);
        return entryNode;
    }
    void expand(ListBoxCollapsibleNode node)
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

        var parent = node;
        while (parent is not null)
        {
            parent.ChildrenGroupBox.AlignTopToBottom(this.Spacing);
            parent = parent.Parent;
        }
        this.AlignTopToBottom(this.Spacing);

    }
    public ListCollapsible<T> AddNode(ListBoxCollapsibleNode node)
    {
        this.ARoot.AddNode(node);
        return this;
    }
    Control Build(ListBoxCollapsibleNode node)
    {
        var nodeContainer = new GroupBox() { Name = node.Name, BackgroundColor = UIManager.DefaultListItemBackgroundColor };
        var nodeItem = new GroupBox() { Name = $"{node.Name} content" };
        node.Arrow = new PictureBox(UIManager.ArrowRight) { LeftDownAction = () => expand(node) };// { LeftClickAction = expand };
        var label = new Label(node.Name) { Active = true };
        var control = node.Control;
        if (control is not null)
            nodeItem.AddControlsHorizontally(node.Arrow, control, label);
        else
            nodeItem.AddControlsHorizontally(node.Arrow, label);
        nodeItem.CenterControlsAlignmentVertically();
        nodeItem.Validate(true);
        nodeContainer.AddControls(nodeItem);
        node.Parent?.ChildControls.Add(nodeContainer);
        node.Parent?.ChildrenGroupBox.Controls.Insert(0, nodeContainer);
        label.LeftClickAction = () => expand(node);
        node.Control = nodeContainer;
        return nodeContainer;
    }
    public ListCollapsible<T> Build()
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
            node.Parent?.ChildrenGroupBox.Controls.Insert(0, nodeContainer);

            label.LeftClickAction = expand;
            void expand()
            {
                if (!node.Expanded)
                {
                    node.Expanded = true;
                    node.Arrow.SetTexture(UIManager.ArrowDown);
                    node.ChildrenGroupBox.Location = nodeItem.BottomLeft + new Vector2(ListBoxCollapsibleNode.IndentWidth, Spacing);
                    nodeContainer.AddControls(node.ChildrenGroupBox);
                }
                else
                {
                    node.Expanded = false;
                    node.Arrow.SetTexture(UIManager.ArrowRight);
                    nodeContainer.RemoveControls(node.ChildrenGroupBox);
                }
                var parent = node;
                while (parent is not null)
                {
                    parent.ChildrenGroupBox?.AlignTopToBottom(this.Spacing);
                    parent = parent.Parent;
                }
                this.AlignTopToBottom(this.Spacing);
            }
            ;
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


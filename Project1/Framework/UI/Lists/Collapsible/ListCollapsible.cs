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
            nodeWrapper.AddNode(childNode);
        }
        if (node.Children.Any())
            nodeWrapper.ShowArrow();
        nodeWrapper.Control.Validate(true);
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
        parentWrapper.AddNode(nodeWrapper);
        parentWrapper.ShowArrow();
        this.ResetLayoutFrom(parentWrapper);
    }

    private void ResetLayoutFrom(ListBoxCollapsibleNode parentControl)
    {
        var currentParent = parentControl;
        while (currentParent is not null)
        {
            currentParent.Control.AlignTopToBottom(this.Spacing);
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
            node.ChildrenGroupBox.AlignTopToBottom(Spacing);
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
    //public ListCollapsible<T> AddNode(ListBoxCollapsibleNode node)
    //{
    //    this.ARoot.AddNodeOld(node);
    //    return this;
    //}
    void Build(ListBoxCollapsibleNode nodeWrapper)
    {
        var node = this._mapByWrapper[nodeWrapper];
        var nodeItem = nodeWrapper.Control;
        nodeWrapper.Arrow.LeftClickAction = () => Expand(nodeWrapper);
        nodeItem.AlignHorizontally();
        nodeItem.Validate(true);
    }
    
}

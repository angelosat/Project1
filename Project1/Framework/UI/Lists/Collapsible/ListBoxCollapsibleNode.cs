using Microsoft.Xna.Framework;
using Project1.Framework;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;

namespace Project1.Core.UI;

public class ListBoxCollapsibleNode
{
    public List<Control> Leafs = new();
    public Control Control;
    public List<Control> LeafControls = new();
    public List<Control> ChildControls = new();
    public ListBoxCollapsibleNode Parent;
    public List<ListBoxCollapsibleNode> Children = new();
    public Func<Control> ControlGetter;
    public PictureBox Arrow;
    public GroupBox ChildrenGroupBox = new() { BackgroundColor = Color.Red * .2f };
    public static readonly int IndentWidth = UIManager.ArrowRight.Rectangle.Width;

    public string Name;
    public bool Expanded;

    public ListBoxCollapsibleNode(Control control)
    {
        this.Control = control;
    }

    public ListBoxCollapsibleNode(IListCollapsibleDataSource node)
    {
        this.Name = node.LabelReadable;
        this.ControlGetter = () => node.GetListControlGui();

        foreach (var child in node.ListBranches)
            this.AddNode(new ListBoxCollapsibleNode(child));
        foreach (var leaf in node.ListLeafs)
            this.AddLeaf(leaf.GetListControlGui());
    }
    public ListBoxCollapsibleNode(string name)
    {
        this.Name = name;
        this.ChildrenGroupBox.Name = name;
    }
    public ListBoxCollapsibleNode(string name, Func<ListBoxCollapsibleNode, Control> controlGetter) : this(name)
    {
        this.ControlGetter = () => controlGetter(this);
    }

    public ListBoxCollapsibleNode(string name, Control control) : this(name)
    {
        this.Control = control;
    }

    public ListBoxCollapsibleNode AddNode(ListBoxCollapsibleNode node)
    {
        this.Children.Add(node);
        node.Parent = this;
        return this;
    }
    public void RemoveNode(ListBoxCollapsibleNode node)
    {
        this.Children.Remove(node);
    }
 
   
    public ListBoxCollapsibleNode AddLeaf(ListBoxCollapsibleNode leaf)
    {
        this.Children.Add(leaf);
        this.ChildrenGroupBox.AddControlsBottomLeft(leaf.Control);
        leaf.Control.Validate(true);
        return this;
    }
    [Obsolete]
    public ListBoxCollapsibleNode AddLeaf(Control leaf)
    {
        throw new Exception();
    }
    public ListBoxCollapsibleNode RemoveChild(ListBoxCollapsibleNode child)
    {
        this.Children.Remove(child);
        this.ChildrenGroupBox.RemoveControls(child.Control);
        return this;
    }
    public ListBoxCollapsibleNode Clear()
    {
        this.Children.Clear();
        return this;
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
}

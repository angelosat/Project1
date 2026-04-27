using Project1.Framework;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;

namespace Project1.Core.Systems.Crafting;

sealed class ItemFilterNode : ICollapsibleNode<Def>
{
    Def Def;
    ItemFilterNode _parent;
    List<ItemFilterNode> _children = [];
    public string Label => this.Def.LabelReadable;

    public ICollapsibleNode<Def> Parent => this._parent;

    public IEnumerable<ICollapsibleNode<Def>> Children => this._children;

    public event Action<ICollapsibleNode<Def>> ChildAdded;
    public event Action<ICollapsibleNode<Def>> ChildRemoved;

    public Control GetControl()
        => new GroupBox().AddControlsHorizontally(
            new CheckBoxFinalNew(() => { }, () => true),
            new LabelNew(() => this.Label));
}

public record IngredientGroupEntry
{
    internal string Label;
    internal List<IngredientGroupEntry> Children = [];
    internal Action Toggle;
    internal Func<bool> IsAllowed;
}

public sealed class IngredientGroupEntryNew : ICollapsibleNode<Def>
{
    internal string Label;
    internal List<IngredientGroupEntryNew> Children = [];
    internal Action Toggle;
    internal Func<bool> IsAllowed;
    IngredientGroupEntryNew _parent;

    public ICollapsibleNode<Def> Parent => this._parent;

    string ICollapsibleNode<Def>.Label => this.Label;

    IEnumerable<ICollapsibleNode<Def>> ICollapsibleNode<Def>.Children => this.Children;

    public event Action<ICollapsibleNode<Def>> ChildAdded;
    public event Action<ICollapsibleNode<Def>> ChildRemoved;

    public Control GetControl()
          => this.Toggle is not null 
                ? new GroupBox().AddControlsHorizontally(
                    new CheckBoxFinalNew(this.Toggle, this.IsAllowed),
                    new LabelNew(() => this.Label))
                : new LabelNew(() => this.Label);
}

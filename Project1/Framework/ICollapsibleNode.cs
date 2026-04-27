using Project1.Framework.UI;
using System;
using System.Collections.Generic;

namespace Project1.Framework;

public interface ICollapsibleNode<T>
{
    string Label { get; }
    ICollapsibleNode<T> Parent { get; }
    IEnumerable<ICollapsibleNode<T>> Children { get; }
    Control GetControl();
    event Action<ICollapsibleNode<T>> ChildAdded;
    event Action<ICollapsibleNode<T>> ChildRemoved;
    //event Action<ICollapsibleNode<T>> Updated;
}

using Start_a_Town_.UI;
using System;

namespace Start_a_Town_
{
    public interface IListable
    {
        string Label { get; }
        Control GetListControlGui();// Action<IListable> callback);
    }
}

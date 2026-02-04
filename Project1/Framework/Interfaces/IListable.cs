using Project1.Framework.UI;
using System;

namespace Project1.Framework.Interfaces
{
    public interface IListable
    {
        string Label { get; }
        Control GetListControlGui();// Action<IListable> callback);
    }
}

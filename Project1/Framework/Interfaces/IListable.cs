
using Project1.Core.UI;

namespace Project1.Core.Interfaces
{
    public interface IListable
    {
        string Label { get; }
        Control GetListControlGui();// Action<IListable> callback);
    }
}

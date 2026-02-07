using System.Collections.Generic;
using System.Collections.ObjectModel;
using Project1.Core.Interfaces;
using Project1.Core.UI;

namespace Project1.Core
{
    public interface IListCollapsibleDataSource : IListable
    {
        IEnumerable<IListCollapsibleDataSource> ListBranches { get; }
        IEnumerable<IListable> ListLeafs { get; }
        Control GetGui();
    }
    public interface IListCollapsibleDataSourceObservable : IListable
    {
        ObservableCollection<IListCollapsibleDataSourceObservable> ListBranches { get; }
        ObservableCollection<IListable> ListLeafs { get; }
        Control GetGui();
    }
}

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Project1.Framework.Interfaces;
using Project1.Framework.UI;

namespace Start_a_Town_
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

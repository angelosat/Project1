using Project1.Core.Interfaces;
using System;

namespace Project1.Core.UI
{
    interface IListSearchable<TObject> 
    {
        void Filter(Func<TObject, bool> filter);
    }
    interface IListSearchable
    {
        void Filter(Func<IListable, bool> filter);
    }
}

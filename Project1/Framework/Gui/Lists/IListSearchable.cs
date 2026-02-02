using System;

namespace Start_a_Town_.UI
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

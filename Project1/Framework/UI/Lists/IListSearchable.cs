using System;

namespace Project1.Framework.UI
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

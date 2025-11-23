using System;

namespace Start_a_Town_.UI
{
    class SelectableItemList<T> : GroupBox where T : INamed
    {
        Action<T> _SelectAction = (i) => { };
        public Action<T> SelectAction
        {
            get
            {
                return this._SelectAction;
            }
            set
            {
                this._SelectAction = value;
            }
        }
    }
}

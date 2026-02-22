using Project1.Framework.Interfaces;
using System;

namespace Project1.Framework.UI
{
    class SelectableItemList<T> : GroupBox// where T : INamed
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

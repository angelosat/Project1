using Project1.Core.Interfaces;
using Project1.Core.UI;
using System;

namespace Project1.Core.UI
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

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class ButtonList<T> : ScrollableBoxNew where T : class
    {
        public ButtonList(IEnumerable<T> list, int w, int h, Func<T, string> labelGetter, Action<T, Button> btnInit)
            : base(new Rectangle(0, 0, w, h))
        {
            var i = 0;
            foreach (var item in list)
            {
                var btn = new Button(labelGetter(item), w);
                btnInit(item, btn);
                btn.Location = new Vector2(0, i);
                i += btn.Height;
                this.Add(btn);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Project1.Core.UI;

namespace Project1.Core.UI
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

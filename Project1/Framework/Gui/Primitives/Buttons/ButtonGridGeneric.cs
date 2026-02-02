using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class ButtonGridGeneric<TagType, ButtonType> : ScrollableBoxNew
        where TagType : class
        where ButtonType : ButtonBase, new()
    {
        public ButtonGridGeneric(Rectangle bounds, IEnumerable<TagType> tags, int lineMax, Action<ButtonType, TagType> btnInit)
            : base(bounds)
        {
            var lastx = 0;
            var lasty = 0;

            foreach (var tag in tags)
            {
                var btn = new ButtonType();
                
                btnInit(btn, tag);
                btn.Location = new Vector2(lastx, lasty);
                lastx += btn.Width;
                this.Add(btn);
                if (this.Client.Controls.Count % lineMax == 0)
                {
                    lastx = 0;
                    lasty += btn.Height;
                }
            }
        }
    }
}

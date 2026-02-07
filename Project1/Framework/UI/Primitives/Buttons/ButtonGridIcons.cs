using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Project1.Core.UI;

namespace Project1.Core.UI
{
    class ButtonGridIcons<TagType> : ScrollableBoxNew
      where TagType : class
    {
        public ButtonGridIcons(int visibleHor, int visibleVer, IEnumerable<TagType> tags, Action<IconButton, TagType> btnInit)
            : this(new Rectangle(0, 0, UIManager.DefaultIconButtonSprite.Width * visibleHor, UIManager.DefaultIconButtonSprite.Height * visibleVer), tags, visibleHor, btnInit)
        { }
        public ButtonGridIcons(Rectangle bounds, IEnumerable<TagType> tags, int lineMax, Action<IconButton, TagType> btnInit)
            : base(bounds)
        {
            var lastx = 0;
            var lasty = 0;

            foreach (var tag in tags)
            {
                var btn = new IconButton()
                {
                    BackgroundTexture = UIManager.DefaultIconButtonSprite
                };
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

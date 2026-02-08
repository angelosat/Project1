using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Framework.UI;

namespace Project1.Core.UI
{
    static class ButtonHelper
    {
        static public Button CreateFromItemCompact(Entity obj)
        {
            var btn = new Button();
            var pic = new PictureBox(obj, .5f) { MouseThrough = true };
            var lbl = new Label(obj.Name) { MouseThrough = true, Location = new Vector2(pic.Right, btn.Height / 2) };
            lbl.Location.Y -= lbl.Height / 2;
            btn.AddControls(pic, lbl);
            return btn;
        }
    }
}

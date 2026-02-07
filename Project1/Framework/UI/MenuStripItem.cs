using Microsoft.Xna.Framework;
using Project1.Core.UI;

namespace Project1.Core.UI
{
    class MenuStripItem : Button
    {
        public new MenuStrip Parent;
        public Panel Dropdown;

        public MenuStripItem(MenuStrip parent) : base()
        {
            this.Parent = parent;
            this.Dropdown = new Panel()
            {
                AutoSize = true,
                LocationFunc = () =>
                {
                    return this.BottomLeft;
                }
            };
            Color = Color.White * 0.5f;
            IdleColor = Color.Transparent;
        }

        protected override void OnLeftClick()
        {
            Parent.Open();
            Parent.Activate(this);
        }

        public override void OnMouseEnter()
        {
            base.OnMouseEnter();
            if(Parent.IsMenuOpen)
            Parent.Activate(this);
        }
    }
}

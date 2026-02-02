using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.UI
{   
    class MenuStrip : GroupBox
    {
        public class MenuSripItemCollection : List<MenuStripItem>
        {
            MenuStrip Parent;
            public MenuSripItemCollection(MenuStrip parent)
            {
                this.Parent = parent;
            }
            public void Add(params MenuStripItem[] items)
            {
                foreach (var item in items)
                {
                    base.Add(item);
                    Parent.Controls.Add(item);
                }
            }
        }

        public MenuSripItemCollection Items;

        public bool IsMenuOpen;
        public void Open()
        {
            IsMenuOpen = true;
        }
        public void Activate(MenuStripItem item)
        {
            var items = this.Items.Except(new MenuStripItem[] { item }).ToList();
            foreach (var other in items)
            {
                if (other.Dropdown.Controls.Count > 0)
                    other.Dropdown.Hide();
            }
            if (item.Dropdown.Controls.Count > 0)
                item.Dropdown.Show();
        }

        public MenuStrip()
        {
            this.Items = new MenuSripItemCollection(this);
        }

        public override void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            IsMenuOpen = false;
            foreach (var item in this.Items)
            {
                if (item.Dropdown.Controls.Count > 0)
                    item.Dropdown.Hide();
            }
            base.HandleLButtonUp(e);
        }
    }
}

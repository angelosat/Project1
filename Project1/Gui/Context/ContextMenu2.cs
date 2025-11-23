using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class ContextMenu2 : Panel
    {
        ListBoxNoScroll<ContextAction, Button> Panel_List;

        static ContextMenu2 _Instance;
        public static ContextMenu2 Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new ContextMenu2();
                return _Instance;
            }
        }

        ContextMenu2()
        {
            AutoSize = true;
            Panel_List = new ListBoxNoScroll<ContextAction, Button>(act =>
            {
                var btn = new Button(act.Name(), delegate { });
                btn.Color = Color.White * 0.5f;
                btn.IdleColor = Color.Transparent;
                act.ControlInit(act, btn);
                //btn.LeftClickAction += ()=>act.Action();
                btn.LeftClickAction = () => act.Action();
                btn.LeftClickAction += () => this.Hide();
                return btn;
            });// new Rectangle(0, 0, 100, 5));
        }

        public void Initialize(ContextArgs a)
        {
            var actions = a.Actions;
            if (actions.Count == 0)
                return;
            this.Controls.Clear();
            this.Panel_List.Clear();
            this.Panel_List.AddItems(a.Actions);
            this.Controls.Add(Panel_List);
            //this.Location = UIManager.Mouse - ClientLocation - Panel_List.Location - (Panel_List.Controls.Count > 0 ? Panel_List.Controls.First().Location : Vector2.Zero) - new Vector2(Label.DefaultHeight / 2);
            this.SnapToMouse();
            this.Show();
            this.Invalidate();
            this.Texture = null;
        }
    }
}

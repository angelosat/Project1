using Microsoft.Xna.Framework;
using System;

namespace Start_a_Town_.UI
{
    class MessageBox : Window
    {
        readonly Panel Panel_Text, Panel_Buttons;

        public static MessageBox Create(string title, string text, string b1text, Action b1action)
        {
            return new MessageBox(title, text, b1text, b1action);
        }
        public static MessageBox Create(string title, string text, Action yesAction = null, Action noAction = null)
        {
            return new MessageBox(title, text, yesAction, noAction);
        }
        public static MessageBox CreateDialogue(string title, string text, Action yesAction = null, Action noAction = null)
        {
            var box = Create(title, text, yesAction, noAction);
            box.ShowDialog();
            return box;
        }
        MessageBox(string title, string text, string b1text, Action b1action)
        {
            this.AutoSize = true;
            this.Title = title;
            this.AutoSize = true;

            int width = 600;

            this.Panel_Text = new Panel();
            this.Panel_Text.AutoSize = true;

            var label = new Label(new Vector2(0), StringHelper.Wrap(text, width));
            this.Panel_Text.Controls.Add(label);
            this.Panel_Text.ClientSize = new Rectangle(0, 0, width, Math.Max(label.Height, UIManager.SlotSprite.Height));

            this.Panel_Buttons = new Panel(new Vector2(0, this.Panel_Text.Bottom));
            this.Panel_Buttons.ClientSize = new Rectangle(0, 0, this.Panel_Text.ClientSize.Width, Button.DefaultHeight);

            Button btn = new Button(b1text, this.Panel_Buttons.ClientSize.Width) { LeftClickAction = b1action };

            this.Panel_Buttons.Controls.Add(btn);
            this.Client.Controls.Add(this.Panel_Text, this.Panel_Buttons);

            this.SnapToScreenCenter();
        }

        public MessageBox(string title, string text, Action yesAction = null, Action noAction = null)
        {
            this.Title = title;
            this.AutoSize = true;
            this.Closable = false;

            int width = 200;
            this.Panel_Text = new Panel();
            this.Panel_Text.AutoSize = true;

            var label = new Label(new Vector2(0), StringHelper.Wrap(text, width));
            this.Panel_Text.Controls.Add(label);
            this.Panel_Text.ClientSize = new Rectangle(0, 0, width, Math.Max(label.Height, UIManager.SlotSprite.Height));

            this.Panel_Buttons = new Panel(new Vector2(0, this.Panel_Text.Bottom)) { AutoSize = true };
            this.Panel_Buttons.ClientSize = new Rectangle(0, 0, this.Panel_Text.ClientSize.Width, Button.DefaultHeight);

            Button Yes = new Button(Vector2.Zero, width / 2, "Yes") { LeftClickAction = () => { this.Hide(); if (yesAction != null) yesAction(); } };
            Button No = new Button(new Vector2(Yes.Right, 0), width / 2, "No") { LeftClickAction = () => { this.Hide(); if (noAction != null) noAction(); } };
            this.Panel_Buttons.Controls.Add(Yes, No);

            this.Client.Controls.Add(this.Panel_Text, this.Panel_Buttons);
        }
        public MessageBox(string title, string text, params ContextAction[] actions)
        {
            this.Title = title;
            this.AutoSize = true;
            this.Closable = false;

            int width = 200;
            this.Panel_Text = new Panel();
            this.Panel_Text.AutoSize = true;
            var label = new Label(new Vector2(0), StringHelper.Wrap(text, width)) { Width = width };
            this.Panel_Text.Controls.Add(label);
            this.Panel_Buttons = new Panel(new Vector2(0, this.Panel_Text.Bottom)) { AutoSize = true };

            this.CreateButtonsHor(actions, width);

            this.Client.Controls.Add(this.Panel_Text, this.Panel_Buttons);
        }

        // TODO refactor to button helper class
        private void CreateButtonsHor(ContextAction[] actions, int width)
        {
            var w = width / actions.Length;
            foreach (var action in actions)
            {
                Button btn = new Button(action.Name(), w) { LeftClickAction = () => { action.Action(); this.Hide(); } };
                btn.Location = this.Panel_Buttons.Controls.TopRight;
                this.Panel_Buttons.AddControlsTopRight(btn);
            }
        }
    }
}

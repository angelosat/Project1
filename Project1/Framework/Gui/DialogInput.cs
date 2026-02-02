using Microsoft.Xna.Framework;
using System;

namespace Start_a_Town_.UI
{
    class DialogInput : Window
    {
        readonly Panel Panel_Input, Panel_Buttons;
        readonly TextBox Txt_Input;
        readonly Button Btn_Accept, Btn_Cancel;
        public string Input => this.Txt_Input.Text;

        public DialogInput(string title, Action<string> callback, int maxlength = int.MaxValue, string initialText = "")
        {
            this.Title = title;
            this.AutoSize = true;

            this.Panel_Input = new Panel();
            this.Panel_Input.AutoSize = true;
            this.Panel_Input.BackgroundStyle = BackgroundStyle.TickBox;

            this.Panel_Buttons = new Panel(this.Panel_Input.BottomLeft);
            this.Panel_Buttons.AutoSize = true;

            this.Txt_Input = new TextBox(initialText, 300) { MaxLength = maxlength, EnterFunc = callback, BackgroundStyle = BackgroundStyle.TickBox };

            this.Panel_Input.Controls.Add(this.Txt_Input);

            this.Panel_Buttons = new Panel(this.Panel_Input.BottomLeft);
            this.Panel_Buttons.AutoSize = true;
            this.Btn_Accept = new Button(Vector2.Zero, 50, "Done") { LeftClickAction = () => callback(this.Txt_Input.Text) };
            this.Btn_Cancel = new Button(this.Btn_Accept.TopRight, 50, "Cancel");
            this.Panel_Buttons.Controls.Add(this.Btn_Accept);

            this.Panel_Buttons.Location = this.Panel_Input.BottomCenter;
            this.Panel_Buttons.Anchor = Vector2.UnitX / 2;

            this.Client.Controls.Add(this.Panel_Input, this.Panel_Buttons);
            //this.SnapToScreenCenter();
            this.AnchorToScreenCenter();
        }
        public DialogInput(string title, Action<DialogInput> callback, int maxlength = int.MaxValue, string initialText = "")
        {
            this.Title = title;
            this.AutoSize = true;

            this.Panel_Input = new Panel();
            this.Panel_Input.AutoSize = true;
            this.Panel_Input.BackgroundStyle = BackgroundStyle.TickBox;

            this.Panel_Buttons = new Panel(this.Panel_Input.BottomLeft);
            this.Panel_Buttons.AutoSize = true;

            this.Txt_Input = new TextBox(initialText, 300) { MaxLength = maxlength, EnterFunc = txt => callback(this), BackgroundStyle = BackgroundStyle.TickBox };

            this.Panel_Input.Controls.Add(this.Txt_Input);

            this.Panel_Buttons = new Panel(this.Panel_Input.BottomLeft);
            this.Panel_Buttons.AutoSize = true;
            this.Btn_Accept = new Button(Vector2.Zero, 50, "Done") { LeftClickAction = () => callback(this) };
            this.Btn_Cancel = new Button(this.Btn_Accept.TopRight, 50, "Cancel");
            this.Panel_Buttons.Controls.Add(this.Btn_Accept);

            this.Panel_Buttons.Location = this.Panel_Input.BottomCenter;
            this.Panel_Buttons.Anchor = Vector2.UnitX / 2;

            this.Client.Controls.Add(this.Panel_Input, this.Panel_Buttons);
            //this.SnapToScreenCenter();
            this.AnchorToScreenCenter();
        }
        public delegate bool Validator<TU>(string text, out TU output);
        const int GUIWidth = 128;
        public static Window ShowInputDialog<T>(string title, Action<T> callback, Validator<T> validator, int maxlength = int.MaxValue, string initialText = "")
        {
            var box = new GroupBox();
            var Panel_Input = new Panel();
            Panel_Input.AutoSize = true;
            Panel_Input.BackgroundStyle = BackgroundStyle.TickBox;

            var Panel_Buttons = new Panel(Panel_Input.BottomLeft);
            Panel_Buttons.AutoSize = true;

            var txt_Input = new TextBox(Vector2.Zero, new Vector2(GUIWidth, Label.DefaultHeight))
            {
                MaxLength = maxlength,
                Text = initialText,
                EnterFunc = (txt) => NewMethod(callback, validator, txt)
            };

            txt_Input.BackgroundStyle = BackgroundStyle.TickBox;

            var Btn_Accept = new Button(Vector2.Zero, GUIWidth, "Done") { LeftClickAction = () => NewMethod(callback, validator, txt_Input.Text) };
            var Btn_Cancel = new Button(Btn_Accept.TopRight, 50, "Cancel");

            box.AddControlsVertically(
                txt_Input.ToPanel(),
                Btn_Accept.ToPanel()
                );
            var win = box.ToWindow(title);
            win.ShowDialog();
            txt_Input.Select();
            return win;

            void NewMethod(Action<T> callback, Validator<T> validator, string txt)
            {
                if (validator(txt, out T val))
                {
                    callback(val);
                    box.GetWindow().Hide();
                }
            }
        }
        public static Window ShowInputDialog(string title, Action<string> callback, int maxlength = int.MaxValue, string initialText = "")
        {
            return ShowInputDialog<string>(title, callback, (string txt, out string v) => { v = txt; return true; }, maxlength, initialText);
        }

        public override bool Show()
        {
            this.Txt_Input.Select();
            return base.Show();
        }
        public DialogInput SetText(string text)
        {
            this.Txt_Input.Text = text;
            return this;
        }
    }
}

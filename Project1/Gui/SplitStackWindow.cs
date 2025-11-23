using Microsoft.Xna.Framework;
using System;

namespace Start_a_Town_.UI
{
    class SplitStackWindow : Window
    {
        static SplitStackWindow _instance;
        public static SplitStackWindow Instance => _instance ??= new SplitStackWindow();

        SliderNew Sldr_Amount;
        Action<int> SplitAction;
        readonly Panel Panel_Input;
        readonly TextBox Txt_Amount;
        readonly Button Btn_Split;
        public new GameObjectSlot Tag;
        GameObjectSlot Copy;
        SplitStackWindow()
        {
            this.AutoSize = true;

            this.Panel_Input = new Panel();
            this.Panel_Input.AutoSize = true;
            this.Panel_Input.BackgroundStyle = BackgroundStyle.TickBox;

            this.Title = "Split";

            float w = Math.Max(70, TitleLocation.X + UIManager.Font.MeasureString(this.Title).X);
            this.Txt_Amount = new TextBox(Vector2.Zero, new Vector2(w, Label.DefaultHeight))
            {
                Text = "1"
            };
            this.Txt_Amount.TextEntered += new EventHandler<TextEventArgs>(this.Txt_Amount_TextEntered);
            this.Panel_Input.Controls.Add(this.Txt_Amount);

            this.Btn_Split = new Button(new Vector2(0, this.Panel_Input.Bottom), width: this.Panel_Input.Width, label: "Split")
            {
                LeftClickAction = () => this.Split()
            };
            this.Client.Controls.Add(this.Panel_Input, this.Btn_Split);
            this.Location = this.CenterMouseOnControl(this.Btn_Split);
        }

        void Split()
        {
            this.SplitAction(short.Parse(this.Txt_Amount.Text));
            this.Hide();
        }

        public override void Update()
        {
            base.Update();
            if ((this.Tag.Object != this.Copy.Object) ||
                (this.Tag.StackSize != this.Copy.StackSize))
                this.Hide();
        }

        public SplitStackWindow Refresh(TargetArgs slotTarget)
        {
            this.SplitAction = (a) =>
            {
                int count = short.Parse(this.Txt_Amount.Text);
                DragDropManager.Create(new DragDropSlot(null, slotTarget, new TargetArgs(new GameObjectSlot(slotTarget.Slot.Object.Clone(), count)), DragDropEffects.Move | DragDropEffects.Link));
            };

            this.Tag = slotTarget.Slot;
            this.Copy = new GameObjectSlot(slotTarget.Slot.Object, slotTarget.Slot.StackSize);

            this.Title = slotTarget.Slot.ToString();
            int w = (int)Math.Max(70, TitleLocation.X + UIManager.Font.MeasureString(this.Title).X);
            int amount = (int)(slotTarget.Slot.StackSize / 2f);
            this.Txt_Amount.Width = w;
            this.Txt_Amount.TextFunc = () => $"{amount}";
            this.Panel_Input.Controls.Remove(this.Txt_Amount);
            this.Panel_Input.Controls.Add(this.Txt_Amount);

            this.Sldr_Amount = new SliderNew(() => amount, v => amount = (int)v, this.Panel_Input.Width, 1, slotTarget.Slot.StackSize, 1)
            {
                Location = this.Panel_Input.BottomLeft,
                HoverFunc = () => "Split to: " + this.Sldr_Amount.Value.ToString(),
            };

            this.Btn_Split.Location = this.Sldr_Amount.BottomLeft;
            this.Btn_Split.Width = this.Panel_Input.Width;

            this.Client.Controls.Clear();

            this.Client.Controls.Add(this.Panel_Input, this.Btn_Split, this.Sldr_Amount);
            this.Txt_Amount.Enabled = true;
            this.Location = this.CenterMouseOnControl(this.Btn_Split);

            return this;
        }

        void Txt_Amount_TextEntered(object sender, TextEventArgs e)
        {
            if (e.Char == '\r')//13) //enter
            {
                this.Btn_Split.PerformLeftClick();
                return;
            }

            string newAmount = this.Txt_Amount.Text + (char.IsDigit(e.Char) ? e.Char.ToString() : "");
            if (!int.TryParse(newAmount, out int amount))
                amount = 1;
            else
                amount = (int)MathHelper.Clamp(short.Parse(newAmount), 1, this.Tag.StackSize);

            this.Txt_Amount.Text = amount.ToString();
            this.Sldr_Amount.Value = amount;
        }
    }
}

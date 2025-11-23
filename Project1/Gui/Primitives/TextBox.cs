using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading;
using System.Windows.Forms;

namespace Start_a_Town_.UI
{
    public class TextEventArgs : EventArgs
    {
        public char Char;
        public TextEventArgs(char c)
        {
            this.Char = c;
        }
    }

    public class TextBox : Label
    {
        const int CursorWidth = 1; //3;
        int _SelectionStart = 0;
        bool CursorVisible;
        public Func<char, bool> InputFilter = c => char.IsWhiteSpace(c) || char.IsLetterOrDigit(c);// c => true;
        public Action<KeyPressEventArgs> TextEnterFunc = (e) => { };
        public Action<string> EnterFunc = (text) => { };
        public Action<KeyPressEventArgs> EscapeFunc = (e) => { };
        public Action<string> TextChangedFunc = txt => { };
        public int MaxLength = int.MaxValue;
        int CursorTimer = CursorTimerMax;
        int _cursorPosition;// = int.MaxValue;
        int CursorPosition
        {
            get => this._cursorPosition;
            set => this._cursorPosition = Math.Max(0, Math.Min(this.Text.Length, value));
        }
        public override string Text
        {
            get => base.Text; 
            set => base.Text = value;
        }
        protected override void OnTextChanged()
        {
            this.Invalidate();
        }
        public event EventHandler<TextEventArgs> TextEntered;
        protected virtual void OnTextEntered(char c)
        {
            TextEntered?.Invoke(this, new TextEventArgs(c));
            this.TextChangedFunc(this.Text);
        }

        public int SelectionStart
        {
            get => this._SelectionStart;
            set => this._SelectionStart = value;
        }

        public TextBox() : base()
        {
            this.Active = true;
            this.BackgroundColor = Color.Black * 0.5f;
            this.BackgroundColor = Color.White * .1f;
        }
        public TextBox(Vector2 position) : this() { this.Location = position; }
        public TextBox(Vector2 position, Vector2 size)
            : this(position)
        {
            this.Width = (int)size.X;
            this.Height = (int)size.Y;
        }
        public TextBox(int width)
            : this()
        {
            this.Width = width;
            this.Height = DefaultHeight;
        }
        public TextBox(string initialText, int width)
            : this()
        {
            this.Text = initialText;
            this.Width = width;
            this.Height = DefaultHeight;
            this.CursorPosition = this.Text.Length;
        }
        static readonly int CursorTimerMax = (int)(Ticks.PerSecond / 2f);
        public override void Update()
        {
            base.Update();
            if (!this.Enabled)
                return;
            this.CursorTimer--;
            if (this.CursorTimer <= 0)
            {
                this.CursorTimer_Tick();
                this.CursorTimer = CursorTimerMax;
            }
        }
        void CursorTimer_Tick()
        {
            this.CursorVisible = !this.CursorVisible;
            //this.Invalidate();
        }

        public override int Height
        {
            get => UIManager.LineHeight;
            set => base.Height = value;
        }

        public override void Select()
        {
            this.Enabled = true;
            this.CursorPosition = this.Text.Length;
            base.Select();
        }

        public override void Unselect()
        {
            this.Enabled = false;
            base.Unselect();
        }

        public override void OnLostFocus()
        {
            this.Enabled = false;
            base.OnLostFocus();
        }

        bool _Enabled;
        public bool Enabled
        {
            get => this._Enabled;
            set
            {
                bool old = this._Enabled;
                this._Enabled = value;
                if (this._Enabled)
                {
                    if (!old)
                    {
                        this.CursorTimer = CursorTimerMax;
                        this.CursorVisible = true;
                        this.CursorPosition = Math.Min(this.CursorPosition, this.Text.Length);
                    }
                }
                else
                    this.CursorVisible = false;
                this.Invalidate();
            }
        }
        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
            if (this.CursorVisible && this.Enabled)
            {
                var textbounds = UIManager.Font.MeasureString(this.Text.Substring(0, this.CursorPosition));
                sb.Draw(UIManager.Highlight, new Rectangle(this.BoundsScreen.X + (int)textbounds.X, this.BoundsScreen.Y, CursorWidth, this.Height), Color.White);
            }
        }
        public override void Dispose()
        {
            this.Texture.Dispose();
        }

        public void AppendText(string text)
        {
            this.Text += text;
        }

        public Action<int> HistoryNextPrev = i => { };

        public override void HandleKeyDown(KeyEventArgs e)
        {
            if (!this.Enabled)
                return;
            if (e.Handled)
                return;
            e.Handled = true;

            switch (e.KeyCode)
            {
                case Keys.Enter:
                    this.EnterFunc(this.Text);
                    //this.Text = "";
                    //this.CursorPosition = 0;
                    break;
                default:
                    break;
            }

            switch (e.KeyValue)
            {
                case 37:
                    this.CursorPosition--;
                    this.RefreshCursor();
                    break;
                case 39:
                    this.CursorPosition++;
                    this.RefreshCursor();
                    break;

                default:
                    if ((Keys)e.KeyValue == Keys.Up ||
                        (Keys)e.KeyValue == Keys.Down)
                    {
                        int index = ((Keys)e.KeyValue == Keys.Up) ? 1 : -1;
                        this.HistoryNextPrev(index);
                        return;
                    }
                    break;
            }
        }

        void GetTextFromClipboard()
        {
            this.Text += Clipboard.ContainsText() ? Clipboard.GetText() : "";
        }

        /// <summary>
        /// temporary fix until i figure out what the fuck is wrong with changing background styles messing up size of the control
        /// </summary>
        public override BackgroundStyle BackgroundStyle
        {
            get => this._BackgroundStyle;
            set
            {
                this._BackgroundStyle = value;
                this.ClientLocation = new Vector2(this._BackgroundStyle.Border);
            }
        }

        public override void HandleKeyPress(KeyPressEventArgs e)
        {
            if (!this.Enabled)
                return;
            if (e.Handled)
                return;
            e.Handled = true;
            switch (e.KeyChar)
            {
                case '\b':
                    //this.Text = this.Text.Length > 0 ? this.Text.Remove(this.Text.Length - 1) : "";
                    if (this.CursorPosition == 0)
                        break;
                    this.Text = this.Text.Length > 0 ? this.Text.Remove(this.CursorPosition - 1, 1) : "";
                    this.CursorPosition--;
                    this.RefreshCursor();
                    this.OnTextEntered(e.KeyChar);
                    break;

                case (char)27: //escape
                    this.EscapeFunc(e);
                    break;

                //case '\r'://13) //enter
                //    EnterFunc(this.Text);
                //    this.Text = "";
                //    break;

                case (char)22: //paste
                    var t = new Thread(this.GetTextFromClipboard);
                    t.SetApartmentState(ApartmentState.STA);
                    t.Start();
                    break;

                default:
                    if (this.Text.Length == this.MaxLength)
                        break;
                    if (char.IsControl(e.KeyChar))
                        break;
                    if (!this.InputFilter(e.KeyChar))
                        break;
                    var cursor = this.CursorPosition;
                    this.Text = this.Text.Insert(cursor, e.KeyChar.ToString());
                    this.CursorPosition = cursor + 1;
                    this.RefreshCursor();
                    this.TextEnterFunc(e);
                    this.OnTextEntered(e.KeyChar);
                    break;
            }
        }

        private void RefreshCursor()
        {
            this.CursorVisible = true;
            this.CursorTimer = CursorTimerMax;
        }
        private void RefreshCursor(int position)
        {
            this.RefreshCursor();
            this.CursorPosition = position;
        }
        public override void HandleKeyUp(KeyEventArgs e)
        {
            if (!this.Enabled)
                return;
            if (e.Handled)
                return;
            e.Handled = true;
            switch (e.KeyValue)
            {
                case 27: //escape
                    this.Enabled = false;
                    break;
                default:
                    break;
            }
        }

        public static void DefaultTextHandling(TextBox txtBox, TextEventArgs e)
        {
            if (e.Char == 13) //enter
            {
                txtBox.Enabled = false;
            }
            else if (e.Char == 27) //escape
            {
                txtBox.Enabled = false;
            }
            else if (!char.IsControl(e.Char))
                txtBox.Text += e.Char;
        }

        internal static GroupBox CreateWithLabel(string name, string initialText, int width, out Label label, out TextBox textBox)
        {
            label = new Label($"{name}: ");
            textBox = new TextBox(initialText, width);
            return new GroupBox()
                .AddControlsHorizontally(
                    label,
                    textBox) as GroupBox;
        }

        public override void HandleLButtonDown(HandledMouseEventArgs e)
        {
            base.HandleLButtonDown(e);
            var mousex = UIManager.Mouse.X - this.ScreenLocation.X;
            int currentx = 0;
            for (int i = 0; i < this.Text.Length; i++)
            {
                var sub = this.Text.Substring(i, 1);
                var charWidth = (int)this.Font.MeasureString(sub).X;
                var charMiddle = currentx + charWidth / 2;
                if (currentx < mousex && mousex <= charMiddle)
                {
                    this.RefreshCursor(i);
                    return;
                }
                else if (charMiddle < mousex && mousex <= currentx + charWidth)
                {   this.RefreshCursor(i + 1);
                    return;
                }
                currentx += charWidth;
            }
            this.RefreshCursor(this.Text.Length);
        }
    }
}

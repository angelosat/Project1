using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Start_a_Town_.UI
{
    class SliderNew : Control
    {
        public static int DefaultHeight = UIManager.DefaultTrackBarThumbSprite.Height;
       
        readonly Panel PanelValue;
        readonly Label LabelValue;
        public float NextValue, PrevValue;
        readonly Func<float> ValueGetter;
        readonly Action<float> ValueSetter;
        public string Format;// = "##0%";
        readonly PictureBox Thumb;
        bool SelectingValue;
        readonly float Border = 5;// UIManager.DefaultTrackBarSprite.Width / 4;
        protected float _value;
        public float Value
        {
            get => this.ValueGetter?.Invoke() ?? this._value;
            set
            {
                this._value = value;
            }
        }

        public override int Height
        {
            get => DefaultHeight;
            set { }
        }

        public override string Name
        {
            get => base.Name;
            set => base.Name = value;
        }

        public float Min, Max, Step;
        public int TickFrequency { get; set; }

        public override void OnPaint(SpriteBatch sb)
        {
            this.DrawSprite(sb, Vector2.Zero);
        }

        public SliderNew(Func<float> valueGetter, Action<float> valueSetter, int width, float min = 0, float max = 1, float step = 0.1f, string format = null)
            : base()
        {
            this.BackgroundTexture = UIManager.DefaultTrackBarSprite;
            this.Width = width;
            this.Alpha = Color.Lerp(Color.White, Color.Transparent, 0.5f);
            this.Format = format;
            this.Min = min;
            this.Max = max;
            this.Step = step;
            this.ValueGetter = valueGetter;
            this.ValueSetter = valueSetter;
            this.NextValue = valueGetter();

            this.Thumb = new PictureBox(UIManager.DefaultTrackBarThumbSprite, null)
            {
                MouseThrough = true,
                LocationFunc = () => new Vector2(this.Border + (this.Width - 2 * this.Border) * (this.Value - this.Min) / (this.Max - this.Min), 0),
                Anchor = new(.5f, 0),
                ClipToBounds = false
            };

            this.PanelValue = new Panel() { Location = this.TopRight };
            this.PanelValue.ClientDimensions = UIManager.Font.MeasureString(this.Min.ToString());
            this.LabelValue = new Label();
            this.PanelValue.AddControls(this.LabelValue);
            this.PanelValue.SetMousethrough(true, true);

            this.Controls.Add(this.Thumb);
        }

        void DrawSprite(SpriteBatch sb, Vector2 position)
        {
            sb.Draw(UIManager.DefaultTrackBarSprite, new Rectangle((int)position.X, (int)position.Y, this.Height / 2, this.Height), new Rectangle(0, 0, UIManager.DefaultTrackBarSprite.Height / 2, UIManager.DefaultTrackBarSprite.Height), Color.White);
            sb.Draw(UIManager.DefaultTrackBarSprite, new Rectangle((int)position.X + this.Height / 2, (int)position.Y, this.Width - this.Height, this.Height), new Rectangle(UIManager.DefaultTrackBarSprite.Height / 2, 0, 1, UIManager.DefaultTrackBarSprite.Height), Color.White);
            sb.Draw(UIManager.DefaultTrackBarSprite, new Rectangle((int)position.X + this.Width - this.Height / 2, (int)position.Y, this.Height / 2, this.Height), new Rectangle(UIManager.DefaultTrackBarSprite.Height / 2 + 1, 0, UIManager.DefaultTrackBarSprite.Height / 2, UIManager.DefaultTrackBarSprite.Height), Color.White);
        }

        protected override void OnMouseLeftPress(System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.SelectingValue = true;
            this.PrevValue = this.NextValue;
            UpdateNextValue();
            base.OnMouseLeftPress(e);
        }
        public override void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (!this.SelectingValue)
                return;

            this.SelectingValue = false;

            if (this.NextValue != this.Value)
                this.ValueSetter(this.NextValue);
        }
        public override void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (!this.SelectingValue)
                return;
            this.SelectingValue = false;
            this.NextValue = this.PrevValue;
            e.Handled = true;
        }
        public override void HandleMouseWheel(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (e.Handled)
                return;
            if (!this.HasMouseHover)
                return;
            e.Handled = true;
            this.ValueSetter(this.Value + this.Step * e.Delta);
        }
        public override void Update()
        {
            base.Update();
            if (!this.SelectingValue)
                return;
            this.LabelValue.Text = this.NextValue.ToString();
            UpdateNextValue();
        }

        private void UpdateNextValue()
        {
            float mouseX = Controller.Instance.msCurrent.X / UIManager.Scale - (this.ScreenLocation.X + this.Border);
            float mousePerc = MathHelper.Clamp(mouseX / (float)(this.Width - 2 * this.Border), 0, 1);
            var newValue = (this.Min + mousePerc * (this.Max - this.Min)) / this.Step;
            this.NextValue = (float)Math.Round(newValue) * this.Step;
        }

        protected override void OnGotFocus()
        {
            this.Alpha = Color.White;
            base.OnGotFocus();
        }
        public override void OnLostFocus()
        {
            this.Alpha = Color.Lerp(Color.Transparent, Color.White, 0.5f);
            base.OnLostFocus();
        }

        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
            if (!this.SelectingValue)
                return;
            var loc = new Vector2((int)(this.Border + (this.Width - 2 * this.Border) * (this.NextValue - this.Min) / (this.Max - this.Min) - UIManager.DefaultTrackBarThumbSprite.Width * .5f), 0);

            sb.Draw(UIManager.DefaultTrackBarThumbSprite,
                this.ScreenLocation + loc,
                Color.White * .5f);
            UIManager.DrawStringOutlined(sb, this.NextValue.ToString(this.Format), this.ScreenLocation + new Vector2(this.Width, 0));//, Vector2.One);
        }

        public static Control CreateWithLabel(string label, Func<float> valueGetter, Action<float> valueSetter, int width, float min = 0, float max = 1, float step = 0.1f, string format = null)
        {
            return new GroupBox()
                .AddControlsVertically(
                new Label(() => $"{label}: {valueGetter().ToString(format)}"),
                new SliderNew(valueGetter, valueSetter, width, min, max, step, format));
        }

        public static Control CreateWithLabelNew(string name, Func<float> valueGetter, Action<float> valueSetter, int width, float min = 0, float max = 1, float step = 0.1f, string format = null)
        {
            var slider = new SliderNew(valueGetter, valueSetter, width, min, max, step, format);
            //var label = Label.ParseNewNew(name + ": ", new Func<string>(() => slider.NextValue.ToString(format))).ToGroupBoxHorizontally();//
            var label = Label.ParseNewNew(name + ": ", new Func<string>(() => valueGetter().ToString(format))).ToGroupBoxHorizontally();//
            //var label = new Label(() => $"{name}: {slider.NextValue.ToString(format)}");
            return new GroupBox()
                .AddControlsHorizontally(
                    slider,
                    label);
        }
    }
}

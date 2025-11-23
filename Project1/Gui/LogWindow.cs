using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.UI
{
    class LogWindow : Window
    {
        readonly ConsoleBoxAsync Box_Text;
        readonly Panel Panel_Text, Panel_Input;
        readonly SliderNew Sldr_Opacity;
        readonly Dictionary<Control, float> Timers;
        public TextBox TextBox;
        public static float FadeDelay = 6 * Ticks.PerSecond;

        static LogWindow _Instance;
        public static LogWindow Instance => _Instance ??= new LogWindow();

        public LogWindow()
        {
            this.Title = "Log";
            this.AutoSize = true;
            this.Closable = false;
            this.MouseThrough = true;

            this.Box_Text = Net.Client.Instance.ConsoleBox;
            this.Box_Text.FadeText = true;
            this.Panel_Text = new Panel() { AutoSize = true, Name = "Panel_Text", Color = Color.Black };
            this.Panel_Text.Controls.Add(this.Box_Text);

            this.Panel_Input = new Panel() { Location = this.Panel_Text.BottomLeft, Color = Color.Black };
            this.Panel_Input.ClientSize = new Rectangle(0, 0, this.Panel_Text.ClientSize.Width, Label.DefaultHeight);
            this.Panel_Input.BackgroundStyle = BackgroundStyle.TickBox;

            this.TextBox = new TextBox()
            {
                Width = this.Panel_Input.ClientSize.Width,
                EnterFunc = (a) =>
                {
                    this.Hide();
                    this.TextBox.Enabled = false;
                    string gotText = this.TextBox.Text;
                    this.TextBox.Text = "";

                    this.Panel_Input.Opacity = 0;
                    if (gotText.Length == 0)
                        return;

                    if (gotText.Length > 0)
                        PacketChat.Send(Net.Client.Instance, Net.Client.Instance.PlayerData.ID, gotText);
                },
                EscapeFunc = (a) =>
                {
                    this.TextBox.Enabled = false;
                    this.TextBox.Text = "";
                    this.Controls.Remove(this.Panel_Input);
                },
                TextEnterFunc = (a) =>
                {
                    if (!char.IsControl(a.KeyChar))
                        this.TextBox.Text += a.KeyChar;
                }
            };

            this.Timers = new Dictionary<Control, float>();

            this.Sldr_Opacity = new SliderNew(() => this.Box_Text.Opacity * 100, v => this.Box_Text.Opacity = v / 100f, width: 100, min: 0, max: 100, step: 1) { Name = "Opacity", Location = this.Label_Title.TopRight };
            //Sldr_Opacity.ValueChanged += new EventHandler<EventArgs>(Sldr_Opacity_ValueChanged);

            this.Panel_Input.Controls.Add(this.TextBox);
            this.Client.Controls.Add(this.Panel_Text, this.Panel_Input);
            this.Controls.Add(this.Client, this.Sldr_Opacity);
            this.Location = new Vector2(0, UIManager.Height);
            this.Anchor = new Vector2(0, 1);

            this.SetOpacity(0, true, exclude: this.Box_Text);
            this.TextBox.Enabled = false;
            this.SetMousethrough(true, true);
        }

        //void Sldr_Opacity_ValueChanged(object sender, EventArgs e)
        //{
        //    Box_Text.Opacity = Sldr_Opacity.Value / 100f;
        //}

        public override void Update()
        {
            base.Update();
            if (this.TextBox.Enabled)
                return;

            Dictionary<Control, float> copy = new Dictionary<Control, float>(this.Timers);
            foreach (KeyValuePair<Control, float> timer in copy)
            {
                float newValue = timer.Value - 1;
                float alpha = 10 * (float)Math.Sin(Math.PI * (newValue / FadeDelay));

                this.Timers[timer.Key] = newValue;
                timer.Key.Opacity = alpha;

                if (timer.Value <= 0)
                {
                    this.Timers.Remove(timer.Key);
                    timer.Key.Opacity = 0;
                }
            }
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            this.Hide();
        }

        public static void Write(LogEventArgs e)
        {
            string text = e.Entry.ToString();
            if (text.Length == 0)
                return;

            Color c = Color.White;
            switch (e.Entry.Type)
            {
                case Log.EntryTypes.System:
                    c = Color.Yellow;
                    break;
                case Log.EntryTypes.Damage:
                    c = Color.Red;
                    break;
                default:
                    break;
            }
            string fullText = DateTime.Now.ToString("[HH:mm:ss]") + text;
            fullText = StringHelper.Wrap(fullText, Instance.Box_Text.Client.Width);
            var line = new Label(fullText)
            {

                TextColor = c,
                Location = Instance.Box_Text.Client.Controls.Count > 0 ? Instance.Box_Text.Client.Controls.Last().BottomLeft : Vector2.Zero,
                MouseThrough = Instance.MouseThrough
            };
            Instance.Timers.Add(line, FadeDelay);
            Instance.Box_Text.AddControls(line);
            Instance.Box_Text.Client.ClientLocation.Y = Instance.Box_Text.Client.Bottom - Instance.Box_Text.Client.ClientSize.Height;
        }

        public override bool Hide()
        {
            this.SetOpacity(0, true, exclude: this.Box_Text);
            this.TextBox.Enabled = false;
            this.SetMousethrough(true, true);
            return true;
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.UI
{
    public class UIChat : Window
    {
        public static UIChat Instance;
        static UIChat()
        {
            Instance = new UIChat();
        }
        readonly GroupBox BoxButtons;
        public ConsoleBoxAsync Console;
        readonly Panel Panel_Text, Panel_Input;
        readonly SliderNew Sldr_Opacity;
        readonly Stack<string> History = new(16);
        int HistoryIndex;
        public TextBox TextBox;
        readonly IconButton BtnSettings;
        readonly UIChatSettings UISettings;
        UIChat()
        {
            this.Title = "Log";
            this.AutoSize = true;
            this.Closable = false;
            this.MouseThrough = true;
            this.Movable = true;

            this.Console = Net.Client.Instance.ConsoleBox;
            this.Console.FadeText = true;
            this.Panel_Text = new Panel() { AutoSize = true, Name = "Panel_Text", Color = Color.Black };
            this.Panel_Text.Controls.Add(this.Console);

            this.Panel_Input = new Panel
            {
                AutoSize = true,
                Location = this.Panel_Text.BottomLeft,
                Color = Color.Black,
                ClientSize = new Rectangle(0, 0, this.Panel_Text.ClientSize.Width, Label.DefaultHeight)
            };
            this.TextBox = new TextBox
            {
                Width = this.Panel_Input.ClientSize.Width,
                EnterFunc = (a) =>
                {
                    this.Hide();
                    this.TextBox.Enabled = false;
                    string gotText = this.TextBox.Text;
                    this.TextBox.Text = "";

                    if (this.History.Count == 15)
                        this.History.Pop();
                    this.History.Push(gotText);
                    this.HistoryIndex = -1;

                    this.Panel_Input.Opacity = 0;
                    if (gotText.Length == 0)
                        return;

                    if (gotText[0] == '/')
                    {
                        //server command
                        Net.Client.PlayerCommand(gotText.TrimStart('/'));
                    }
                    else
                    {
                        PacketChat.Send(Net.Client.Instance, Net.Client.Instance.PlayerData.ID, gotText);
                    }
                },
                EscapeFunc = a =>
                {
                    this.TextBox.Enabled = false;
                    this.TextBox.Text = "";
                    this.Controls.Remove(this.Panel_Input);
                },
                HistoryNextPrev = this.ChatHistory
            };

            this.Sldr_Opacity = new SliderNew(() => this.Console.Opacity * 100, v => this.Console.Opacity = v / 100f, width: 100, min: 0, max: 100, step: 1) { Name = "Opacity" };
            this.Sldr_Opacity.Location = this.Label_Title.TopRight;
            this.Panel_Input.Controls.Add(this.TextBox);
            this.BoxButtons = new GroupBox();
            this.BtnSettings = new IconButton(Icon.ArrowUp) { BackgroundTexture = IconButton.Small, Location = this.TopRight, Anchor = Vector2.UnitX, LeftClickAction = ToggleOptions };
            this.BoxButtons.AddControls(this.BtnSettings);

            this.Client.AddControlsVertically(this.BoxButtons, this.Panel_Text, this.Panel_Input);

            this.UISettings = new UIChatSettings(this);

            //this.Location = new Vector2(0, UIManager.Height);
            //this.Anchor = new Vector2(0, 1);

            this.AnchorToBottomLeft();
            this.SetOpacity(0, true, this.Client);
            this.TextBox.Enabled = false;
            this.SetMousethrough(true, true);
        }

        private void ToggleOptions()
        {
            this.UISettings.Location = this.BtnSettings.ScreenLocation;
            this.UISettings.Anchor = Vector2.UnitY;
            this.UISettings.Toggle();
        }

        void ChatHistory(int index)
        {
            if (this.History.Count == 0)
                return;
            // TODO: do something
            this.HistoryIndex += index;
            this.HistoryIndex = Math.Max(0, Math.Min(this.History.Count - 1, this.HistoryIndex));
            this.TextBox.Text = this.History.ElementAt(this.HistoryIndex);
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb, this.BoundsScreen);
        }

        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }

        public override void Update()
        {
            base.Update();
            if (this.TextBox.Enabled)
                return;
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            this.Hide();
        }

        public void Write(string text)
        {
            this.Write(Log.EntryTypes.Default, text);
        }
        public void Write(Log.EntryTypes type, string text)
        {
            this.Write(new Log.Entry(type, new object[] { text }));
        }
        public void Write(Log.Entry e)
        {
            string text = e.ToString();
            if (text.Length == 0)
                return;

            var line = new ConsoleEntry(e.Color, text);
            this.Console.Write(line);

            this.Console.Client.ClientLocation.Y = this.Console.Client.Bottom - this.Console.Client.ClientSize.Height;
        }

        internal void StartTyping()
        {
            this.Client.SetOpacity(1, true, exclude: this.Console);

            this.SetMousethrough(false, true);
            this.TextBox.Enabled = true;
        }

        internal void StartOrFinishTyping()
        {
            if (this.TextBox.Enabled)
            {
                this.TextBox.EnterFunc(this.TextBox.Text);
                return;
            }

            this.Client.SetOpacity(1, true, exclude: this.Console);

            this.SetMousethrough(false, true);
            this.TextBox.Enabled = true;
        }
        public override bool Hide()
        {
            this.Client.SetOpacity(0, true, exclude: this.Console);
            this.UISettings.Hide();
            this.TextBox.Enabled = false;
            this.SetMousethrough(true, true);
            return true;
        }
    }
}

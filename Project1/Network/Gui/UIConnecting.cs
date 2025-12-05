using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    sealed class UIConnecting : Panel
    {
        readonly Label LabelText;
        readonly string Address;
        readonly Button BtnBack;
        public static UIConnecting Create(string address)
        {
            var ui = new UIConnecting(address);
            ui.ShowDialog();
            return ui;
        }
        UIConnecting(string address)
        {
            this.Address = address;
            this.AutoSize = false;
            this.Size = new Rectangle(0, 0, 300, 100);
            this.LabelText = new Label($"Awaiting response from {address}") { AutoSize = true };
            this.LabelText.AnchorToParentCenter();
            this.AddControls(this.LabelText);
            this.BtnBack = new Button("Back") { Location = new Vector2(this.ClientDimensions.X / 2, this.ClientDimensions.Y), Anchor = new Vector2(.5f, 1), LeftClickAction = () => this.Hide() };
            this.AnchorToScreenCenter();
        }
        internal override void OnGameEvent(GameEvent e)
        {
            switch ((Components.Message.Types)e.Type)
            {
                case Components.Message.Types.ServerResponseReceived:
                    this.SetText("Connected!\nReceiving session...");
                    break;

                case Components.Message.Types.ChunksLoaded:
                    this.Hide();
                    break;

                case Components.Message.Types.ServerNoResponse:
                    this.SetText($"No response from {this.Address}");
                    this.AddControls(this.BtnBack);
                    break;

                default:
                    break;
            }
        }

        private void SetText(string text)
        {
            this.LabelText.Text = text;
            //this.LabelText.AnchorTo(this.ClientDimensions / 2, Vector2.One / 2);
        }
    }
}

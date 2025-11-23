using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Net;

namespace Start_a_Town_.UI
{
    class UIGameSpeed : Panel
    {
        //ButtonTogglable BtnPause, Btn1x, Btn2x, Btn3x;
        
        public UIGameSpeed(INetwork net)
        {
            this.AutoSize = true;
            //var w = Button.GetWidth(UIManager.Font, "►►►");

            //this.BtnPause = new ButtonTogglable("▪ ▪", w) { IsToggled = () => net.GetPlayer().SuggestedSpeed == 0, LeftClickAction = () => this.SetSpeed(net, 0), HoverFunc = () => GetAdditionalHoverText("Pause", 0) };
            //this.BtnPause.TexBackgroundColorFunc = () => (net.Speed == 0) ? Color.White : this.BtnPause.DefaultBackgroundColorFunc();
            //this.BtnPause.Tag = 0;

            //this.Btn1x = new ButtonTogglable("►", w) { IsToggled = () => net.GetPlayer().SuggestedSpeed == 1, LeftClickAction = () => this.SetSpeed(net, 1), HoverFunc = () => GetAdditionalHoverText("Normal", 1) };
            //this.Btn1x.TexBackgroundColorFunc = () => (net.Speed == 1) ? Color.White : this.Btn1x.DefaultBackgroundColorFunc();
            //this.Btn1x.Tag = 1;

            //this.Btn2x = new ButtonTogglable("►►", w) { IsToggled = () => net.GetPlayer().SuggestedSpeed == 2, LeftClickAction = () => this.SetSpeed(net, 2), HoverFunc = () => GetAdditionalHoverText("Fast", 2) };
            //this.Btn2x.TexBackgroundColorFunc = () => (net.Speed == 2) ? Color.White : this.Btn2x.DefaultBackgroundColorFunc();
            //this.Btn2x.Tag = 2;

            //this.Btn3x = new ButtonTogglable("►►►", w) { IsToggled = () => net.GetPlayer().SuggestedSpeed == 3, LeftClickAction = () => this.SetSpeed(net, 3), HoverFunc = () => GetAdditionalHoverText("Fastest", 3) };
            //this.Btn3x.TexBackgroundColorFunc = () => (net.Speed == 3) ? Color.White : this.Btn3x.DefaultBackgroundColorFunc();
            //this.Btn3x.Tag = 3;

            //this.AddControlsHorizontally(1, this.BtnPause, this.Btn1x, this.Btn2x, this.Btn3x);


            var btn0 = ButtonNew.CreateMedium("▪", () => this.SetSpeed(net, 0));
            btn0.IsToggledFunc = () => net.GetPlayer().SuggestedSpeed == 0;
            btn0.HoverFunc = () => GetAdditionalHoverText("Pause", 0);
            btn0.Tag = 0;

            var btn1 = ButtonNew.CreateMedium(">", () => this.SetSpeed(net, 1));
            btn1.IsToggledFunc = () => net.GetPlayer().SuggestedSpeed == 1;
            btn1.HoverFunc = () => GetAdditionalHoverText("Normal", 1);
            btn1.Tag = 0;

            var btn2 = ButtonNew.CreateMedium(">>", () => this.SetSpeed(net, 2));
            btn2.IsToggledFunc = () => net.GetPlayer().SuggestedSpeed == 2;
            btn2.HoverFunc = () => GetAdditionalHoverText("Fast", 2);
            btn2.Tag = 0;

            var btn3 = ButtonNew.CreateMedium(">>>", () => this.SetSpeed(net, 3));
            btn3.IsToggledFunc = () => net.GetPlayer().SuggestedSpeed == 3;
            btn3.HoverFunc = () => GetAdditionalHoverText("Fastest", 3);
            btn3.Tag = 0;

            this.AddControlsHorizontally(1, btn0, btn1, btn2, btn3);
        }
        //public override Vector2 ScreenLocation => base.ScreenLocation; // hm?

        private int GameSpeedPlayerCount(int speed)
        {
            return Client.Instance.GetPlayers().Where(p => p.SuggestedSpeed == speed).Count();
        }
        string GetAdditionalHoverText(string initialText, int speed)
        {
            var players = Client.Instance.GetPlayers().Where(p => p.SuggestedSpeed == speed);
            var count = players.Count();
            var text = $"{initialText}\n\n{count} player(s) at {initialText}:\n";
            foreach (var pl in players)
                text += pl.Name + '\n';
            return text.TrimEnd('\n');
        }
        void SetSpeed(INetwork net, int s)
        {
            PacketPlayerSetSpeed.Send(net, net.GetPlayer().ID, s);
        }
        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
            var players = Client.Instance.GetPlayers().ToLookup(p => p.SuggestedSpeed);
            foreach(var btn in this.Controls)
            {
                var btnSpeed = (int)btn.Tag;
                if(btnSpeed != Client.Instance.Speed && players[btnSpeed].Any())
                    btn.BoundsScreen.DrawFlashingBorder(sb);
            }
        }
    }
}

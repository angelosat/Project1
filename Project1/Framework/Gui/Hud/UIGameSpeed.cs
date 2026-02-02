using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Framework.Net;
using Project1.Framework.StaticMaps.Packets;
using System.Linq;

namespace Start_a_Town_.UI
{
    class UIGameSpeed : Panel
    {
        public UIGameSpeed(NetEndpoint net)
        {
            this.AutoSize = true;

            var btn0 = ButtonNew.CreateMedium("▪", () => SetSpeed(net, 0));
            btn0.IsToggledFunc = () => net.GetPlayer().SuggestedSpeed == 0;
            btn0.HoverFunc = () => GetAdditionalHoverText("Pause", 0);
            btn0.Tag = 0;

            var btn1 = ButtonNew.CreateMedium(">", () => SetSpeed(net, 1));
            btn1.IsToggledFunc = () => net.GetPlayer().SuggestedSpeed == 1;
            btn1.HoverFunc = () => GetAdditionalHoverText("Normal", 1);
            btn1.Tag = 0;

            var btn2 = ButtonNew.CreateMedium(">>", () => SetSpeed(net, 2));
            btn2.IsToggledFunc = () => net.GetPlayer().SuggestedSpeed == 2;
            btn2.HoverFunc = () => GetAdditionalHoverText("Fast", 2);
            btn2.Tag = 0;

            var btn3 = ButtonNew.CreateMedium(">>>", () => SetSpeed(net, 3));
            btn3.IsToggledFunc = () => net.GetPlayer().SuggestedSpeed == 3;
            btn3.HoverFunc = () => GetAdditionalHoverText("Fastest", 3);
            btn3.Tag = 0;

            this.AddControlsHorizontally(1, btn0, btn1, btn2, btn3);
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
        static void SetSpeed(NetEndpoint net, int s)
        {
            Ingame.Instance.Events.Post(new PlayerChangedSpeedEvent(s));
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

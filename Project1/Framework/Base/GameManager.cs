using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Net;
using Project1.Core.Net.Packets;
using Project1.Core.UI;
using Project1.Core.UI.Hud;
using Project1.Framework.UI;

namespace Project1.Core.Base
{
    class GameManager : GameSystem
    {
        public override void Initialize()
        {
            PacketPlayerDisconnected.Init();
        }

        public override void InitHUD(NetEndpoint net, Hud hud)
        {
            net.Events.ListenTo<ActorNeedUpdatedEvent>(HandleActorNeedUpdated);
        }
        void HandleActorNeedUpdated(ActorNeedUpdatedEvent e)
        {
            var actor = e.Need.Owner;
            var value = e.Need.Value;
            FloatingText.Create(actor, string.Format("{0:+;-}{1}", value, e.Need.Name),
                    ft =>
                    {
                        ft.Font = UIManager.FontBold;
                        ft.ColorFunc = () => value < 0 ? Color.Red : Color.Lime;
                    });
        }
    }
}

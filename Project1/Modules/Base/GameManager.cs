using Microsoft.Xna.Framework;
using Project1.Framework.Net;
using Project1.Framework.Net.Packets;
using Start_a_Town_.UI;

namespace Start_a_Town_.Modules.Base
{
    class GameManager : GameSystem
    {
        public override void Initialize()
        {
            //PacketPlayerConnecting.Init();
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

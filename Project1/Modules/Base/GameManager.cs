using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.UI;

namespace Start_a_Town_.Modules.Base
{
    class GameManager : GameComponent
    {
        public override void Initialize()
        {
            PacketPlayerConnecting.Init();
            PacketPlayerDisconnected.Init();
        }

        public override void InitHUD(NetEndpoint net, Hud hud)
        {
            net.Events.ListenTo<ActorNeedUpdatedEvent>(HandleActorNeedUpdated);
        }
        void HandleActorNeedUpdated(ActorNeedUpdatedEvent e)
        {
            FloatingText.Create(e.Actor, string.Format("{0:+;-}{1}", e.Value, e.Need.Name),
                    ft =>
                    {
                        ft.Font = UIManager.FontBold;
                        ft.ColorFunc = () => e.Value < 0 ? Color.Red : Color.Lime;
                    });
        }
    }
}

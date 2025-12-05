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
            Skill.Init(hud);
            net.Events.ListenTo<ActorNeedUpdatedEvent>(HandleActorNeedUpdated);
            //hud.RegisterEventHandler(Components.Message.Types.NeedUpdated, e =>
            //{
            //    var actor = e.Parameters[0] as Actor;
            //    var need = e.Parameters[1] as Need;
            //    var value = (float)e.Parameters[2];
            //    FloatingText.Create(actor, string.Format("{0:+;-}{1}", value, need.NeedDef.Name),
            //         ft =>
            //         {
            //             ft.Font = UIManager.FontBold;
            //             ft.ColorFunc = () => value < 0 ? Color.Red : Color.Lime;
            //         }
            //    );
            //});
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

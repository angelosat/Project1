using Microsoft.Xna.Framework;
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

        public override void InitHUD(Hud hud)
        {
            Skill.Init(hud);

            hud.RegisterEventHandler(Components.Message.Types.NeedUpdated, e =>
            {
                var actor = e.Parameters[0] as Actor;
                var need = e.Parameters[1] as Need;
                var value = (float)e.Parameters[2];
                FloatingText.Create(actor, string.Format("{0:+;-}{1}", value, need.NeedDef.Name),
                     ft =>
                     {
                         ft.Font = UIManager.FontBold;
                         ft.ColorFunc = () => value < 0 ? Color.Red : Color.Lime;
                     }
                );
            });
        }
    }
}

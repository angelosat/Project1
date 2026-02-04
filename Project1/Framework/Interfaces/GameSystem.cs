using Start_a_Town_.UI;
using Project1.Framework.Net;
using Start_a_Town_;
using Project1.Framework.UI;
using Project1.Framework.Base;

namespace Project1.Framework.Interfaces
{
    public class GameSystem
    {
        public virtual void Initialize() { }
        public virtual void InitHUD(NetEndpoint net, Hud hud) { }
        public virtual void OnGameEvent(GameEvent e) { }
        public virtual void OnHudCreated(Hud hud) { }
        public virtual void OnContextMenuCreated(IContextable obj, ContextArgs a) { }
        public virtual void OnTargetInterfaceCreated(TargetArgs t, Control ui) { }
        public virtual void OnContextActionBarCreated(ContextActionBar.ContextActionBarArgs a) { }
        public virtual void OnTooltipCreated(ITooltippable item, Tooltip t) { }
    }
}

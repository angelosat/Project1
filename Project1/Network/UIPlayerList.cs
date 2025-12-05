using System.Collections.Generic;
using Start_a_Town_.UI;

namespace Start_a_Town_.Net
{
    class UIPlayerList : GroupBox
    {
        ListBoxNoScroll<PlayerData, Button> List_Players;
        static readonly int DefaultWidth = 150;
        public UIPlayerList(IEnumerable<PlayerData> pList)
            //: base(150, 300)
        {
            this.AutoSize = true;
            List_Players = new ListBoxNoScroll<PlayerData, Button>(foo =>
            {
                var ctrl = new Button(foo.Name, DefaultWidth)
                {
                    TextColorFunc = () => foo.Color
                };
                ctrl.OnUpdate = () => ctrl.Text = foo.ID + ": " + foo.Name + " " + foo.Ping.ToString("###0ms");
                return ctrl;
            });
            this.List_Players.MouseThrough = true;
            this.Refresh(pList);
            this.AddControls(List_Players);
        }

        public UIPlayerList Refresh(IEnumerable<PlayerData> pList)
        {
            List_Players.Clear();
            List_Players.AddItems(pList);
            return this;
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch((Components.Message.Types)e.Type)
            {
                case Components.Message.Types.PlayerConnected:
                    this.List_Players.AddItems(e.Parameters[0] as PlayerData);
                    break;

                case Components.Message.Types.PlayerDisconnected:
                    this.List_Players.RemoveItems(e.Parameters[0] as PlayerData);
                    break;

                default:
                    break;
            }
        }
    }
}

using System.Collections.Generic;
using Project1.Core.Base;
using Project1.Core.UI;
using Project1.Core.UI;

namespace Project1.Core.Net
{
    class UIPlayerList : GroupBox
    {
        ListBoxNoScroll<PlayerData, Button> List_Players;
        static readonly int DefaultWidth = 150;
        class PlayerConnectedEvent(PlayerData player, bool connected) : IEventPayload
        {
            public readonly PlayerData Player = player;
            public readonly bool Connected = connected;
        }
        public UIPlayerList(NetEndpoint net)// IEnumerable<PlayerData> pList)
            //: base(150, 300)
        {
            var pList = net.GetPlayers();
            this.HideAction = net.Events.ListenTo<PlayerConnectedEvent>(HandlePlayerConnected);
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
        void HandlePlayerConnected(PlayerConnectedEvent e)
        {
            if(e.Connected)
                this.List_Players.AddItems(e.Player);
            else
                this.List_Players.RemoveItems(e.Player);
        }
    }
}

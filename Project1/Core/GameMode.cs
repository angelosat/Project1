using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Project1.Framework.UI;
using Project1.Framework.Events;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Core.UI.Hud;
using Project1.Core.UI;


namespace Project1.Core
{
    abstract class GameMode
    {
        static public GameMode Current;

        static List<GameMode> _Registry;
        static public List<GameMode> Registry
        {
            get
            {
                if (_Registry == null)
                {
                    _Registry =
                    [
                        StaticMaps
                    ];
                }
                return _Registry;
            }
        }

        public string Name;
        
        internal abstract void OnMainMenuCreated(MainMenuWindow mainmenu);

        public virtual void ParseCommand(NetEndpoint net, string command)
        {

        }

        protected List<GameSystem> GameComponents = new List<GameSystem>();
        public abstract GameScreen GetWorldSelectScreen(INetEndpoint net);
        public static readonly GameMode StaticMaps = new GameModeStaticMaps();

        public virtual void OnIngameMenuCreated(IngameMenu menu) { }
        public virtual void OnHudCreated(Hud hud)
        {
            foreach (var comp in this.GameComponents)
                comp.OnHudCreated(hud);
        }
        
        public abstract bool IsPlayerWithinRangeForPacket(PlayerData playerData, Vector3 packetEventGlobal);
      
        internal virtual void PlayerConnected(Server server, PlayerData player) { }
        internal virtual void PlayerIDAssigned(Client client) { }
        internal virtual void MapReceived(MapBase map) { }
        internal virtual void Update(Client client) { }
        internal virtual void Update(Server server) { }

        internal abstract Control LoadGame();
        internal virtual Control GetNewGameGui(Action cancelAction) { return null; }

        internal virtual void ChunkReceived(Server server, int playerid, Vector2 vec) { }
        internal virtual void HandleEvent(NetEndpoint world, GameEvent e) { }
        internal virtual void HandleEvent(NetEndpoint net, object e, object[] p) { }
        internal virtual void AllChunksReceived(NetEndpoint net) { }
    }
}

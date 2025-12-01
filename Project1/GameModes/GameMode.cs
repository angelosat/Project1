using System.Collections.Generic;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using Start_a_Town_.Core;
using Microsoft.Xna.Framework;
using System;

namespace Start_a_Town_
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
                    _Registry = new List<GameMode>()
                    {
                        StaticMaps
                    };
                }
                return _Registry;
            }
        }

        public string Name;
        
        internal abstract void OnMainMenuCreated(MainMenuWindow mainmenu);

        public virtual void ParseCommand(INetEndpoint net, string command)
        {

        }

        protected List<GameComponent> GameComponents = new List<GameComponent>();
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
        internal virtual void HandleEvent(INetEndpoint net, GameEvent e) { }
        internal virtual void HandleEvent(INetEndpoint net, object e, object[] p) { }
        internal virtual void AllChunksReceived(INetEndpoint net) { }
    }
}

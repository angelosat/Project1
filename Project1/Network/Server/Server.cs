using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Start_a_Town_.Net
{
    public partial class Server : NetEndpoint
    {
        public override bool IsServer => true;
        public override bool IsClient => false;
        public override double CurrentTick => ServerClock.TotalMilliseconds;

        readonly string _name = "Server";
        public override string ToString()
        {
            return this._name;
        }
        // TODO: figure out why it's glitching out if I set it lower than 10
        public const int ClockIntervalMS = 10;// 10 is working
        public override TimeSpan Clock => ServerClock;
        static TimeSpan ServerClock;

        static bool IsRunning;

        public bool IsSaving;

        UI.ConsoleBoxAsync _Console;
        public override UI.ConsoleBoxAsync ConsoleBox
        {
            get
            {
                this._Console ??= new UI.ConsoleBoxAsync(new Rectangle(0, 0, 800, 500)) { FadeText = false };
                return this._Console;
            }
        }

        public const int SnapshotIntervalMS = 10;// send 60 snapshots per second to clients
        public const int LightIntervalMS = 10;// send 60 light updates per second to clients

        /// <summary>
        /// Contains objects that have changed since the last world delta state update
        /// </summary>
        public HashSet<GameObject> ObjectsChangedSinceLastSnapshot = [];
        [Obsolete]

        protected override void Post(GameEvent e)
        {
            this.Events.Post(e.Payload);
            this.World.Events.Post(e.Payload);

            GameMode.Current.HandleEvent(this, e);
            foreach (var item in Game1.Instance.GameComponents)
                item.OnGameEvent(e);
            //Instance.Map.OnGameEvent(e);
        }
        //public override void EventOccured(Message.Types type, params object[] p)
        //{
        //    var e = new GameEvent(ServerClock.TotalMilliseconds, type, p);
        //    this.OnGameEvent(e);
        //}

        public static CancellationTokenSource ChunkLoaderToken = new();
        static Server _Instance;
        public static Server Instance => _Instance ??= new Server();

        //static int _refIdSequence = 1;

        static int _playerID = 1;
        public static int PlayerID => _playerID++;
        private void AdvanceClock()
        {
            ServerClock = ServerClock.Add(TimeSpan.FromMilliseconds(ClockIntervalMS));
        }

        public override MapBase Map { get; set; }
        public override WorldBase World { get; set; }
        public static int Port = 5541;
        static Socket Listener;
        public PlayerList Players;

        public override IEnumerable<PlayerData> GetPlayers()
        {
            return this.Players.GetList();
        }

        public static RandomThreaded Random;

        public static void Stop()
        {
            Server.ChunkLoaderToken.Cancel();
            Listener?.Close();

            Instance.ConsoleBox?.Write("SERVER", "Stopped");
            Instance.ResetOutgoingStreams();

            Instance.Players = new PlayerList(Instance);
            IsRunning = false;
            Instance.Map = null;
            Connections = new ConcurrentDictionary<EndPoint, UdpConnection>();
            ServerClock = new TimeSpan();
            Instance.Speed = 0;
        }
        public static void Start()
        {
            Instance.Speed = 0;
            IsRunning = true;
            Instance.ResetOutgoingStreams();

            Connections = new ConcurrentDictionary<EndPoint, UdpConnection>();
            //Instance.NetworkObjects.Clear();
            ServerClock = new TimeSpan();
            Instance.ConsoleBox.Write("SERVER", "Started");
            if (Listener != null)
                Listener.Close();
            Listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Listener.ReceiveBufferSize = Listener.SendBufferSize = Packet.Size;
            Instance.Players = new PlayerList(Instance);
            var anyIP = new IPEndPoint(IPAddress.Any, Port);
            Listener.Bind(anyIP);


            //// IGNORE CONNECTIONRESET EXCEPTIONS
            ////int SIO_UDP_CONNRESET = -1744830452;
            ////Listener.IOControl(
            ////    (IOControlCode)SIO_UDP_CONNRESET,
            ////    new byte[] { 0, 0, 0, 0 },
            ////    null
            ////);

            uint IOC_IN = 0x80000000;
            uint IOC_VENDOR = 0x18000000;
            uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
            Listener.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);

            EndPoint remote = new IPEndPoint(IPAddress.Any, Port);
            var state = new UdpConnection("player", remote) { Buffer = new byte[Packet.Size], IP = remote };
            Listener.BeginReceiveFrom(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ref remote, ar =>
            {
                ReceiveMessage(ar);
            }, state);
            Instance.ConsoleBox.Write(Color.Yellow, "SERVER", "Listening to port " + Port + "...");
        }
        int _Speed = 0;// 1;
        public override int Speed { get => this._Speed; set => this._Speed = value; }
        readonly float BlockUpdateTimerMax = 1;
        float BlockUpdateTimer = 0;

        public void Update(GameTime gt)
        {
            if (!IsRunning)
                return;
            HandleIncoming();
            HandleIncomingOrdered();
            if (GameMode.Current is not null)
                GameMode.Current.Update(Instance);
            if (Instance.Map is null)
                return;
            if (!Instance.IsSaving)
            {
                this.TickMap();
                this.Map.Update();
                SendSnapshots(ServerClock);
            }

            /// THESE MUST BE CALLED FROM WITHIN THE GAMESPEED LOOP
            this.WritePlayerSpecificNew();
            this.CreatePacketsFromStreams();
            this.ResetOutgoingStreams();
            this.AdvanceClock();
            this.SendPackets();
        }
        private void WritePlayerSpecificNew()
        {
            // SEND PLAYER SPECIFIC ACKS
            foreach (var player in this.Players.GetList())
            {
                PacketAcks.Send(player);
            }
        }
        private void CreatePacketsFromStreams()
        {
            foreach (var stream in this.StreamsArray)
            if (stream.Writer.BaseStream.Position > 0 
                    || stream.Reliability == ReliabilityType.Unreliable) // send empty "heartbeat" packets to advance client's clock
                {
                // append per-player data
                foreach (var player in this.GetPlayers())
                {
                    MemoryStream mem = stream.Reliability switch
                    {
                        ReliabilityType.Unreliable => (MemoryStream)player.StreamUnreliable.BaseStream,
                        ReliabilityType.OrderedReliable => (MemoryStream)player.StreamReliable.BaseStream,
                        _ => throw new Exception(),
                    };
                    //if (stream.Reliability != ReliabilityType.Unreliable && stream.Writer.BaseStream.Position == 0)
                    //    continue;
                    var data = stream.GetBytes(mem);
                    var p = Packet.Create(player, PacketType.MergedPackets, data, stream.Reliability);
                    p.Synced = true;
                    p.Tick = this.Clock.TotalMilliseconds;
                    this.Enqueue(player, p);
                }
            }
        }
        private void SendPackets()
        {
            this.SendUnreliable();
            this.TryResendPacketsFirst(); //try resend first or all?
            this.SendOrderedReliable();
        }

        protected void ResetOutgoingStreams()
        {
            foreach (var s in this.StreamsArray)
                s.Reset();
        }

        private void TickMap()
        {
            var auxStream = new BinaryWriter(new MemoryStream());
            for (int i = 0; i < this.Speed; i++)
            {
                /// i moved this from the start of the loop to the end of the loop because
                /// some packets might have been written already during packet handling before the map ticking
                /// (for example as a response to player input) and we don't want to clear them
                //this.OutgoingStreamTimestamped = new(new MemoryStream());

                auxStream.Write(this.Map.World.CurrentTick);
                auxStream.Write(this.CurrentTick);
                this.Map.World.Tick(Instance);
                this.Map.Tick();
                var length = this.OutgoingStreamTimestamped.BaseStream.Position;
                auxStream.Write(length);// write length
                if (length > 0)
                {
                    this.OutgoingStreamTimestamped.BaseStream.Position = 0;
                    this.OutgoingStreamTimestamped.BaseStream.CopyTo(auxStream.BaseStream);
                }

                this.BlockUpdateTimer--;
                if (this.BlockUpdateTimer <= 0)
                {
                    this.BlockUpdateTimer = Instance.BlockUpdateTimerMax;
                    this.SendRandomBlockUpdates();
                }

                /// i moved this from the start of the loop to the end of the loop because
                /// some packets might have been written already during packet handling before the map ticking
                /// (for example as a response to player input) and we don't want to clear them
                this.OutgoingStreamTimestamped = new(new MemoryStream());
            }
            if (auxStream.BaseStream.Position > 0)
            {
                auxStream.BaseStream.Position = 0;
                this.OutgoingStreamOrderedReliable.Write(Network.Packets.PacketTimestamped);
                auxStream.BaseStream.CopyTo(this.OutgoingStreamOrderedReliable.BaseStream);
            }
        }

        public static int RandomBlockUpdatesCount = 1;
        static int RandomBlockUpdateIndex = 0;
        void SendRandomBlockUpdates() // TODO: move this to map class. server object shouldn't contain map logic
        {
            var tosend = new IntVec3[Instance.Map.ActiveChunks.Count];
            var k = 0;
            foreach (var chunk in Instance.Map.ActiveChunks.Values)
            {
                var randcell = chunk.GetRandomCellInOrder(RandomBlockUpdateIndex);
                var global = randcell.ToGlobal(chunk);
                Instance.Map.RandomBlockUpdate(global);
                tosend[k++] = global;
            }
            RandomBlockUpdateIndex++;
            if (RandomBlockUpdateIndex == Chunk.Volume)
                RandomBlockUpdateIndex = 0;
            PacketRandomBlockUpdates.Send(this, tosend);
        }

        private static void HandleIncoming()
        {
            foreach (var player in from conn in Connections select conn.Value.Player)
                while (player.IncomingAll.TryDequeue(out Packet msg))
                {
                    if ((msg.Reliability & ReliabilityType.Reliable) == ReliabilityType.Reliable)
                        player.IncomingOrderedReliable.Enqueue(msg.OrderedReliableID, msg);
                    else
                        HandleMessage(msg);
                }
        }
        private static void HandleIncomingOrdered()
        {
            foreach (var player in from conn in Connections select conn.Value.Player)
                while (player.IncomingOrderedReliable.Count > 0)
                {
                    var packet = player.IncomingOrderedReliable.Peek();
                    long nextid = packet.OrderedReliableID;
                    if (nextid == player.RemoteOrderedReliableSequence + 1)
                    {
                        player.RemoteOrderedReliableSequence = nextid;
                        player.IncomingOrderedReliable.Dequeue();
                        HandleMessage(packet);
                    }
                    else
                        return;
                }
        }

        void SendUnreliable()
        {
            foreach (var player in this.Players.GetList())
                while (player.OutUnreliable.Any())
                {
                    if (!player.OutUnreliable.TryDequeue(out Packet p))
                        return;
                    p.BeginSendTo(Listener, player.IP);
                }
        }

        void SendOrderedReliable()
        {
            foreach (var player in this.Players.GetList())
                while (player.OutReliable.Any())
                {
                    if (!player.OutReliable.TryDequeue(out Packet packet))
                        return;
                    if (packet.PacketType == PacketType.RequestConnection)
                    {

                    }
                    packet.Player.WaitingForAck[packet.ID] = packet;
                    packet.BeginSendTo(Listener, player.IP);
                }
        }

        void TryResendPacketsFirst()
        {
            foreach (var player in this.Players.GetList())
            {
                if(NetworkHelper.TryResend(Listener, player.IP, player) is { } timedout)
                {
                    this.ConsoleBox.Write(UI.ConsoleMessageTypes.Acks, Color.Orange, "SERVER", "Send retries exceeded maximum for packet " + timedout);
                    this.ConsoleBox.Write(Color.Red, "SERVER", player.Name + " timed out");
                    CloseConnection(player.Connection);
                }
            }
        }

       
        internal void Enqueue(PacketType type, ReliabilityType reliability, byte[] data, bool sync = true)
        {
            if (!sync)
                throw new NotImplementedException(); // TODO NET: I NEVER PASS SYNC AS FALSE SO REMOVE IT AND ALL LOGIC SURROUNDING IT
            foreach (var player in this.Players.GetList())
            {
                var p = Packet.Create(player, type, data, reliability);
                p.Synced = sync;
                p.Tick = this.Clock.TotalMilliseconds;
                this.Enqueue(player, p);
            }
        }
        internal void Enqueue(PlayerData player, Packet packet)
        {
            if ((packet.Reliability & ReliabilityType.Reliable) == ReliabilityType.Reliable)
            {
                if (packet.Reliability == ReliabilityType.OrderedReliable)
                    packet.Tick = this.Clock.TotalMilliseconds;
                player.OutReliable.Enqueue(packet);
            }
            else
                player.OutUnreliable.Enqueue(packet);
        }
        public override void Enqueue(PacketType type, byte[] data, ReliabilityType send)
        {
            if (data.Length > 60000)
            {
                foreach (var player in this.Players.GetList())
                {
                    throw new NotImplementedException();
                }
                return;
            }
            foreach (var player in this.Players.GetList())
                this.Enqueue(player, Packet.Create(player, type, data, send));
        }
        internal void Enqueue(PacketType type, byte[] data, ReliabilityType send, Vector3 global)
        {
            this.Enqueue(type, data, send, player => player.IsWithin(global));
        }
        internal void Enqueue(PacketType type, byte[] data, ReliabilityType send, Vector3 global, bool sync)
        {
            foreach (var player in this.Players.GetList().Where(player => player.IsWithin(global)))
            {
                var p = Packet.Create(player, type, data, send);
                var t = this.Clock.TotalMilliseconds;
                p.Synced = sync;
                p.Tick = this.Clock.TotalMilliseconds;
                this.Enqueue(player, p);
            }
        }
        internal void Enqueue(PacketType type, byte[] data, ReliabilityType send, Func<PlayerData, bool> filter)
        {
            if (data.Length > 60000)
            {
                foreach (var player in this.Players.GetList().Where(filter))
                {
                    throw new NotImplementedException();
                }
                return;
            }
            foreach (var player in this.Players.GetList().Where(filter))
                this.Enqueue(player, Packet.Create(player, type, data, send));
        }

        public BinaryWriter OutgoingStreamUnreliable => this.GetStream(ReliabilityType.Unreliable).Writer;// this[ReliabilityType.Unreliable];
        public BinaryWriter OutgoingStreamOrderedReliable => this.GetStream(ReliabilityType.OrderedReliable).Writer;// this[ReliabilityType.OrderedReliable];
        public BinaryWriter OutgoingStreamReliable => this.GetStream(ReliabilityType.Reliable).Writer;// this[ReliabilityType.Reliable];


        public BinaryWriter OutgoingStreamTimestamped = new(new MemoryStream());

        public override BinaryWriter GetOutgoingStreamOrderedReliable()
        {
            return this.OutgoingStreamOrderedReliable;
        }
        
        static ConcurrentDictionary<EndPoint, UdpConnection> Connections = new();
        static void ReceiveMessage(IAsyncResult ar)
        {
            UdpConnection state = (UdpConnection)ar.AsyncState;
            EndPoint remoteIP = state.IP;
            EndPoint anyIP = new IPEndPoint(IPAddress.Any, Port);
            try
            {
                Packet packet = Packet.Read(state.Buffer);
                int bytesReceived = Listener.EndReceiveFrom(ar, ref remoteIP);

                state.Buffer = new byte[Packet.Size];
                Listener.BeginReceiveFrom(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ref anyIP, ReceiveMessage, state);

                if (!Connections.TryGetValue(remoteIP, out state))
                {
                    // only register new connection if it's a requestplayerid packet type
                    if (packet.PacketType != PacketType.RequestConnection)
                    {
                        //throw new Exception("Invalid sender");
                        return;
                    }
                    Server.Instance.ConsoleBox.Write(Color.Yellow, "SERVER", remoteIP + " connecting...");
                    UdpConnection newConnection = CreateConnection(remoteIP);
                    state = newConnection;
                }

                packet.Connection = state;
                var player = state.Player;
                packet.Player = player;

                player.IncomingAll.Enqueue(packet);

                if ((packet.Reliability & ReliabilityType.Reliable) == ReliabilityType.Reliable)
                    player.AckQueue.Enqueue(packet.ID);
            }
            catch (SocketException)
            {
                // this is thrown (at least) twice because the client sends 2 packets per frame (reliable+unreliable) + any resent reliable packets
                CloseConnection(state);
            }
            catch (ObjectDisposedException) { }
        }

        private static UdpConnection CreateConnection(EndPoint remoteIP)
        {
            var newPlayer = new PlayerData(remoteIP);
            var newConnection = new UdpConnection(newPlayer.IP.ToString(), remoteIP)
            {
                Player = newPlayer
            };
            newPlayer.Connection = newConnection;
            Connections.TryAdd(newConnection.IP, newConnection);

            newConnection.Ping = new System.Diagnostics.Stopwatch();

            return newConnection;
        }

        private static void HandleMessage(Packet msg)
        {
            var r = msg.Reader;
            switch (msg.PacketType)
            {
                case PacketType.RequestConnection:

                    string name = msg.Reader.ReadString();

                    Instance.ConsoleBox.Write(Color.Lime, "SERVER", name + " connected from " + msg.Connection.IP);

                    PlayerData pl = msg.Connection.Player;
                    pl.Name = name;
                    pl.ID = PlayerID;
                    pl.SuggestedSpeed = Instance.Speed;
                    msg.Player = pl;

                    // send packet back to the player
                    Instance.Enqueue(pl, Packet.Create(msg.Player, msg.PacketType, Network.Serialize(w =>
                    {
                        w.Write(msg.Player.ID);
                        Instance.Players.Write(w);
                        w.Write(Instance.Speed);
                    }), ReliabilityType.Reliable | ReliabilityType.Ordered));

                    var state = new UdpConnection(pl.Name + " listener", pl.IP) { Buffer = new byte[Packet.Size], Player = pl };
                    EndPoint ip = state.IP;
                    Listener.BeginReceiveFrom(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ref ip, ReceiveMessage, state);
                    PacketPlayerConnecting.Send(Instance, msg.Player);

                    // add to players when entering world instead?
                    Instance.Players.Add(pl);
                    GameMode.Current.PlayerConnected(Instance, msg.Player);
                    return;

                case PacketType.PlayerDisconnected:
                    CloseConnection(msg.Connection);
                    break;

                case PacketType.PlayerServerCommand:
                    CommandParser.Execute(Instance, msg.Player, r.ReadASCII());
                    break;

                case PacketType.MergedPackets:
                    UnmergePackets(msg);
                    break;

                default:
                    break;
            }
        }

        public void SpawnRequestFromTemplate(int templateID, TargetArgs target)
        {
            var entity = GameObject.Templates[templateID].Clone() as Entity;

            entity.Randomize(Random);
            target.Map = Instance.Map;
            switch (target.Type)
            {
                case TargetType.Slot:
                    Instance.Instantiate(entity);
                    target.Slot.Object = entity;
                    Instance.SyncChild(entity, target.Slot.Parent, target.Slot.ID);
                    break;

                case TargetType.Position:
                    entity.Global = target.Global;
                    this.Instantiate(entity);
                    PacketEntityInstantiate.SendFromTemplate(this, templateID, entity);
                    this.Map.SyncSpawnUntimestamped(entity);
                    break;

                default:
                    break;
            }
        }

        void SyncChild(GameObject obj, GameObject parent, int childIndex)
        {
            byte[] data = Network.Serialize(w =>
            {
                obj.Write(w);
                w.Write(parent.RefId);
                w.Write(childIndex);
            });
            foreach (var player in this.Players.GetList())
                this.Enqueue(player, Packet.Create(player, PacketType.SpawnChildObject, data, ReliabilityType.Ordered | ReliabilityType.Reliable));
        }

        private static void KickPlayer(int plid)
        {
            CloseConnection(Instance.Players.GetList().First(p => p.ID == plid).Connection);
        }

        static void CloseConnection(UdpConnection connection)
        {
            if (!Connections.TryRemove(connection.IP, out UdpConnection existing))
            {
                ("Tried to close nonexistent connection").ToConsole();
                return;
            }
            existing.Ping.Stop();
            "connection closed".ToConsole();
            Instance.Players.Remove(existing.Player);
            if (existing.Player.IsActive)
                existing.Player.ControllingEntity.OnDespawn();
            Instance.DisposeObject(existing.Player.CharacterID);
            PacketPlayerDisconnected.Send(Instance, existing.Player.ID);
        }

        [Obsolete("use world.register instead")]
        public override GameObject Instantiate(GameObject obj)
        {
            foreach (var o in obj.GetSelfAndChildren())
                this.Instantiator(o);
            return obj;
        }
        [Obsolete("use world.register instead")]
        public override void Instantiator(GameObject obj)
        {
            this.World.RegisterOld(obj as Entity);
            obj.Net = this;
        }

        /// <summary>
        /// Releases the object's networkID.
        /// </summary>
        /// <param name="objNetID"></param>
        public override bool DisposeObject(GameObject obj)
        {
            return this.DisposeObject(obj.RefId);
        }
        /// <summary>
        /// Releases the object's networkID.
        /// </summary>
        /// <param name="objNetID"></param>
        public override bool DisposeObject(int netID)
        {
            return this.World.DisposeEntity(netID);
        }
        [Obsolete("use world.disposeandsync")]
        public void SyncDispose(int refID)
        {
            this.DisposeObject(refID);
            PacketEntityDispose.Send(this, refID);
        }
        public void SetMap(MapBase map)
        {
            this.World = map.World;
            this.World.Net = Instance;
            Instance.Map = map;
            foreach (var ch in map.GetActiveChunks().Values)
                InstantiateChunk(ch);
            foreach (var obj in Instance.World.Entities)
                obj.Value.OnMapLoaded(Instance.Map);
            map.ResolveReferences();
            Random = new RandomThreaded(Instance.Map.Random);
        }
        private static void InstantiateChunk(Chunk chunk)
        {
            chunk.GetObjects().ForEach(obj =>
            {
                Instance.Instantiate(obj);
                //_refIdSequence = Math.Max(_refIdSequence, obj.RefId + 1);
            });

            foreach (var (local, entity) in chunk.GetBlockEntitiesByPosition())
            {
                var global = local.ToGlobal(chunk);
                entity.ResolveReferences(Instance.Map, global);
                foreach (var o in entity.GetChildren())
                    Instance.Instantiate(o);
            }
        }
        public override bool TryGetNetworkObject(int netID, out Entity obj)
        {
            return this.World.TryGetEntity(netID, out obj);
        }
        public override void PostLocalEvent(GameObject recipient, ObjectEventArgs args)
        {
            args.Network = Instance;
            recipient.PostMessage(args);
        }
        public override void PostLocalEvent(GameObject recipient, Components.Message.Types type, params object[] args)
        {
            ObjectEventArgs a = ObjectEventArgs.Create(type, args);
            a.Network = Instance;
            recipient.PostMessage(a);
        }
        private static void SendSnapshots(TimeSpan gt)
        {
            /// always send snapshots every frame, even empty ones. so that the client can interpolate correctly
            PacketSnapshots.Send(Instance, Instance.ObjectsChangedSinceLastSnapshot);
            Instance.ObjectsChangedSinceLastSnapshot.Clear();
        }

        public override bool LogStateChange(int netID)
        {
            return this.ObjectsChangedSinceLastSnapshot.Add(this.World.Entities[netID]);
        }

        #region Loot
        public override void PopLoot(LootTable table, Vector3 startPosition, Vector3 startVelocity)
        {
            //foreach (var obj in this.GenerateLoot(table))
            foreach (var obj in table.GenerateLoot(Random))
                this.PopLoot(obj, startPosition, startVelocity);
        }
        public RandomThreaded GetRandom() => Random;
        public override void PopLoot(GameObject obj, Vector3 startPosition, Vector3 startVelocity)
        {
            //double angle = Random.NextDouble() * (Math.PI + Math.PI);
            //double w = Math.PI / 4f;

            //float verticalForce = .3f;// 0.3f;
            //float horizontalForce = .1f;
            //float x = horizontalForce * (float)(Math.Sin(w) * Math.Cos(angle));
            //float y = horizontalForce * (float)(Math.Sin(w) * Math.Sin(angle));
            //float z = verticalForce * (float)Math.Cos(w);

            //var direction = new Vector3(x, y, z);
            //var final = startVelocity + direction;

            //obj.Global = startPosition;
            //obj.Velocity = final;
            obj.Global = startPosition;
            obj.Velocity = LootManager.RandomPopVelocity(Random);


            //if (obj.RefId == 0)
            //    obj.SyncInstantiate(this);
            //this.Map.SyncSpawn(obj, startPosition, final);
        }
        //public IEnumerable<GameObject> GenerateLoot(LootTable lootTable)
        //{
        //    foreach (var i in lootTable.Generate(Random))
        //        yield return i;
        //}
        #endregion

        private bool UnloadWorld()
        {
            if (Connections.Count > 0)
            {
                this.ConsoleBox.Write(Color.Red, "SERVER", "Can't unload world while active connections exist");
                return false;
            }
            if (this.World != null)
            {
                this.ConsoleBox.Write(Color.Lime, "SERVER", "World " + this.World.Name + " unloaded");
            }
            this.World = null;
            return true;
        }

        ServerCommandParser Parser;
        public static void Command(string command)
        {
            if (Instance.Parser == null)
                Instance.Parser = new ServerCommandParser(Instance);
            Instance.Parser.Command(command);
        }

        private static void UnmergePackets(Packet packet)
        {
            var player = packet.Player;
            var r = packet.PacketReader;
            while (r.Position < r.Length)
            {
                var typeID = r.ReadInt32();
                var lastPos = r.Position;

                Instance.HandlePacket(typeID, packet);
                if (r.Position == lastPos)
                    // if the stream position hasn't changed, and we're still not at the end, it means that there are no packet handlers registered to read the next set of data. break or throw?
                    //throw new Exception();
                    break; // i think that's the price of not sending the length as the header and just continuing to read until the packethandler is invalid, which implies we reached the end. but that doesnt sound very clean
            }
        }

        public static void StartSaving()
        {
            Instance.IsSaving = true;
            PacketSaving.Send(Instance);
        }
        public static void FinishSaving()
        {
            Instance.IsSaving = false;
            PacketSaving.Send(Instance);
        }
        public override PlayerData CurrentPlayer => null; //placeholder until a server session supports a player and not just be dedicated
        public override PlayerData GetPlayer()
        {
            return this.CurrentPlayer;
        }
        public override PlayerData GetPlayer(int id)
        {
            return this.Players.GetPlayer(id);
        }
        public override void SetSpeed(int playerID, int speed)
        {
            this.GetPlayer(playerID).SuggestedSpeed = speed;
            var newspeed = this.Players.GetLowestSpeed();
            this.Speed = newspeed;
        }

        public override void Write(string text)
        {
        }

        public override void Report(string text)
        {
            this.Write(text);
        }

        public override void SyncReport(string text)
        {
            this.Report(text);
            Network.SyncReport(this, text);
        }

        public override void WriteToStream(params object[] args)
        {
            this.GetOutgoingStreamOrderedReliable().Write(args);
        }

    }
}
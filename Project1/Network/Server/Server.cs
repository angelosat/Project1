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
    public partial class Server : INetEndpoint
    {
        public double CurrentTick => ServerClock.TotalMilliseconds;

        static Server()
        {
            //RegisterPacketHandler(PacketType.Acks, Instance.ReceiveAcks);
        }

        readonly string _name = "Server";
        public override string ToString()
        {
            return this._name;
        }
        // TODO: figure out why it's glitching out if I set it lower than 10
        public const int ClockIntervalMS = 10;// 10 is working
        public TimeSpan Clock => ServerClock;
        static TimeSpan ServerClock;

        static bool IsRunning;

        bool IsSaving;

        UI.ConsoleBoxAsync _Console;
        public UI.ConsoleBoxAsync ConsoleBox
        {
            get
            {
                this._Console ??= new UI.ConsoleBoxAsync(new Rectangle(0, 0, 800, 500)) { FadeText = false };
                return this._Console;
            }
        }

        public const int SnapshotIntervalMS = 10;// send 60 snapshots per second to clients
        public const int LightIntervalMS = 10;// send 60 light updates per second to clients
        readonly Dictionary<int, GameObject> NetworkObjects;

        /// <summary>
        /// Contains objects that have changed since the last world delta state update
        /// </summary>
        public HashSet<GameObject> ObjectsChangedSinceLastSnapshot = [];

        static readonly Dictionary<int, PacketHandlerWithPlayer> PacketHandlersWithPlayer = [];
        static readonly Dictionary<int, PacketHandlerWithPacket> PacketHandlersWithPacket = [];
        public static void RegisterPacketHandlerWithPlayer(int id, PacketHandlerWithPlayer handler)
        {
            PacketHandlersWithPlayer.Add(id, handler);
        }
        internal static void RegisterPacketHandler(int id, PacketHandlerWithPacket handler)
        {
            PacketHandlersWithPacket.Add(id, handler);
        }
        static readonly Dictionary<int, PacketHandler> PacketHandlersGeneric = [];

        internal static void RegisterPacketHandler(int id, PacketHandler handler)
        {
            PacketHandlersGeneric.Add(id, handler);
        }
        void OnGameEvent(GameEvent e)
        {
            GameMode.Current.HandleEvent(this, e);
            foreach (var item in Game1.Instance.GameComponents)
                item.OnGameEvent(e);

            Instance.Map.OnGameEvent(e);
        }
        public void EventOccured(Message.Types type, params object[] p)
        {
            var e = new GameEvent(ServerClock.TotalMilliseconds, type, p);
            this.OnGameEvent(e);
        }

        public static CancellationTokenSource ChunkLoaderToken = new();
        static Server _Instance;
        public static Server Instance => _Instance ??= new Server();
        public NetworkSideType Type => NetworkSideType.Server;

        static int _refIdSequence = 1;

        public static int GetNextObjID()
        {
            return _refIdSequence++;
        }
        static int _playerID = 1;
        public static int PlayerID => _playerID++;
        public Server()
        {
            this.NetworkObjects = new Dictionary<int, GameObject>();
        }
        private void AdvanceClock()
        {
            ServerClock = ServerClock.Add(TimeSpan.FromMilliseconds(ClockIntervalMS));
        }

        /// <summary>
        /// TODO: Make it a field
        /// </summary>
        public MapBase Map { get; set; }

        static WorldBase _World;
        static WorldBase World
        {
            get => _World;
            set => _World = value;
        }
        public static int Port = 5541;
        static Socket Listener;
        public PlayerList Players;

        public IEnumerable<PlayerData> GetPlayers()
        {
            return this.Players.GetList();
        }

        public static RandomThreaded Random;

        public static void Stop()
        {
            Server.ChunkLoaderToken.Cancel();
            Listener?.Close();

            Instance.ConsoleBox?.Write("SERVER", "Stopped");
            Instance.OutgoingStream = new BinaryWriter(new MemoryStream());
            Instance.OutgoingStreamUnreliable = new BinaryWriter(new MemoryStream());
            Instance.OutgoingStreamReliable = new BinaryWriter(new MemoryStream());

            Instance.Players = new PlayerList(Instance);
            IsRunning = false;
            Instance.Map = null;
            Connections = new ConcurrentDictionary<EndPoint, UdpConnection>();
            Instance.NetworkObjects.Clear();
            ServerClock = new TimeSpan();
            Instance.Speed = 0;
        }
        public static void Start()
        {
            Instance.Speed = 0;
            IsRunning = true;
            Instance.OutgoingStream = new BinaryWriter(new MemoryStream());
            Instance.OutgoingStreamUnreliable = new BinaryWriter(new MemoryStream());
            Instance.OutgoingStreamReliable = new BinaryWriter(new MemoryStream());

            Connections = new ConcurrentDictionary<EndPoint, UdpConnection>();
            Instance.NetworkObjects.Clear();
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
        public int Speed { get => this._Speed; set => this._Speed = value; }
        readonly float BlockUpdateTimerMax = 1;
        float BlockUpdateTimer = 0;

        public void Update(GameTime gt)
        {
            if (!IsRunning)
                return;
            HandleIncoming();
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
            this.CreatePacketsFromAllStreams();
            this.WritePlayerSpecific();
            ResetOutgoingStreams();
            Instance.AdvanceClock();
            this.SendPackets();
        }

        private void WritePlayerSpecific()
        {
            // SEND PLAYER SPECIFIC ACKS
            foreach(var player in this.Players.GetList())
            {
                var qu = player.AckQueue;
                if (qu.IsEmpty)
                    continue;
                using var str = new BinaryWriter(new MemoryStream());
                PacketAcks.Send(str, player);
                var data = ((MemoryStream)str.BaseStream).ToArray();
                var p = Packet.Create(player, PacketType.MergedPackets, data, SendType.Unreliable);
                p.Synced = true;
                p.Tick = this.Clock.TotalMilliseconds;
                this.Enqueue(player, p);
            }
        }

        private void SendPackets()
        {
            this.SendUnreliable();
            this.TryResendPacketsFirst(); //try resend first or all?
            this.SendOrderedReliable();
        }
        private void CreatePacketsFromAllStreams()
        {
            this.CreatePacketsFromStreamUnreliable();
            this.CreatePacketsFromStream();
        }
        private static void ResetOutgoingStreams()
        {
            Instance.OutgoingStream = new BinaryWriter(new MemoryStream());
            Instance.OutgoingStreamUnreliable = new BinaryWriter(new MemoryStream());
            Instance.OutgoingStreamReliable = new BinaryWriter(new MemoryStream());
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
                this.OutgoingStream.Write(Network.Packets.PacketTimestamped);
                auxStream.BaseStream.CopyTo(this.OutgoingStream.BaseStream);
            }
        }

        public static int RandomBlockUpdatesCount = 1;
        static int RandomBlockUpdateIndex = 0;
        void SendRandomBlockUpdates() // TODO move this to map class. server object shouldn't contain map logic
        {
            var tosend = new Vector3[Instance.Map.ActiveChunks.Count];
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
                while (player.Incoming.TryDequeue(out Packet msg))
                    HandleMessage(msg);
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

        private void CreatePacketsFromStream()
        {
            var data = ((MemoryStream)this.OutgoingStream.BaseStream).ToArray();
           
            //if (data.Length > 0) // send empty packet anyway to substitute pinging for keeping an active connection
            this.Enqueue(PacketType.MergedPackets, data);
        }
        private void CreatePacketsFromStreamUnreliable()
        {
            var data = ((MemoryStream)this.OutgoingStreamUnreliable.BaseStream).ToArray();
            //if (data.Length > 0) // send empty packet anyway to substitute pinging for keeping an active connection
            this.EnqueueUnreliable(PacketType.MergedPackets, data);
        }

        internal void EnqueueUnreliable(PacketType packetType, byte[] p)
        {
            this.Enqueue(packetType, p, SendType.Unreliable, true);
        }
        internal void Enqueue(PacketType packetType, byte[] p, bool sync = true)
        {
            this.Enqueue(packetType, p, SendType.OrderedReliable, sync);
        }

        internal void Enqueue(PlayerData player, Packet packet)
        {
            if ((packet.SendType & SendType.Reliable) == SendType.Reliable)
            {
                if (packet.SendType == SendType.OrderedReliable)
                    packet.Tick = this.Clock.TotalMilliseconds;
                player.OutReliable.Enqueue(packet);
            }
            else
                player.OutUnreliable.Enqueue(packet);
        }
        public void Enqueue(PacketType type, byte[] data, SendType send)
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
        internal void Enqueue(PacketType type, byte[] data, SendType send, Vector3 global)
        {
            this.Enqueue(type, data, send, player => player.IsWithin(global));
        }
        internal void Enqueue(PacketType type, byte[] data, SendType send, Vector3 global, bool sync)
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
        internal void Enqueue(PacketType type, byte[] data, SendType send, bool sync)
        {
            foreach (var player in this.Players.GetList())
            {
                var p = Packet.Create(player, type, data, send);
                p.Synced = sync;
                p.Tick = this.Clock.TotalMilliseconds;
                this.Enqueue(player, p);
            }
        }
        internal void Enqueue(PacketType type, byte[] data, SendType send, Func<PlayerData, bool> filter)
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

        public BinaryWriter OutgoingStream = new(new MemoryStream());
        public BinaryWriter OutgoingStreamUnreliable = new(new MemoryStream());
        public BinaryWriter OutgoingStreamReliable = new(new MemoryStream());
        public BinaryWriter OutgoingStreamTimestamped = new(new MemoryStream());

        public BinaryWriter GetOutgoingStream()
        {
            return this.OutgoingStream;
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

                player.Incoming.Enqueue(packet);

                if ((packet.SendType & SendType.Reliable) == SendType.Reliable)
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
                    }), SendType.Reliable | SendType.Ordered));

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

                case PacketType.PlayerSlotClick:
                    TargetArgs actor = TargetArgs.Read(Instance, r);
                    TargetArgs target = TargetArgs.Read(Instance, r);
                    Instance.PostLocalEvent(target.Slot.Parent, Components.Message.Types.SlotInteraction, actor.Object, target.Slot);
                    Instance.Enqueue(PacketType.PlayerSlotClick, msg.Payload, SendType.OrderedReliable);
                    return;

                case PacketType.PlayerRemoteCall:
                    int netid = r.ReadInt32();
                    target = TargetArgs.Read(Instance, r);
                    Message.Types call = (Message.Types)r.ReadInt32();
                    int dataLength = (int)(r.BaseStream.Length - r.BaseStream.Position);
                    byte[] args = r.ReadBytes(dataLength);
                    target.HandleRemoteCall(Instance, ObjectEventArgs.Create(call, args));
                    Instance.Enqueue(PacketType.PlayerRemoteCall, msg.Payload, SendType.OrderedReliable, msg.Player.ControllingEntity.Global);
                    return;

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
                w.Write(parent.RefID);
                w.Write(childIndex);
            });
            foreach (var player in this.Players.GetList())
                this.Enqueue(player, Packet.Create(player, PacketType.SpawnChildObject, data, SendType.Ordered | SendType.Reliable));
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
                existing.Player.ControllingEntity.Despawn();
            Instance.DisposeObject(existing.Player.CharacterID);
            PacketPlayerDisconnected.Send(Instance, existing.Player.ID);
        }

        public GameObject Instantiate(GameObject obj)
        {
            //obj.Instantiate(this.Instantiator);
            foreach (var o in obj.GetSelfAndChildren())
                this.Instantiator(o);
            return obj;
        }
        public void Instantiator(GameObject obj)
        {
            if (obj.RefID == 0)
                obj.RefID = GetNextObjID();
            else
                _refIdSequence = Math.Max(_refIdSequence, obj.RefID + 1);
            //if (Instance.NetworkObjects.TryGetValue(obj.RefID, out var existing))
            //{
            //    if (existing == obj) return;
            //    else throw new Exception();
            //}
            obj.Net = this;
            Instance.NetworkObjects.Add(obj.RefID, obj);
        }

        /// <summary>
        /// Releases the object's networkID.
        /// </summary>
        /// <param name="objNetID"></param>
        public bool DisposeObject(GameObject obj)
        {
            return this.DisposeObject(obj.RefID);
        }
        /// <summary>
        /// Releases the object's networkID.
        /// </summary>
        /// <param name="objNetID"></param>
        public bool DisposeObject(int netID)
        {
            if (!this.NetworkObjects.TryGetValue(netID, out GameObject o))
                return false;
            foreach (var obj in o.GetSelfAndChildren())
            {
                Console.WriteLine($"{this} disposing {obj.DebugName}");
                obj.OnDispose();
                this.NetworkObjects.Remove(netID);
                obj.Net = null;
                obj.RefID = 0;
                if (obj.IsSpawned)
                    obj.Despawn();
                //foreach (var child in from slot in o.GetChildren() where slot.HasValue select slot.Object)
                //    this.DisposeObject(child);
            }
            return true;
        }
        public void SyncDispose(int refID)
        {
            this.DisposeObject(refID);
            PacketEntityDispose.Send(this, refID, null);
        }
        public static void SetMap(MapBase map)
        {
            World = map.World;
            World.Net = Instance;
            Instance.Map = map;
            foreach (var ch in map.GetActiveChunks().Values)
                InstantiateChunk(ch);
            foreach (var obj in Instance.NetworkObjects)
                obj.Value.MapLoaded(Instance.Map);
            map.ResolveReferences();
            Random = new RandomThreaded(Instance.Map.Random);
        }
        private static void InstantiateChunk(Chunk chunk)
        {
            chunk.GetObjects().ForEach(obj =>
            {
                Instance.Instantiate(obj);
                _refIdSequence = Math.Max(_refIdSequence, obj.RefID + 1);
                //obj.Map = Instance.Map; // this isn't necessary, i set the entity's map field when loading the chunk
            });

            foreach (var (local, entity) in chunk.GetBlockEntitiesByPosition())
            {
                var global = local.ToGlobal(chunk);
                entity.ResolveReferences(Instance.Map, global);
                foreach (var o in entity.GetChildren())
                    Instance.Instantiate(o);
            }
        }

        public GameObject GetNetworkEntity(int netID)
        {
            this.NetworkObjects.TryGetValue(netID, out var obj);
            return obj;
        }
        public T GetNetworkObject<T>(int netID) where T : GameObject
        {
            this.NetworkObjects.TryGetValue(netID, out var obj);
            return obj as T;
        }

        public IEnumerable<GameObject> GetNetworkObjects()
        {
            foreach (var o in this.NetworkObjects.Values)
                yield return o;
        }
        public IEnumerable<GameObject> GetNetworkObjects(IEnumerable<int> netID)
        {
            return (from o in this.NetworkObjects where netID.Contains(o.Key) select o.Value);
        }
        public bool TryGetNetworkObject(int netID, out GameObject obj)
        {
            return this.NetworkObjects.TryGetValue(netID, out obj);
        }

        public void PostLocalEvent(GameObject recipient, ObjectEventArgs args)
        {
            args.Network = Instance;
            recipient.PostMessage(args);
        }
        public void PostLocalEvent(GameObject recipient, Components.Message.Types type, params object[] args)
        {
            ObjectEventArgs a = ObjectEventArgs.Create(type, args);
            a.Network = Instance;
            recipient.PostMessage(a);
        }

        private static void SendSnapshots(TimeSpan gt)
        {
            if (Instance.ObjectsChangedSinceLastSnapshot.Count > 0)
            {
                PacketSnapshots.Send(Instance, Instance.ObjectsChangedSinceLastSnapshot);
                Instance.ObjectsChangedSinceLastSnapshot.Clear();
            }
        }

        public bool LogStateChange(int netID)
        {
            return this.ObjectsChangedSinceLastSnapshot.Add(this.NetworkObjects[netID]);
        }

        #region Loot
        public void PopLoot(LootTable table, Vector3 startPosition, Vector3 startVelocity)
        {
            foreach (var obj in this.GenerateLoot(table))
                this.PopLoot(obj, startPosition, startVelocity);
        }
        public void PopLoot(GameObject obj, Vector3 startPosition, Vector3 startVelocity)
        {
            double angle = Random.NextDouble() * (Math.PI + Math.PI);
            double w = Math.PI / 4f;

            float verticalForce = .3f;// 0.3f;
            float horizontalForce = .1f;
            float x = horizontalForce * (float)(Math.Sin(w) * Math.Cos(angle));
            float y = horizontalForce * (float)(Math.Sin(w) * Math.Sin(angle));
            float z = verticalForce * (float)Math.Cos(w);

            var direction = new Vector3(x, y, z);
            var final = startVelocity + direction;

            obj.Global = startPosition;
            obj.Velocity = final;

            if (obj.RefID == 0)
                obj.SyncInstantiate(this);
            this.Map.SyncSpawn(obj, startPosition, final);
        }
        public IEnumerable<GameObject> GenerateLoot(LootTable lootTable)
        {
            //foreach (var i in lootTable.GenerateNew(Random))
            //    yield return i;
            foreach (var i in lootTable.Generate(Random))
                yield return i;
        }
        #endregion

        public RandomThreaded GetRandom()
        {
            return Random;
        }

        public static void SetWorld(WorldBase world)
        {
            if (!UnloadWorld())
                return;
            World = world;
            if (World is not null)
                Instance.ConsoleBox.Write(Color.Lime, "SERVER", "World " + World.Name + " loaded");
        }
        private static bool UnloadWorld()
        {
            if (Connections.Count > 0)
            {
                Instance.ConsoleBox.Write(Color.Red, "SERVER", "Can't unload world while active connections exist");
                return false;
            }
            if (World != null)
            {
                Instance.ConsoleBox.Write(Color.Lime, "SERVER", "World " + World.Name + " unloaded");
            }
            World = null;
            return true;
        }

        ServerCommandParser Parser;
        public static void Command(string command)
        {
            if (Instance.Parser == null)
                Instance.Parser = new ServerCommandParser(Instance);
            Instance.Parser.Command(command);
        }
       
        public void SyncSlots(params TargetArgs[] slots)
        {
            byte[] data = Network.Serialize(w =>
            {
                w.Write(slots.Length);
                foreach (var slot in slots)
                {
                    slot.Write(w);
                    w.Write(slot.Slot.StackSize);
                    if (slot.Slot.StackSize > 0)
                        w.Write(slot.Slot.Object.RefID);
                }
            });
            Instance.Enqueue(PacketType.EntityInventoryChange, data, SendType.OrderedReliable); // WARNING!!! TODO: handle case where each slot is owned by a different entity     
        }
        private static void UnmergePackets(Packet packet)
        {
            var player = packet.Player;
            var r = packet.Reader;
            var mem = r.BaseStream;
            var lastPos = mem.Position;
            while (mem.Position < mem.Length)
            {
                var typeID = r.ReadInt32();
                lastPos = mem.Position;

                if (PacketHandlersWithPlayer.TryGetValue(typeID, out var handlerActionNew))
                    handlerActionNew(Instance, player, r);
                else if (PacketHandlersGeneric.TryGetValue(typeID, out var handlerActionNewNew))
                    handlerActionNewNew(Instance, r);
                else if (PacketHandlersWithPacket.TryGetValue(typeID, out var handler))
                    handler(Instance, packet);
                if (mem.Position == lastPos)
                    // if the stream position hasn't changed, and we're still not at the end, it means that there are no packet handlers registered to read the next set of data. break or throw?
                    //throw new Exception();
                    break; // i think that's the price of not sending the length as the header and just continuing to read until the packethandler is invalid, which implies we reached the end. but that doesnt sound very clean
            }
        }

        public static void StartSaving()
        {
            Instance.IsSaving = true;
            Instance.Enqueue(PacketType.SetSaving, Network.Serialize(w => { w.Write(true); }));
        }
        public static void FinishSaving()
        {
            Instance.IsSaving = false;
            Instance.Enqueue(PacketType.SetSaving, Network.Serialize(w => { w.Write(false); }));
        }
        public PlayerData CurrentPlayer => null; //placeholder until a server session supports a player and not just be dedicated
        public PlayerData GetPlayer()
        {
            return this.CurrentPlayer;
        }
        public PlayerData GetPlayer(int id)
        {
            return this.Players.GetPlayer(id);
        }
        public void SetSpeed(int playerID, int speed)
        {
            this.GetPlayer(playerID).SuggestedSpeed = speed;
            var newspeed = this.Players.GetLowestSpeed();
            this.Speed = newspeed;
        }

        public void Write(string text)
        {
        }

        public void Report(string text)
        {
            this.Write(text);
        }

        public void SyncReport(string text)
        {
            this.Report(text);
            Network.SyncReport(this, text);
        }

        public void WriteToStream(params object[] args)
        {
            this.GetOutgoingStream().Write(args);
        }
    }
}
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Start_a_Town_.Net
{
    internal enum PlayerSavingState
    { Saved, Changed, Saving }

    public partial class Client : NetEndpoint// INetEndpoint
    {
        private static Client _Instance;
        public static Client Instance => _Instance ??= new Client();

        public override double CurrentTick => this.ClientClock.TotalMilliseconds;

        private UI.ConsoleBoxAsync _Console;
        public override UI.ConsoleBoxAsync ConsoleBox => this._Console ??= new UI.ConsoleBoxAsync(new Rectangle(0, 0, 400, 400)) { FadeText = false }; //new Rectangle(0, 0, 800, 600)

        public UI.ConsoleBoxAsync GetConsole()
        {
            return this.ConsoleBox;
        }

        private bool IsRunning;

        private long _packetID = 1;
        public long NextPacketID => this._packetID++;
        private long RemoteSequence = 0;

        public override MapBase Map
        {
            set => Engine.Map = value;
            get => Engine.Map;
        }
        public override WorldBase World { get; set; }

        private readonly int TimeoutLength = Ticks.PerSecond * 2;
        private int Timeout = -1;

        private const int OrderedReliablePacketsHistoryCapacity = 64;
        private readonly Queue<Packet> OrderedReliablePacketsHistory = new(OrderedReliablePacketsHistoryCapacity);

        private Client()
        {
        }

        public PlayerData PlayerData;
        public Socket Host;
        public EndPoint RemoteIP;
        public PlayerList Players;
        private ConcurrentQueue<Packet> IncomingAll = new();
        private readonly PriorityQueue<long, Packet> IncomingOrdered = new();
        private readonly PriorityQueue<long, Packet> IncomingOrderedReliable = new();
        private readonly PriorityQueue<long, Packet> IncomingSynced = new();
        private ConcurrentDictionary<Vector2, ConcurrentQueue<Action<Chunk>>> ChunkCallBackEvents;
        private TimeSpan ClientClock = new();
        private double LastReceivedTime = int.MinValue;
        public static bool IsSaving;

        public override TimeSpan Clock => this.ClientClock;


        public BinaryWriter OutgoingStreamUnreliable => this.GetStream(ReliabilityType.Unreliable).Writer;// this[ReliabilityType.Unreliable];
        public BinaryWriter OutgoingStreamOrderedReliable => this.GetStream(ReliabilityType.OrderedReliable).Writer;// this[ReliabilityType.OrderedReliable];
        public BinaryWriter OutgoingStreamReliable => this.GetStream(ReliabilityType.Reliable).Writer;// this[ReliabilityType.Reliable];

        public override BinaryWriter GetOutgoingStreamOrderedReliable()
        {
            return this.OutgoingStreamOrderedReliable;
        }

        public BinaryWriter OutgoingStreamTimestamped = new(new MemoryStream());

        private readonly Queue<WorldSnapshot> WorldStateBuffer = new();
        private readonly int WorldStateBufferSize = 10;
        public const int ClientClockDelayMS = Server.SnapshotIntervalMS * 4;
        private int _Speed = 0;// 1;
        public override int Speed { get => this._Speed; set => this._Speed = value; }

        public void Disconnect()
        {
            this.IsRunning = false;
            Instance.World = null;
            Engine.Map = null;
            this.Timeout = -1;
            Packet.Create(this.NextPacketID, PacketType.PlayerDisconnected).BeginSendTo(this.Host, this.RemoteIP, a => { });
            this.IncomingAll = new ConcurrentQueue<Packet>();
            this.ClientClock = new TimeSpan();
            this.SyncedPackets = new Queue<Packet>();
        }

        /// <summary>
        /// Called when communication with server times out
        /// </summary>
        private void Disconnected()
        {
            this.IsRunning = false;
            "receiving pakets from server timed out".ToConsole();
            this.Timeout = -1;
            this.World = null;
            Engine.Map = null;
            this.IncomingAll = new ConcurrentQueue<Packet>();
            this.SyncedPackets = new Queue<Packet>();

            ScreenManager.GameScreens.Clear();
            ScreenManager.Add(MainScreen.Instance);
            this.EventOccured(Message.Types.ServerNoResponse);
            this.ClientClock = new TimeSpan();
        }

        public void Connect(string address, string playername, AsyncCallback callBack)
        {
            this.Connect(address, new PlayerData(playername), callBack);
        }

        public void Connect(string address, PlayerData playerData, AsyncCallback callBack)
        {
            this.PlayerData = playerData;
            this.SyncedPackets = new Queue<Packet>();
            this.Timeout = this.TimeoutLength;
            this.LastReceivedTime = int.MinValue;
            this.IsRunning = true;
            this.ChunkCallBackEvents = new ConcurrentDictionary<Vector2, ConcurrentQueue<Action<Chunk>>>();
            this.RecentPackets = new Queue<long>();
            this.RemoteSequence = 0;
            this._packetID = 1;
            this.IncomingOrderedReliable.Clear();
            this.IncomingOrdered.Clear();
            this.IncomingSynced.Clear();
            this.IncomingAll = new ConcurrentQueue<Packet>();
            this.ClientClock = new TimeSpan();
            this.Players = new PlayerList(this);
            if (this.Host != null)
                this.Host.Close();
            this.Host = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.Host.ReceiveBufferSize = this.Host.SendBufferSize = Packet.Size;
            if (!IPAddress.TryParse(address, out IPAddress ipAddress))
            {
                try
                {
                    var fromdns = Dns.GetHostEntry(address);
                    ipAddress = fromdns.AddressList[0];
                }
                catch (Exception)
                {
                    "error resolving hostname".ToConsole();
                    return;
                }
            }
            this.RemoteIP = new IPEndPoint(ipAddress, 5541);
            var state = new UdpConnection("Server", this.Host) { Buffer = new byte[Packet.Size] };
            this.Host.Bind(new IPEndPoint(IPAddress.Any, 0));

            byte[] data = Packet.Create(this.NextPacketID, PacketType.RequestConnection, playerData.Name.Serialize()).ToArray();

            this.Host.SendTo(data, this.RemoteIP);
            this.Host.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, a =>
            {
                // connection established
                // enter main receive loop
                callBack(a);
                this.ReceiveMessage(a);
            }, state);
        }

        public void Connect(IPAddress ipAddress, PlayerData playerData, AsyncCallback callBack)
        {
            this.SyncedPackets = new Queue<Packet>();
            this.Timeout = this.TimeoutLength;
            this.LastReceivedTime = int.MinValue;
            this.IsRunning = true;
            this.ChunkCallBackEvents = new ConcurrentDictionary<Vector2, ConcurrentQueue<Action<Chunk>>>();
            this.RecentPackets = new Queue<long>();
            this.RemoteSequence = 0;
            this.PlayerData = playerData;
            this._packetID = 1;
            this.IncomingOrderedReliable.Clear();
            this.IncomingOrdered.Clear();
            this.IncomingSynced.Clear();
            this.IncomingAll = new ConcurrentQueue<Packet>();
            this.ClientClock = new TimeSpan();
            this.Players = new PlayerList(this);
            if (this.Host != null)
                this.Host.Close();
            this.Host = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.Host.ReceiveBufferSize = this.Host.SendBufferSize = Packet.Size;

            this.RemoteIP = new IPEndPoint(ipAddress, 5541);
            var state = new UdpConnection("Server", this.Host) { Buffer = new byte[Packet.Size] };
            this.Host.Bind(new IPEndPoint(IPAddress.Any, 0));

            byte[] data = Packet.Create(this.NextPacketID, PacketType.RequestConnection, Network.Serialize(w =>
            {
                w.Write(playerData.Name);
            })).ToArray();

            this.Host.SendTo(data, this.RemoteIP);
            this.Host.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, a =>
            {
                // connection established
                // enter main receive loop
                callBack(a);
                this.ReceiveMessage(a);
            }, state);
        }

        public override IEnumerable<PlayerData> GetPlayers()
        {
            return this.Players.GetList();
        }

        public void Update()
        {
            this.Timeout--;
            if (this.Timeout == 0)
                this.Disconnected();
            if (!this.IsRunning)
                return;

            this.HandleOrderedPackets();
            this.HandleOrderedReliablePackets();
            this.HandleSyncedPackets();
            this.ProcessIncomingPackets();
            GameMode.Current?.Update(Instance);

            if (Instance.Map is not null)
            {
                var size = Instance.Map.GetSizeInChunks();
                var maxChunks = size * size;
                if (Instance.Map.ActiveChunks.Count == maxChunks && !IsSaving)
                {
                    for (int i = 0; i < Instance.Speed; i++)
                        this.TickMap();
                    this.Map.Update();
                    this.UpdateWorldState();
                }
            }
            //this.ClientClock = this.ClientClock.Add(TimeSpan.FromMilliseconds(Server.ClockIntervalMS));

            if (this.PlayerData is not null && this.Map is not null)
                PacketMousePosition.Send(Instance, this.PlayerData.ID, ToolManager.CurrentTarget); // TODO: do this at the toolmanager class instead of here

            PacketAcks.Send(this, this.PlayerData); // acks are written to the unreliable packet
                                                    //simulate the unreliable packet getting lost
                                                    //var packetLost = _random.Chance(.8);
                                                    //if (!packetLost)
            this.SendOutgoingStreamsArray();
            //if (_random.Chance(.05))
            //    simLossStreak = _random.Next(5, 20);
            //if (simLossStreak > 0)
            //    simLossStreak--;
            //else
            //                this.SendOutgoingStreamsArray();

            this.TryResendPacketsFirst();
            this.ResetStreams();
        }
        int simLossStreak;
        Random _random = new();
        private readonly SortedDictionary<ulong, (ulong worldtick, double servertick, byte[] data)> BufferTimestamped = [];
        private readonly SortedDictionary<ulong, (ulong worldtick, double servertick, Packet packet, int payloadPos, int frameLength)> BufferTimestampedNew = [];

        private ulong lasttickreceived;

        public void HandleTimestamped(Packet packet)
        {
            var r = packet.Reader;
            var currenttick = this.Map.World.CurrentTick;
            for (int i = 0; i < this.Speed; i++)
            {
                var mapTick = r.ReadUInt64();
                var serverTick = r.ReadDouble();
                var length = r.ReadInt64();
                int frameStart = (int)r.BaseStream.Position;
                if (length > 0)
                    r.BaseStream.Position += length; // skip data, dont read them. because i read them when handling them

                if (mapTick == currenttick)
                {
                    r.BaseStream.Position = frameStart;
                    this.UnmergePackets(packet, (int)length);
                }
                else
                    this.BufferTimestampedNew[mapTick] = (mapTick, serverTick, packet, frameStart, (int)length);

                if (mapTick < this.lasttickreceived)
                    throw new Exception();
                this.lasttickreceived = mapTick;
            }
        }

        private void HandleBufferedTimestampedNew()
        {
            while (this.BufferTimestampedNew.Count != 0)
            {
                var item = this.BufferTimestampedNew.First();
                var currenttick = this.Map.World.CurrentTick;
                if (item.Key != currenttick)
                    return;
                var packet = item.Value.packet;
                packet.Reader.BaseStream.Position = item.Value.payloadPos;
                this.UnmergePackets(item.Value.packet, item.Value.frameLength);
                this.BufferTimestampedNew.Remove(item.Key);
            }
        }
        private void TickMap()
        {
            this.HandleBufferedTimestampedNew();
            this.Map.UpdateParticles();
            this.Map.World.Tick(Instance);
            this.Map.Tick();
            //this.HandleBufferedTimestamped();
            /// move this to after the map ticks because workcomponent ticks before aicomponent,
            /// which results new interactions getting ticked in the next frame on the server,
            /// but getting ticked in the same frame when received on the client
            /// SOLUTIONS:
            /// 1) manually add aicomponent before workcomponent inside entities
            /// 2) process packets after ticking map
            /// 3) add a timestamp on the actual interaction class during the frame that it's first ticked on the server, and make clients tick it only then as well
        }

        void ResetStreams()
        {
            foreach (var s in this.StreamsArray)
                s.Reset();
        }
        private void OnGameEvent(GameEvent e)
        {
            GameMode.Current.HandleEvent(Instance, e);

            foreach (var item in Game1.Instance.GameComponents)
                item.OnGameEvent(e);
            UI.TooltipManager.OnGameEvent(e);
            ScreenManager.CurrentScreen.OnGameEvent(e);

            ToolManager.OnGameEvent(this, e);
            this.Map?.OnGameEvent(e);
        }

        [Obsolete]
        private static readonly Dictionary<PacketType, Action<INetEndpoint, IDataReader>> PacketHandlersNew = new();

        [Obsolete]
        public static void RegisterPacketHandler(PacketType channel, Action<INetEndpoint, IDataReader> handler)
        {
            PacketHandlersNew.Add(channel, handler);
        }

        public override void EventOccured(Message.Types type, params object[] p)
        {
            var e = new GameEvent(this.ClientClock.TotalMilliseconds, type, p);
            this.OnGameEvent(e);
        }

        private void HandleSyncedPackets()
        {
            while (this.SyncedPackets.Count > 0)
            {
                var next = this.SyncedPackets.Peek();
                if (next.Tick > this.ClientClock.TotalMilliseconds)
                    return;
                this.SyncedPackets.Dequeue();
                this.HandleMessage(next);
            }
        }

        /// <summary>
        /// Immediately handles unreliable packets, but enqueues ordered and reliable packets to be handled by this.HandleOrderedPackets() and this.HandleOrderedReliablePackets()
        /// </summary>
        private void ProcessIncomingPackets()
        {
            while (this.IncomingAll.TryDequeue(out Packet packet))
            {
                if (packet.PacketType == PacketType.RequestConnection)
                {
                    this.HandleMessage(packet);
                    continue;
                }
                // if the timer is not stopped (not -1), reset it
                if (this.Timeout > -1)
                    this.Timeout = this.TimeoutLength;

                if (this.IsDuplicate(packet))
                {
                    continue;
                }
                this.RecentPackets.Enqueue(packet.ID);
                if (this.RecentPackets.Count > this.RecentPacketBufferSize)
                    this.RecentPackets.Dequeue();

                // clock correction happens first, for all packets
                double target = packet.Tick - ClientClockDelayMS;
                double curr = this.ClientClock.TotalMilliseconds;
                double smoothed = curr + (target - curr) * 0.15;
                this.ClientClock = TimeSpan.FromMilliseconds(Math.Max(smoothed, 0));

                //this.ClientClock = TimeSpan.FromMilliseconds(target);
                // for ordered packets, only handle last one (store most recent and discard and older ones)
                if (packet.Reliability == ReliabilityType.Ordered)
                {
                    this.IncomingOrdered.Enqueue(packet.ID, packet);//e);
                }
                else if (packet.Reliability == ReliabilityType.OrderedReliable)
                {
                    if (packet.OrderedReliableID > this.PlayerData.RemoteOrderedReliableSequence)
                        this.IncomingOrderedReliable.Enqueue(packet.OrderedReliableID, packet);
                }
                else
                {
                    //var clientms = packet.Tick - ClientClockDelayMS;
                    //if (this.CurrentTick < clientms)
                    //{
                    //    this.ClientClock = TimeSpan.FromMilliseconds(clientms);
                    //    "client clock caught up".ToConsole();
                    //}
                    this.HandleMessage(packet);
                }
            }
        }

        public void SavePlayer(GameObject actor, BinaryWriter writer)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, "Character");

            var charTag = new SaveTag(SaveTag.Types.Compound, "PlayerCharacter", actor.SaveInternal());

            // save metadata such as hotbar

            tag.Add(charTag);

            tag.WriteTo(writer);
        }

        void TryResendPacketsFirst()
        {
            if (NetworkHelper.TryResend(this.Host, this.RemoteIP, this.PlayerData) is { } timedout)
                throw new TimeoutException($"Exceeded maximum retries to resend packet {timedout.ID}");
        }
       
        private void UnmergePackets(Packet packet, long maxBytes = -1)
        {
            var r = packet.PacketReader;
            var lastPos = r.Position;
            var endPos = maxBytes == -1 ? r.Length : lastPos + maxBytes;
            var packetsHandled = 0;
            var lastHandled = -1;
            while (r.Position < endPos)
            {
                var id = r.ReadInt32();
                var type = (PacketType)id;
                lastPos = r.Position;

                if (PacketHandlersNew.TryGetValue(type, out Action<INetEndpoint, IDataReader> handlerAction))
                    handlerAction(Instance, r);
                else
                    base.HandlePacket(id, packet);

                if (r.Position == lastPos)
                    break;
                packetsHandled++;
                lastHandled = id;
            }
        }
        
        private string _name = "Client";

        public override string ToString()
        {
            return this._name;
        }

        private void HandleMessage(Packet msg)
        {
            
            var r = msg.PacketReader;
            switch (msg.PacketType)
            {
                case PacketType.RequestConnection:
                    this.Timeout = this.TimeoutLength;
                    this.PlayerData.ID = r.ReadInt32();
                    this.Players = PlayerList.Read(Instance, r);
                    this.Speed = r.ReadInt32();
                    Log.Network(this, $"Connected to {this.RemoteIP}");
                    GameMode.Current.PlayerIDAssigned(this);
                    this.ClientClock = TimeSpan.FromMilliseconds(Math.Max(msg.Tick - ClientClockDelayMS, 0));
                    this.PlayerData.RemoteOrderedReliableSequence = msg.OrderedReliableID;
                    Instance.EventOccured(Message.Types.ServerResponseReceived);
                    break;

                case PacketType.PlayerDisconnected:
                    int plid = msg.Reader.ReadInt32();
                    Instance.PlayerDisconnected(plid);
                    break;

                case PacketType.SpawnChildObject:
                    GameObject obj = GameObject.Create(msg.PacketReader);
                    if (obj.RefId == 0)
                        throw new Exception("Uninstantiated entity");
                    if (!Instance.World.Entities.ContainsKey(obj.RefId))
                        Instance.Instantiate(obj);

                    int parentID = r.ReadInt32();
                    if (!Instance.World.TryGetEntity(parentID, out var parent))
                        throw (new Exception("Parent doesn't exist"));

                    obj.Parent = parent;
                    int childIndex = r.ReadInt32();
                    var slot = parent.GetChildren()[childIndex];
                    slot.Object = obj;
                    return;

                case PacketType.ServerBroadcast:
                    string chatText = r.ReadASCII();
                    Network.Console.Write(Color.Yellow, "SERVER", chatText);
                    break;

                case PacketType.PlayerServerCommand:
                    Instance.ParseCommand(r.ReadASCII());
                    break;

                case PacketType.MergedPackets:
                    this.UnmergePackets(msg);
                    break;

                default:
                    if (PacketHandlersNew.TryGetValue(msg.PacketType, out Action<INetEndpoint, IDataReader> handlerNew))
                        handlerNew(this, msg.PacketReader);
                    break;
            }
        }

        public void SetSaving(bool val)
        {
            IsSaving = val;
            Log.System(IsSaving ? "Saving..." : "Game saved");
        }

        private void SyncTime(double serverMS)
        {
            if (this.LastReceivedTime > serverMS)
            {
                ("sync time packet dropped (last: " + this.LastReceivedTime.ToString() + ", received: " + serverMS.ToString()).ToConsole();// + "server: " + Server.ServerClock.TotalMilliseconds.ToString() + ")").ToConsole();
                return;
            }

            this.LastReceivedTime = serverMS;
            var newtime = serverMS - ClientClockDelayMS;

            var serverTime = TimeSpan.FromMilliseconds(newtime);

            this.ClientClock = serverTime;
        }

        private void ParseCommand(string command)
        {
            CommandParser.Execute(this, command);
        }

        private readonly int OrderedPacketsHistoryCapacity = 32;
        private readonly Queue<Packet> OrderedPacketsHistory = new(32);

        private void HandleOrderedPackets()
        {
            while (this.IncomingOrdered.Count > 0)
            {
                Packet packet = this.IncomingOrdered.Dequeue();

                this.HandleMessage(packet);
                this.OrderedPacketsHistory.Enqueue(packet);
                while (this.OrderedPacketsHistory.Count > this.OrderedPacketsHistoryCapacity)
                    this.OrderedPacketsHistory.Dequeue();
            }
        }

        private Queue<Packet> SyncedPackets = new();

        private void HandleOrderedReliablePackets()
        {
            while (this.IncomingOrderedReliable.Count > 0)
            {
                var next = this.IncomingOrderedReliable.Peek();
                long nextid = next.OrderedReliableID;
                if (nextid == this.PlayerData.RemoteOrderedReliableSequence + 1)
                {
                    this.PlayerData.RemoteOrderedReliableSequence = nextid;
                    Packet packet = this.IncomingOrderedReliable.Dequeue();
                    if (next.Tick > Instance.Clock.TotalMilliseconds) // TODO maybe use this while changing clock to ad
                    {
                        this.SyncedPackets.Enqueue(next);
                        continue;
                    }
                    this.HandleMessage(packet);
                    this.OrderedReliablePacketsHistory.Enqueue(packet);
                    while (this.OrderedReliablePacketsHistory.Count > OrderedReliablePacketsHistoryCapacity)
                        this.OrderedReliablePacketsHistory.Dequeue();
                }
                else
                    return;
            }
        }

        /// <summary>
        /// Both removes an object form the game world and releases its networkID
        /// </summary>
        /// <param name="objNetID"></param>
        public override bool DisposeObject(GameObject obj)
        {
            return this.DisposeObject(obj.RefId);
        }

        public override bool DisposeObject(int netId)
        {
            return this.World.DisposeEntity(netId);
        }

        /// <summary>
        /// Is passed recursively to an object and its children objects (inventory items) to register their network ID.
        /// </summary>
        /// <param name="ob"></param>
        /// <returns></returns>
        public override GameObject Instantiate(GameObject ob)
        {
            foreach (var obj in ob.GetSelfAndChildren())
                this.Instantiator(obj);
            return ob;
        }

        public GameObject InstantiateLocal(GameObject ob)
        {
            throw new Exception();
        }

        public override void Instantiator(GameObject ob)
        {
            ob.Net = this;
            Instance.World.Register(ob as Entity);
        }

        internal void AddPlayer(PlayerData player)
        {
            this.Players.Add(player);
            UI.LobbyWindow.RefreshPlayers(this.Players.GetList());
            Log.Network(this, $"{player.Name} connected");
            Log.System($"{player.Name} connected");
        }

        private void PlayerDisconnected(PlayerData player)
        {
            this.Players.Remove(player);
            if (Instance.Map != null && player.ControllingEntity != null)
            {
                Instance.World.Entities[player.CharacterID].Despawn();
                Instance.World.DisposeEntity(player.CharacterID);
            }
            Log.Network(this, $"{player.Name} disconnected");
            Log.System($"{player.Name} disconnected");
        }

        public void PlayerDisconnected(int playerID)
        {
            PlayerData player = this.Players.GetList().FirstOrDefault(p => p.ID == playerID);
            if (player is null)
                return;
            this.PlayerDisconnected(player);
        }

        private void ReceiveMessage(IAsyncResult ar)
        {
            try
            {
                UdpConnection state = (UdpConnection)ar.AsyncState;
                int bytesRead = state.Socket.EndReceive(ar);
                if (bytesRead == Packet.Size)
                    throw new Exception("buffer full");

                byte[] bytesReceived = state.Buffer.Take(bytesRead).ToArray();
                this.Host.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, this.ReceiveMessage, state);

                Packet packet = Packet.Read(bytesReceived);
                packet.Player = this.PlayerData;

                if ((packet.Reliability & ReliabilityType.Reliable) == ReliabilityType.Reliable)
                    this.PlayerData.AckQueue.Enqueue(packet.ID);

                this.IncomingAll.Enqueue(packet);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception e)
            {
                // this is thrown (at least) twice because the server sends 2 packets per frame (reliable+unreliable) + any resent reliable packets
                //ScreenManager.Remove(); // this is not the main thread. if i remove from here then in case the main thread is drawing, it won't be able to access the ingame camera class
                //// so either don't remove the screen here, or pass the camera to the root draw call instead of accessing it through the screenmanager currentscreen property
                this.Timeout = 0;
                e.ToConsole();
                //e.ShowDialog();
            }
        }

        private readonly int RecentPacketBufferSize = 32;
        private Queue<long> RecentPackets = new();

        private bool IsDuplicate(Packet packet)
        {
            long id = packet.ID;
            if (id > this.RemoteSequence)
            {
                if (id - this.RemoteSequence > 31)
                {
                    // very large jump in packets
                    this.ConsoleBox.Write(Color.Orange, "CLIENT", "Warning! Large gap in received packets!");
                }
                this.RemoteSequence = id;
                return false;
            }
            else if (id == this.RemoteSequence)
                return true;
            BitVector32 field = this.GenerateBitmask();
            int distance = (int)(this.RemoteSequence - id);
            if (distance > 31)
            {
                // very old packet
                //this.ConsoleBox.Write(Color.Orange, "CLIENT", "Warning! Received severely outdated packet: " + packet.PacketType.ToString());
                this.ConsoleBox.Write(Color.Orange, "CLIENT", $"Warning! Received severely outdated packet: {packet.PacketType.ToString()} / Packet ID: {id} / RemoteSequence: {this.RemoteSequence}");
                // this can occur on a bad connection, or even if the client's ack for this specific initial packet was lost and never reached the server
                return false;
            }
            int mask = (1 << distance);
            bool found = (field.Data & mask) == mask;
            if (found)
                if (distance < 32)
                    if (!this.RecentPackets.Contains(id))
                        throw new Exception("duplicate detection error");
            return found;
        }

        private BitVector32 GenerateBitmask()
        {
            int mask = 0;
            foreach (var recent in this.RecentPackets)
            {
                int distance = (int)(this.RemoteSequence - recent);
                if (distance > 31)
                    continue;
                mask |= 1 << distance;
                var test = new BitVector32(mask);
            }
            var bitvector = new BitVector32(mask);
            return bitvector;
        }

        public HashSet<Vector2> ChunkRequests = new();

        public void ReceiveChunk(Chunk chunk)
        {
            this.ChunkRequests.Remove(chunk.MapCoords);

            if (this.Map.GetActiveChunks().ContainsKey(chunk.MapCoords))
            {
                (chunk.MapCoords.ToString() + " already loaded").ToConsole();
                return;
            }
            chunk.Map = this.Map;

            chunk.GetObjects().ForEach(obj =>
            {
                this.Instantiate(obj);
                obj.MapLoaded(Instance.Map);
                /// why here too? BECAUSE some things dont get initialized properly on client. like initializing sprites from defs
                //obj.ObjectLoaded();
                /// FIXED by saving and serializing sprites along bones (by using the assetpath and the static sprite registry)
            });

            foreach (var (local, entity) in chunk.GetBlockEntitiesByPosition())
            {
                var global = local.ToGlobal(chunk);
                entity.ResolveReferences(this.Map, global);
                //entity.Instantiate(global, Instance.Instantiator);
                foreach (var o in entity.GetChildren())
                    Instance.Instantiate(o);
            }

            Instance.Map.AddChunk(chunk);
            return;
        }

        /// <summary>
        /// The client can't create objects, must await for a server message
        /// </summary>
        /// <param name="obj"></param>
        public GameObject InstantiateObject(GameObject obj)
        {
            return obj;
        }

        public override GameObject GetNetworkEntity(int netID)
        {
            this.World.Entities.TryGetValue(netID, out var obj);
            return obj;
        }

        public override T GetNetworkObject<T>(int netID)
        {
            this.World.Entities.TryGetValue(netID, out var obj);
            return obj as T;
        }

        public override IEnumerable<GameObject> GetNetworkObjects()
        {
            foreach (var o in this.World.Entities.Values)
                yield return o;
        }

        public override bool TryGetNetworkObject(int netID, out Entity obj)
        {
            return this.World.TryGetEntity(netID, out obj);
        }

        /// <summary>
        /// find way to write specific changes, maybe by passing a state Object
        /// </summary>
        /// <param name="netID"></param>
        /// <returns></returns>
        public override bool LogStateChange(int netID)
        {
            return false;
        }

        //internal void ReadSnapshot(BinaryReader reader)
        internal void ReadSnapshot(IDataReader reader)
        {
            double totalMs = reader.ReadDouble();

            var time = TimeSpan.FromMilliseconds(totalMs);
            var worldState = new WorldSnapshot(time, reader);

            // insert world snapshot to world snapshot history
            this.WorldStateBuffer.Enqueue(worldState);
            while (this.WorldStateBuffer.Count > this.WorldStateBufferSize)
                this.WorldStateBuffer.Dequeue();
        }

        private void UpdateWorldState()
        {
            // iterate through the state buffer and find position
            List<WorldSnapshot> list = this.WorldStateBuffer.ToList();
            for (int i = 0; i < this.WorldStateBuffer.Count - 1; i++)
            {
                WorldSnapshot
                    prev = list[i],
                    next = list[i + 1];

                if (this.ClientClock >= prev.Time && this.ClientClock < next.Time)
                {
                    this.SnapObjectPositions(prev, next);
                    return;
                }
            }
        }
        private void SnapObjectPositions(WorldSnapshot prev, WorldSnapshot next)
        {
            float t = (float)((ClientClock.TotalMilliseconds - prev.Time.TotalMilliseconds) /
                  (next.Time.TotalMilliseconds - prev.Time.TotalMilliseconds));
            t = Math.Clamp(t, 0f, 1f);

            foreach (var kv in prev.Dictionary)
            {
                var prevSnap = kv.Value;
                next.Dictionary.TryGetValue(prevSnap.RefID, out var nextSnap);
                if (nextSnap is null)
                    nextSnap = prevSnap;
                var entity = this.GetNetworkEntity(prevSnap.RefID);

                entity.SetPosition(prevSnap.Position + (prevSnap.Position - prevSnap.Position) * t);
                entity.Velocity = prevSnap.Velocity + (prevSnap.Velocity - prevSnap.Velocity) * t;
                entity.Direction = prevSnap.Orientation + (prevSnap.Orientation - prevSnap.Orientation) * t;

                if (float.IsNaN(entity.Direction.X) || float.IsNaN(entity.Direction.Y))
                    throw new Exception();
            }

            foreach(var kv in next.Dictionary)
            {
                if (prev.Dictionary.ContainsKey(kv.Key))
                    continue;

                var nextObj = kv.Value;
                var entity = this.GetNetworkEntity(nextObj.RefID);
                if (entity == null) continue;

                // Policy for spawns: snap to the authoritative snapshot immediately.
                // Alternative: treat prev as same as next and interpolate from same => same.
                entity.SetPosition(nextObj.Position);
                entity.Velocity = nextObj.Velocity;
                entity.Direction = nextObj.Orientation;
            }
        }

        internal static void PlayerCommand(string command)
        {
            var p = command.Split(' ');
            var type = p[0];
            switch (type)
            {
                case "reset":
                    ScreenManager.CurrentScreen.Camera.OnDeviceLost();
                    return;

                case "rebuildchunks":
                    foreach (var chunk in Instance.Map.GetActiveChunks().Values)
                    {
                        chunk.LightCache.Clear();
                        chunk.InvalidateMesh();
                    }
                    return;

                default:
                    break;
            }

            Network.Serialize(writer =>
            {
                writer.WriteASCII(command);
            }).Send(Instance.NextPacketID, PacketType.PlayerServerCommand, Instance.Host, Instance.RemoteIP);
        }

        private void Send(PacketType packetType, byte[] data, ReliabilityType sendType)
        {
            var packet = Packet.Create(this.NextPacketID, packetType, sendType, data);
            if ((packet.Reliability & ReliabilityType.Reliable) == ReliabilityType.Reliable)
            {
                packet.OrderedReliableID = this.PlayerData.OrderedReliableSequence++;
                this.PlayerData.WaitingForAck[packet.ID] = packet;
            }
            packet.BeginSendTo(this.Host, this.RemoteIP);
        }

        /// <summary>
        /// Does nothing on client!
        /// </summary>
        /// <param name="packetType"></param>
        /// <param name="payload"></param>
        /// <param name="sendType"></param>
        public override void Enqueue(PacketType packetType, byte[] payload, ReliabilityType sendType)
        { }

        /// <summary>
        /// Posts event data to a local object
        /// </summary>
        /// <param name="data">A serialized ObjectEventArgs array</param>
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

        public override void PopLoot(GameObject loot, Vector3 startPosition, Vector3 startVelocity)
        { }

        public override void PopLoot(LootTable table, Vector3 startPosition, Vector3 startVelocity)
        { }

        public void InventoryOperation(GameObjectSlot sourceSlot, GameObjectSlot targetSlot, int amount)
        {
            var sourceParent = sourceSlot.Parent;
            var destinationParent = targetSlot.Parent;
            if (targetSlot == sourceSlot)
                return;
            if (!targetSlot.Filter(sourceSlot.Object))
                return;

            this.Map.GetChunk(sourceParent.Global).Invalidate();
            this.Map.GetChunk(destinationParent.Global).Invalidate();

            var obj = sourceSlot.Object;
            if (!targetSlot.HasValue) // if target slot empty, set object of target slot without swapping and return
            {
                if (amount < sourceSlot.StackSize) // if the amount moved is smaller than the source amount
                {
                    sourceSlot.Object.StackSize -= amount;
                    // DO NOTHING. WAIT FOR NEW OBJECT FROM SERVER INSTEAD
                    return;
                }
                else
                    sourceSlot.Clear();
                targetSlot.Object = obj;
                return;
            }
            if (targetSlot.Object.CanAbsorb(sourceSlot.Object))
            {
                if (sourceSlot.StackSize + targetSlot.StackSize <= targetSlot.StackMax)
                {
                    targetSlot.StackSize += sourceSlot.StackSize;
                    this.DisposeObject(sourceSlot.Object.RefId);
                    sourceSlot.Clear();
                    //merge slots
                    return;
                }
            }
            else
                if (amount < sourceSlot.StackSize)
                return;

            if (targetSlot.Filter(obj))
                if (sourceSlot.Filter(targetSlot.Object))
                    targetSlot.Swap(sourceSlot);
        }

        public override PlayerData GetPlayer(int id)
        {
            return this.Players.GetPlayer(id);
        }

        public override PlayerData GetPlayer()
        {
            return this.GetPlayer(this.PlayerData.ID);
        }

        public override PlayerData CurrentPlayer => this.PlayerData;

        internal void HandleServerResponse(int playerID, PlayerList playerList, int speed)
        {
            throw new Exception();
        }

        public override void SetSpeed(int playerID, int playerSpeed)
        {
            var player = this.GetPlayer(playerID);
            player.SuggestedSpeed = playerSpeed;
            var newspeed = this.Players.GetLowestSpeed();
            if (newspeed != this.Speed)
                //Ingame.Instance.Hud.Chat.Write(Start_a_Town_.Log.EntryTypes.System, string.Format("Speed set to {0}x ({1})", newspeed, string.Join(", ", this.Players.GetList().Where(p => p.SuggestedSpeed == newspeed).Select(p => p.Name))));
                Log.Network(this, $"Speed set to to {newspeed}"); // TODO prevent spam
            else
                Log.Network(this, $"{player.Name} wants to set speed to {playerSpeed}"); // TODO prevent spam
            this.Speed = newspeed;
        }

        public override void Write(string text)
        {
            Log.Write(text);
        }

        public override void Report(string text)
        {
            this.Write(text);
        }

        public override void SyncReport(string text)
        {
        }

        public override void WriteToStream(params object[] args)
        {
            this.GetOutgoingStreamOrderedReliable().Write(args);
        }
      
        private void SendOutgoingStreamsArray()
        {
            foreach (var i in this.StreamsArray)
                if (i.Writer.BaseStream.Position > 0)
                {
                    var data = i.GetBytes();
                    if (data.Length > 0)
                        this.Send(PacketType.MergedPackets, data, i.Reliability);
                }
        }
    }
}
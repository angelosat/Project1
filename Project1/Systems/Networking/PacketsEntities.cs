using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal class PacketsEntities
    {
        static readonly int _pRegister, _pDispose, _pStackIncrease, _pStackDecrease, _pSpawn, _pDespawn;
        static PacketsEntities()
        {
            _pRegister = Registry.PacketHandlers.Register(OnRegister);
            _pDispose = Registry.PacketHandlers.Register(OnDispose);
            _pStackIncrease = Registry.PacketHandlers.Register(OnStackIncrease);
            _pStackDecrease = Registry.PacketHandlers.Register(OnStackDecrease);
            _pSpawn = Registry.PacketHandlers.Register(OnSpawn);
            _pDespawn = Registry.PacketHandlers.Register(OnDespawn);

            Registry.WorldEventHooksServer.Register<EntityRegisteredEvent>(SendEntityRegistered);
            Registry.WorldEventHooksServer.Register<EntityStackIncreased>(SendEntityStackIncreased);
            Registry.WorldEventHooksServer.Register<EntityStackDecreased>(SendEntityStackDecreased);
            Registry.WorldEventHooksServer.Register<EntityDisposedEvent>(SendEntityDisposed);
            Registry.MapEventHooksServer.Register<EntitySpawnedEvent>(SendEntitySpawned);
            Registry.MapEventHooksServer.Register<EntityDespawnedEvent>(SendEntityDespawned);
        }

        private static void SendEntityDespawned(EntityDespawnedEvent @event)
        {
            Server.Instance.BeginPacket(_pDespawn)
                .Write(@event.Entity.RefId);
        }

        private static void OnDespawn(NetEndpoint endpoint, Packet packet)
        {
            var entityId = packet.PacketReader.ReadEntityRefId();
            var entity = endpoint.World.GetEntity(entityId);
            if (entity is null)
                return;
            endpoint.Map.Despawn(entity);
        }

        private static void SendEntitySpawned(EntitySpawnedEvent e)
        {
            var w = 
                e.Immediate ? 
                Server.Instance.BeginPacketImmediate(_pSpawn) 
                :  Server.Instance.BeginPacket(_pSpawn)
                ;
            w
                .Write((int)e.Entity.RefId)
                .Write(e.Entity.Global)
                .Write(e.Entity.Velocity);
        }

        private static void OnSpawn(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            var id = r.ReadEntityRefId();
            var global = r.ReadVector3();
            var vel = r.ReadVector3();
            var entity = endpoint.World.GetEntity(id);
            endpoint.Map.Spawn(entity, global, vel);
        }

        private static void SendEntityStackIncreased(EntityStackIncreased increased)
        {
            Server.Instance.BeginPacket(_pStackIncrease)
                .Write(increased.Entity.RefId)
                .Write(increased.Amount);
        }
        private static void OnStackIncrease(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var entity = client.World.GetEntity(r.ReadEntityRefId());
            var amount = r.ReadInt32();
            entity.Add(amount);
        }
        private static void SendEntityStackDecreased(EntityStackDecreased decreased)
        {
            Server.Instance.BeginPacket(_pStackDecrease)
                .Write(decreased.Entity.RefId)
                .Write(decreased.Amount);
        }
        private static void OnStackDecrease(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var entity = client.World.GetEntity(r.ReadEntityRefId());
            var amount = r.ReadInt32();
            entity.Consume(amount);
        }
        private static void SendEntityRegistered(EntityRegisteredEvent e)
        {
            var w = e.Immediate ? Server.Instance.BeginPacketImmediate(_pRegister) : Server.Instance.BeginPacket(_pRegister);
            e.Entity.Write(w);
        }
        private static void OnRegister(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var obj = GameObject.Create(r);
            client.World.Register(obj);
        }
        private static void SendEntityDisposed(EntityDisposedEvent @event)
        {
            Server.Instance.BeginPacket(_pDispose)
                .Write(@event.Entity.RefId);
        }
        private static void OnDispose(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var refid = r.ReadEntityRefId();
            client.World.TryDisposeEntity(refid);
        }
    }

    internal record struct EntityStackDecreased(Entity Entity, int Amount) : IEventPayload { }
    internal record struct EntityStackIncreased(Entity Entity, int Amount) : IEventPayload { }
    internal record struct EntityRegisteredEvent(Entity Entity, bool Immediate = false) : IEventPayload { }
    internal record struct EntityDisposedEvent(Entity Entity) : IEventPayload { }
    internal record struct EntitySpawnedEvent(Entity Entity, bool Immediate = false) : IEventPayload { }
    internal record struct EntityDespawnedEvent(Entity Entity) : IEventPayload { }
}

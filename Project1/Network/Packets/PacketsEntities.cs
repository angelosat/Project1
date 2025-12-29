using Start_a_Town_.Net;
using System;

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

            Registry.WorldEventHooks.Register<EntityRegisteredEvent>(SendEntityRegistered);
            Registry.WorldEventHooks.Register<EntityStackIncreased>(SendEntityStackIncreased);
            Registry.WorldEventHooks.Register<EntityStackDecreased>(SendEntityStackDecreased);
            Registry.WorldEventHooks.Register<EntityDisposedEvent>(SendEntityDisposed);
            Registry.MapEventHooks.Register<EntitySpawnedEvent>(SendEntitySpawned);
            Registry.MapEventHooks.Register<EntityDespawnedEvent>(SendEntityDespawned);
        }

        private static void SendEntityDespawned(EntityDespawnedEvent @event)
        {
            Server.Instance.BeginPacket(_pDespawn)
                .Write(@event.Entity.RefId);
        }

        private static void OnDespawn(NetEndpoint endpoint, Packet packet)
        {
            endpoint.Map.Despawn(
                packet.PacketReader.ReadEntityRefId());
        }

        private static void SendEntitySpawned(EntitySpawnedEvent @event)
        {
            Server.Instance.BeginPacket(_pSpawn)
                .Write(@event.Entity.RefId)
                .Write(@event.Entity.Global)
                .Write(@event.Entity.Velocity);
        }

        private static void OnSpawn(NetEndpoint endpoint, Packet packet)
        {
            endpoint.Map.Spawn(
                packet.PacketReader.ReadEntityRefId(),
                packet.PacketReader.ReadVector3(),
                packet.PacketReader.ReadVector3());
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
            e.Entity.Write(Server.Instance.BeginPacket(_pRegister));
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
            client.World.DisposeEntity(r.ReadEntityRefId());
        }
    }

    internal class EntityStackDecreased(Entity entity, int amount) : EventPayloadBase
    {
        public readonly Entity Entity = entity;
        public readonly int Amount = amount;
    }

    internal class EntityStackIncreased(Entity entity, int amount) : EventPayloadBase
    {
        public readonly Entity Entity = entity;
        public readonly int Amount = amount;
    }

    internal class EntityRegisteredEvent(Entity entity) : EventPayloadBase
    {
        public readonly Entity Entity = entity;
    }
}

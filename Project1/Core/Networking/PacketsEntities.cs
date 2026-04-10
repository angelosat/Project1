using Project1.Framework;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Entities;

namespace Project1.Core.Networking
{
    [EnsureStaticCtorCall]
    internal class PacketsEntities
    {
        static readonly PacketId _pRegister, _pDispose, _pStackIncrease, _pStackDecrease, _pSpawn, _pDespawn, _pCompUpdated;
        static PacketsEntities()
        {
            _pRegister = Registry.PacketHandlers.Register(OnRegister);
            _pDispose = Registry.PacketHandlers.Register(OnDispose);
            _pStackIncrease = Registry.PacketHandlers.Register(OnStackIncrease);
            _pStackDecrease = Registry.PacketHandlers.Register(OnStackDecrease);
            _pSpawn = Registry.PacketHandlers.Register(OnSpawn);
            _pDespawn = Registry.PacketHandlers.Register(OnDespawn);
            _pCompUpdated = Registry.PacketHandlers.Register(OnCompUpdated);

            Registry.WorldEventHooksServer.Register<EntityRegisteredEvent>(SendEntityRegistered);
            Registry.WorldEventHooksServer.Register<EntityStackIncreased>(SendEntityStackIncreased);
            Registry.WorldEventHooksServer.Register<EntityStackDecreased>(SendEntityStackDecreased);
            Registry.WorldEventHooksServer.Register<EntityDisposedEvent>(SendEntityDisposed);
            Registry.WorldEventHooksServer.Register<EntityCompUpdatedEvent>(SendEntityCompUpdated);

            Registry.MapEventHooksServer.Register<EntitySpawnedEvent>(SendEntitySpawned);
            Registry.MapEventHooksServer.Register<EntityDespawnedEvent>(SendEntityDespawned);
        }

        private static void OnCompUpdated(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var entityid = r.ReadInt32();
            var compindex = r.ReadInt32();
            var entity = client.World.Get(entityid);
            var comp = entity.Components.GetComp(compindex);
            comp.Read(r);
        }

        private static void SendEntityCompUpdated(EntityCompUpdatedEvent e)
        {
            Server.Instance.BeginPacket(_pCompUpdated)
                .Write(e.Comp.Owner.RefId)
                .Write(e.Comp.RuntimeIndex)
                .Write(e.Comp);
        }

        private static void SendEntityDespawned(EntityDespawnedEvent e)
        {
            Server.Instance.BeginPacket(_pDespawn)
                .Write(e.Entity.RefId);
        }

        private static void OnDespawn(NetEndpoint endpoint, Packet packet)
        {
            var entityId = packet.PacketReader.ReadEntityRefId();
            var entity = endpoint.World.Get(entityId);
            if (entity is null)
                return;
            entity.Map.Despawn(entity);
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
                .Write(e.Map.ID)
                .Write(e.Entity.Global)
                .Write(e.Entity.Velocity);
        }

        private static void OnSpawn(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            var id = r.ReadEntityRefId();
            var mapid = r.ReadMapId();
            var map = endpoint.World.Get(mapid);
            var global = r.ReadVector3();
            var vel = r.ReadVector3();
            var entity = endpoint.World.Get(id);
            map.Spawn(entity, global, vel);
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
            var entity = client.World.Get(r.ReadEntityRefId());
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
            var entity = client.World.Get(r.ReadEntityRefId());
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
            var obj = GameObject.Create(r, client.World);
            client.World.RegisterInt(obj);
            //client.World.Register(obj);
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
  
}

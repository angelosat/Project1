using Project1.Core.Blocks.Comps;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Resources;
using Project1.Framework;
using System;

namespace Project1.Core.Networking
{
    [EnsureStaticCtorCall]
    internal static class Packets_Resources
    {
        readonly static PacketId 
            _pResourceDelta = Registry.PacketHandlers.Register(ReceiveResourceDelta),
            _pResourceValue = Registry.PacketHandlers.Register(ReceiveResourceValue),
            _pResourceChanged = Registry.PacketHandlers.Register(ReceiveResourceChanged),
            _pBlockResourceDelta = Registry.PacketHandlers.Register(ReceiveBlockResourceDelta),
            _pBlockResourceValue = Registry.PacketHandlers.Register(ReceiveBlockResourceValue);

       

        private static void ReceiveResourceValue(NetEndpoint endpoint, Packet packet)
        {
            throw new NotImplementedException();
        }

        static Packets_Resources()
        {
            Registry.WorldEventHooksServer.Register<ResourceDeltaAppliedEvent>(SendResourceDelta);
            Registry.WorldEventHooksServer.Register<ResourceChangedEvent>(SendResourceChanged);
            Registry.WorldEventHooksServer.Register<BlockResourceDeltaAppliedEvent>(SendBlockResourceDelta);
            Registry.WorldEventHooksServer.Register<BlockResourceValueSetEvent>(SendBlockResourceValue);
        }

        private static void SendResourceChanged(ResourceChangedEvent e)
        {
            var w = Server.Instance.BeginPacket(_pResourceChanged)
                .Write(e.Entity.RefId)
                .Write(e.Resource.Def);

            e.Resource.Write(w);
        }
        private static void ReceiveResourceChanged(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            var entity = endpoint.World.Get<Actor>(r.ReadEntityRefId());
            var def = r.ReadDef<ResourceDef>();
            //entity.Resources.View(def).Read(r);
            entity.Resources.Sync(def, r);
        }

        private static void SendBlockResourceValue(BlockResourceValueSetEvent e)
        {
            Server.Instance.BeginPacket(_pBlockResourceValue)
                .Write(e.Map.ID)
                .Write(e.Cell)
                .Write(e.Def)
                .Write(e.Value);
        }
        private static void ReceiveBlockResourceValue(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            var mapid = r.ReadMapId();
            var map = endpoint.World.Get(mapid);
            var cell = r.ReadIntVec3();
            var resDef = r.ReadDef<ResourceDef>();
            var delta = r.ReadInt32();
            map
                .Query(cell).BlockEntity.GetComp<BlockResourcesComp>()
                .SetValue(resDef, delta);
        }
        private static void SendResourceDelta(ResourceDeltaAppliedEvent e)
        {
            Server.Instance.BeginPacket(_pResourceDelta)
                .Write(e.Entity.RefId)
                .Write(e.Def)
                .Write(e.Delta);
        }
        static void ReceiveResourceDelta(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            var actorid = r.ReadId<EntityRefId>();
            var resDef = r.ReadDef<ResourceDef>();
            var delta = r.ReadInt32();
            var actor = endpoint.World.Get(actorid);
            actor.Resources.ApplyDelta(resDef, delta);
            //endpoint.World
            //    .Get(packet.PacketReader.ReadEntityRefId())
            //    .Resources.ApplyDelta(packet.PacketReader.ReadDef<ResourceDef>(), packet.PacketReader.ReadInt32());
        }
        static void SendBlockResourceDelta(BlockResourceDeltaAppliedEvent e)
        {
            Server.Instance.BeginPacket(_pBlockResourceDelta)
             .Write(e.Map.ID)
             .Write(e.Cell)
             .Write(e.Def)
             .Write(e.Delta);
        }
        private static void ReceiveBlockResourceDelta(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            var mapid = r.ReadMapId();
            var map = endpoint.World.Get(mapid);
            var cell = r.ReadIntVec3();
            var resDef = r.ReadDef<ResourceDef>();
            var delta = r.ReadInt32();
            map
                .Query(cell).BlockEntity.GetComp<BlockResourcesComp>()
                .ApplyDelta(resDef, delta);
        }
    }
}

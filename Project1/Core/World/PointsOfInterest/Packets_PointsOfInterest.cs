using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Core.World.WorldAreas;
using Project1.Framework;
using SharpDX.MediaFoundation;
using System;

namespace Project1.Core.World.PointsOfInterest;

[EnsureStaticCtorCall]
internal class Packets_PointsOfInterest
{
    static PacketId
        _pPoIAdded = Registry.PacketHandlers.Register(ReceivePoIAdded),
        _pPoIRemoved = Registry.PacketHandlers.Register(ReceivePoIRemoved),
        _pPoIActorAdded = Registry.PacketHandlers.Register(ReceiveActorAdded);

 

    static Packets_PointsOfInterest()
    {
        Registry.WorldEventHooksServer.Register<PoIAddedEvent>(SendPoIAdded);
        Registry.WorldEventHooksServer.Register<PoIActorAddedEvent>(SendActorAdded);
    }

    private static void SendActorAdded(PoIActorAddedEvent e)
    {
        Server.Instance.BeginPacket(_pPoIActorAdded)
            .Write(e.Actor.RefId)
            .Write(e.Frontier)
            .Write(e.PoIId);
    }

    private static void ReceiveActorAdded(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var world = (StaticWorld)endpoint.World;
        var actor = world.Get<Actor>(r.ReadEntityRefId());
        var frontierDef = r.ReadDef<FrontierDef>();
        var poiid = r.ReadInt32();
        world.Space.Get(frontierDef).PoIs[poiid].Add(actor);
    }

    private static void SendPoIAdded(PoIAddedEvent e)
    {
        Server.Instance.BeginPacket(_pPoIAdded)
            .Write(e.FrontierDef)
            .Write(e.PoI.Id)
            .Write(e.PoI.Def)
            .Write(e.PoI.Size);
    }

    private static void ReceivePoIAdded(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var frontierDef = r.ReadDef<FrontierDef>();
        var id = r.ReadInt32();
        var poiDef = r.ReadDef<PoIDef>();
        var size = r.ReadInt32();
        var world = (StaticWorld)endpoint.World;
        world.Space.Get(frontierDef).RegisterPoI(id, poiDef, size);
    }

    private static void ReceivePoIRemoved(NetEndpoint endpoint, Packet packet)
    {
        throw new NotImplementedException();
    }
}

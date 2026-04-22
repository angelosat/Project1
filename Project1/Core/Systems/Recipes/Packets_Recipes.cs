using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Framework;

namespace Project1.Core.Systems.Recipes;

[EnsureStaticCtorCall]
internal static class Packets_Recipes
{
    static readonly PacketId _pActorMastery = Registry.PacketHandlers.Register(ReceiveActorMastery);

    static Packets_Recipes()
    {
        Registry.WorldEventHooksServer.Register<ActorRecipeMasteryEvent>(HandleActorRecipeMastery);
    }

    private static void HandleActorRecipeMastery(ActorRecipeMasteryEvent e)
    {
        var actor = e.Actor;
        actor.Net.BeginPacket(_pActorMastery)
            .Write(actor.RefId)
            .Write(e.Knowledge.Recipe);
    }

    private static void ReceiveActorMastery(NetEndpoint endpoint, Packet packet)
    {
        var client = endpoint as Client;
        var r = packet.PacketReader;
        var actor = client.World.Get<Actor>(r.ReadId<EntityRefId>());
        actor.Recipes.Add(r.ReadDef());
    }
}

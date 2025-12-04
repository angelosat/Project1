using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketEntityInstantiate
    {
        static readonly int PckType;
        static readonly int PckTypeNew;
        static PacketEntityInstantiate()
        {
            PckType = Registry.PacketHandlers.Register(Receive);
            PckTypeNew = Registry.PacketHandlers.Register(ReceiveTemplate);
        }
        [Obsolete]
        static public void Send(NetEndpoint net, GameObject entity)
        {
            Send(net, [entity]);
        }
        static public void SendFromTemplate(NetEndpoint net, int templateID, GameObject entity)
        {
            if (net is Client)
                throw new Exception();
            var strem = net.BeginPacketOld(PckTypeNew);
            strem.Write(templateID);
            entity.Write(strem);
        }
        static public void ReceiveTemplate(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            if (net is Server)
                throw new Exception();
            var templateID = r.ReadInt32();
            var entity = GameObject.CloneTemplate(templateID, r);
            net.Instantiate(entity);
        }
        [Obsolete]
        static public void Send(NetEndpoint net, IEnumerable<GameObject> entities)
        {
            if (net is Client)
                throw new Exception();
            var strem = net.BeginPacketOld(PckType);
            strem.Write(entities.Count());
            foreach(var entity in entities)
            {
                if (entity.RefId != 0)
                    throw new Exception();
                net.Instantiate(entity);
                entity.Spawn(net.Map);
                entity.Write(strem);
            }
        }
        static public void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            if (net is Server)
                throw new Exception();
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                //var length = r.ReadInt32();
                //var data = r.ReadBytes(length);
                //var entity = Network.Deserialize<GameObject>(data, GameObject.Create);
                var entity = GameObject.Create(r);
                net.Instantiate(entity);
                if (entity.Exists)
                    entity.Spawn(net.Map);
            }
        }
    }
}

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
            PckType = PacketRegistry.Register(Receive);
            PckTypeNew = PacketRegistry.Register(ReceiveTemplate);
        }
        [Obsolete]
        static public void Send(INetEndpoint net, GameObject entity)
        {
            Send(net, [entity]);
        }
        static public void SendFromTemplate(INetEndpoint net, int templateID, GameObject entity)
        {
            if (net is Client)
                throw new Exception();
            //var strem = net.GetOutgoingStreamOrderedReliable();
            //strem.Write(PckTypeNew);
            var strem = net.BeginPacket(ReliabilityType.OrderedReliable, PckTypeNew);

            strem.Write(templateID);
            entity.Write(strem);
            //var data = entity.Serialize();
            //strem.Write(data.Length);
            //strem.Write(data);
        }
        static public void ReceiveTemplate(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            if (net is Server)
                throw new Exception();
            var templateID = r.ReadInt32();
            //var length = r.ReadInt32();
            //var data = r.ReadBytes(length);
            //var entity = Network.Deserialize(data, reader=> GameObject.CloneTemplate(templateID, reader));
            var entity = GameObject.CloneTemplate(templateID, r);
            net.Instantiate(entity);
        }
        [Obsolete]
        static public void Send(INetEndpoint net, IEnumerable<GameObject> entities)
        {
            if (net is Client)
                throw new Exception();
            //var strem = net.GetOutgoingStreamOrderedReliable();
            //strem.Write(PckType);
            var strem = net.BeginPacket(ReliabilityType.OrderedReliable, PckType);
            strem.Write(entities.Count());
            foreach(var entity in entities)
            {
                if (entity.RefId != 0)
                    throw new Exception();
                net.Instantiate(entity);
                entity.Spawn(net.Map);
                //var data = entity.Serialize();
                //strem.Write(data.Length);
                //strem.Write(data);
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

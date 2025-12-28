using Start_a_Town_.Net;
using System;
using System.Net;

namespace Start_a_Town_
{
    internal class PacketsConstruction
    {
        static readonly int _pSync, _pReady, _pFinished;

        static PacketsConstruction()
        {
            _pSync = Registry.PacketHandlers.Register(OnSync);
            _pReady = Registry.PacketHandlers.Register(OnReady);
            _pFinished = Registry.PacketHandlers.Register(OnFinished);
        }
        internal static void Finished(BlockConstructionComp comp)
        {
            var server = comp.Map.Net as Server;
            var w = server.BeginPacket(_pFinished)
                .Write(comp.Parent.OriginGlobal);
        }
        static void OnFinished(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var comp = client.Map.GetBlockEntityComp<BlockConstructionComp>(r.ReadIntVec3());
            comp.Map.Events.Post(new ConstructionFinishedEvent(comp));
        }
        public static void Ready(BlockConstructionComp comp)
        {
            var server = comp.Map.Net as Server;
            var w = server.BeginPacket(_pReady)
                .Write(comp.Parent.OriginGlobal);
        }
        private static void OnReady(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var comp = client.Map.GetBlockEntityComp<BlockConstructionComp>(r.ReadIntVec3());
            comp.Map.Events.Post(new ConstructionReadyEvent(comp));
        }

        public static void Sync(BlockConstructionComp comp)
        {
            var server = comp.Map.Net as Server;
            var w = server.BeginPacket(_pSync)
                .Write(comp.Parent.OriginGlobal);
            comp.Write(w);
        }
        private static void OnSync(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var comp = client.Map.GetBlockEntityComp<BlockConstructionComp>(r.ReadIntVec3());
            comp.Read(r);
        }

        

        //public override void Write(IDataWriter w)
        //{
        //    w.Write(this.Fulfilment.Current);
        //    w.Write(this.IsReady);
        //    if (this.IsReady)
        //        w.Write(this.Progress.Value);
        //}
        //public override ISerializable Read(IDataReader r)
        //{
        //    this.Fulfilment.Current = r.ReadInt32();
        //    if (r.ReadBoolean())
        //    {
        //        if (this.Progress is null)
        //            new Progress();
        //        this.Progress.Value = r.ReadSingle();
        //    }
        //    return this;
        //}
        //private void SyncProgress()
        //{
        //    var server = this.Map.Net as Server;
        //    server.BeginPacket(_pSyncProgress)
        //        .Write(this.Parent.OriginGlobal)
        //        .Write(this.Progress.Value);
        //}
        //private static void OnSyncProgress(NetEndpoint endpoint, Packet packet)
        //{
        //    var client = endpoint as Client;
        //    var r = packet.PacketReader;
        //    var comp = client.Map.GetBlockEntityComp<BlockConstructionComp>(r.ReadIntVec3());
        //    var amount = r.ReadSingle();
        //    comp.Progress.Value = amount;
        //}

        //private void SyncRequirement()
        //{
        //    var server = this.Map.Net as Server;
        //    server.BeginPacket(_pSyncFulfilment)
        //        .Write(this.Parent.OriginGlobal)
        //        .Write(this.Fulfilment.Current);
        //}
        //private static void OnSyncRequirement(NetEndpoint endpoint, Packet packet)
        //{
        //    var client = endpoint as Client;
        //    var r = packet.PacketReader;
        //    var comp = client.Map.GetBlockEntityComp<BlockConstructionComp>(r.ReadIntVec3());
        //    var amount = r.ReadInt32();
        //    comp.Fulfilment.Current += amount;
        //    comp.ValidateReadiness();
        //}
    }
}

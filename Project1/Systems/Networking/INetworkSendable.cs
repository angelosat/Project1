using Start_a_Town_.Net;

namespace Project1.Systems.Networking
{
    internal interface INetworkSendable
    {
        void SendTo(Client client);
    }
}

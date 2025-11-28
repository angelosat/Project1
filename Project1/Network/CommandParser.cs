namespace Start_a_Town_.Net
{
    interface ICommandParser
    {
        bool Execute(INetPeer net, string command);
    }
    class CommandParser
    {
        static public bool Execute(INetPeer net, PlayerData player, string command)
        {
            var p = command.Split(' ');
            var type = p[0];
            switch (type)
            {
                default:
                    GameMode.Current.ParseCommand(net, command);
                    break;
            }
            return false;
        }

        static public bool Execute(INetPeer net, string command)
        {
            var p = command.Split(' ');
            var type = p[0];
            switch (type)
            {
                case "fog":
                    var client = net as Client;
                    if(client!=null)
                    {
                        ScreenManager.CurrentScreen.Camera.FogLevel = int.Parse(p[1]);
                    }
                    if (net is Server)
                        (net as Server).Enqueue(PacketType.PlayerServerCommand, Network.Serialize(w => w.WriteASCII(command)), SendType.OrderedReliable);
                    break;

                default:
                    GameMode.Current.ParseCommand(net, command);
                    break;
            }
            return false;
        }
    }
}

using Project1.Core.UI;

namespace Project1.Core.Net
{
    class ServerUI : GroupBox
    {
        static ServerUI _Instance;
        static public ServerUI Instance
        {
            get
            {
                if (_Instance is null)
                    _Instance = new ServerUI();
                return _Instance;
            }
        }

        ServerUI()
        {
            this.Controls.Add(ServerConsole.Instance);
        }
    }
}

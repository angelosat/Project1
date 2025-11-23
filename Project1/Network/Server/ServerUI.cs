using Start_a_Town_.UI;

namespace Start_a_Town_.Net
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

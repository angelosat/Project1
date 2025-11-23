using Start_a_Town_.UI;

namespace Start_a_Town_.Net
{
    class ServerConsole : GroupBox
    {
        static ServerConsole _Instance;
        static public ServerConsole Instance => _Instance ??= new ServerConsole();
        
        ServerConsole()
        {
            Panel console = new Panel() { AutoSize = true };
            console.Controls.Add(Server.Instance.ConsoleBox);

            Panel input = new Panel() { Location = console.BottomLeft, AutoSize = true };
            TextBox txtbox = new TextBox()
            {
                Width = Server.Instance.ConsoleBox.Width,
                EnterFunc = (text) =>
                {
                    if (text.Length > 0)
                        Server.Command(text);
                }
            };
            input.Controls.Add(txtbox);
            this.Controls.Add(console, input);
        }
    }
}

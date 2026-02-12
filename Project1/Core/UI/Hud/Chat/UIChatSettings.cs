using Project1.Framework.Helpers;
using Project1.Framework.UI;

namespace Project1.Core.UI.Hud.Chat
{
    class UIChatSettings : Panel
    {
        public UIChatSettings(UIChat chat)
        {
            this.AutoSize = true;
            var chkTimestamps = new CheckBoxNew("Timestamps", toggleTimestamps, () => chat.Console.TimeStamp);
            this.AddControls(chkTimestamps);

            void toggleTimestamps()
            {
                chat.Console.TimeStamp = !chat.Console.TimeStamp;
                Engine.Config.SetValue("Interface/Timestamps", chat.Console.TimeStamp);
                Engine.SaveConfig();
            }
        }
    }
}

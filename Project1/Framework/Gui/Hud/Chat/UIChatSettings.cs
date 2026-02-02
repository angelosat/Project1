using Start_a_Town_.UI;

namespace Start_a_Town_
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

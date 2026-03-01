using System.Windows.Forms;

namespace Project1.Core.Input
{
    internal static class KeysHelper
    {
        extension(Keys key)
        {
            public string Label => GetLabel(key);
        }
        static string GetLabel(Keys key)
        {
            return key switch
            {
                Keys.D0 => "0",
                Keys.D1 => "1",
                Keys.D2 => "2",
                Keys.D3 => "3",
                Keys.D4 => "4",
                Keys.D5 => "5",
                Keys.D6 => "6",
                Keys.D7 => "7",
                Keys.D8 => "8",
                Keys.D9 => "9",
                Keys.Oemtilde => "~",
                Keys.Oemcomma => ",",
                Keys.OemPeriod => ".",
                Keys.OemPipe => "|",
                Keys.ControlKey => "Ctrl",
                Keys.ShiftKey => "Shift",
                Keys.Menu => "Alt",
                // Add other special keys as needed
                _ => key.ToString(),// fallback
            };
        }
    }
}

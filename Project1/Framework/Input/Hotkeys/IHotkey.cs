namespace Project1.Core.Input.Hotkeys
{
    public interface IHotkey
    {
        System.Windows.Forms.Keys[] ShortcutKeys { get; }
        string GetLabel();
        bool Contains(System.Windows.Forms.Keys key);
    }
}

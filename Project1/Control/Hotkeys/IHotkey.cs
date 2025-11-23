namespace Start_a_Town_
{
    public interface IHotkey
    {
        System.Windows.Forms.Keys[] ShortcutKeys { get; }
        string GetLabel();
        bool Contains(System.Windows.Forms.Keys key);
    }
}

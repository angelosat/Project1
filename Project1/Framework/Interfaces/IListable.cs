namespace Project1.Framework.UI
{
    public interface IListable
    {
        string LabelReadable { get; }
        Control GetListControlGui();
    }
}

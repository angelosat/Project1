using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class StorageContentsUI : GroupBox
    {
        TableObservable<GameObject> Table;
        BlockStorage.BlockStorageEntity Container;
        public StorageContentsUI()
        {
            this.Table = new TableObservable<GameObject>() { BackgroundStyle = BackgroundStyle.Tooltip }
                .AddColumn("mame", 150, o => new Label() { TextFunc = o.GetName });
            this.AddControls(this.Table);
        }
        public void Refresh(BlockStorage.BlockStorageEntity container)
        {
            this.Container = container;
            this.Table.Bind(this.Container.Contents);
        }
    }
}

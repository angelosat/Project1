using Project1.Core.Entities;
using Project1.Framework.UI;

namespace Project1.Core
{
    class StorageContentsUI : GroupBox
    {
        TableObservable<GameObject> Table;
        BlockStorage.BlockStorageEntity Container;
        public StorageContentsUI()
        {
            this.Table = new TableObservable<GameObject>() { BackgroundStyle = BackgroundStyle.Tooltip }
                .AddColumn("mame", 150, o => new Label() { TextFunc = () => o.Name });
            this.AddControls(this.Table);
        }
        public void Refresh(BlockStorage.BlockStorageEntity container)
        {
            this.Container = container;
            this.Table.Bind(this.Container.Contents);
        }
    }
}

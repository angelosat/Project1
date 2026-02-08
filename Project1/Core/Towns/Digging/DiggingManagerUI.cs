using Project1.Framework.UI;

namespace Project1.Core.Towns.Digging
{
    class DiggingManagerUI : GroupBox
    {
        IconButton BtnDesignate;
        public DiggingManagerUI(DiggingManager manager)
        {
            this.BtnDesignate = new IconButton()
            {
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                HoverFunc = () => "Designate digging\n\nLeft click & drag: Add digging\nCtrl+Left click: Remove digging",
                LeftClickAction = manager.Edit
            };
            this.Controls.Add(this.BtnDesignate);
        }
    }
}

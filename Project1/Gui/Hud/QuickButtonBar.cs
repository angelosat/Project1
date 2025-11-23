using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class QuickButtonBar : Panel
    {
        static QuickButtonBar Instance;
        static QuickButtonBar()
        {
            Instance = new QuickButtonBar();
        }

        static public void AddItem(IconButton btn)
        {
            Instance.AddControlsTopRight(btn);
        }
        static public void Popup()
        {
            Instance.Location = SelectionManager.Instance.TopRight;
            Instance.Anchor = Vector2.UnitY;
            Instance.Toggle();
        }
    }
}

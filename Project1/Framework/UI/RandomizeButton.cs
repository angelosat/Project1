using Project1.Framework.UI;

namespace Project1.Core.UI
{
    class RandomizeButton : IconButton
    {
        public RandomizeButton()
        {
            BackgroundTexture = UIManager.Icon16Background;
            Icon = new Icon(UIManager.Icons16x16, 1, 16);
            HoverText = "Randomize";
        }
    }
}

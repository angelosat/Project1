namespace Start_a_Town_.UI
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

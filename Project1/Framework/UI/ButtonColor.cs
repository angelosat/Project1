using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.UI;
using System;

namespace Project1.Core.UI
{
    class ButtonColorNew : IconButton
    {
        Color _selectedColor = Color.White;
        Func<Color> ColorGetter;
        Action<Color> ColorSetter;
        public Color SelectedColor => this.ColorGetter?.Invoke() ?? this._selectedColor;
        Rectangle Rect = new(UIManager.DefaultIconButtonSprite.Width / 2 - 5, UIManager.DefaultIconButtonSprite.Height / 2 - 5, 10, 10);
        ButtonColorNew()
        {
            this.BackgroundTexture = UIManager.Icon16Background;
        }
        public ButtonColorNew(Func<Color> colGetter, Action<Color> colSetter) : this()
        {
            this.ColorSetter = colSetter;
            this.ColorGetter = colGetter;
        }
        protected override void OnLeftClick()
        {
            var picker = ColorPickerBoxNew.Popup;
            picker.Refresh(this.ColorGetter, this.ColorSetter);
            picker.SetLocation(this.ScreenLocation + Vector2.UnitX * this.Width);
            picker.Show();
        }
        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
            this.Rect.X = (int)(this.ScreenLocation.X + UIManager.Icon16Background.Width / 2 - 5);
            this.Rect.Y = (this.Pressed ? 1 : 0) + (int)(this.ScreenLocation.Y + UIManager.Icon16Background.Height / 2 - 5);
            var screenRect = Rectangle.Intersect(viewport, this.Rect);
            this.DrawHighlight(sb, screenRect, this.SelectedColor);
        }
    }
    class ButtonColor : IconButton
    {
        Color _selectedColor = Color.White;
        public Func<Color> SelectedColorFunc;
        public Color SelectedColor => this.SelectedColorFunc?.Invoke() ?? this._selectedColor;
        Rectangle Rect = new(UIManager.DefaultIconButtonSprite.Width / 2 - 5, UIManager.DefaultIconButtonSprite.Height / 2 - 5, 10, 10);

        public ButtonColor()
        {
            this.BackgroundTexture = UIManager.Icon16Background;
        }

        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
            this.Rect.X = (int)(this.ScreenLocation.X + UIManager.Icon16Background.Width / 2 - 5);
            this.Rect.Y = (this.Pressed ? 1 : 0) + (int)(this.ScreenLocation.Y + UIManager.Icon16Background.Height / 2 - 5);
            var screenRect = Rectangle.Intersect(viewport, this.Rect);
            this.DrawHighlight(sb, screenRect, this.SelectedColor);
        }
    }
}

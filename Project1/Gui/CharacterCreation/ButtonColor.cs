using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Start_a_Town_.UI
{
    class ButtonColorNew : IconButton
    {
        Color _selectedColor = Color.White;
        Func<Color> ColorGetter;
        Action<Color> ColorSetter;
        public Color SelectedColor => this.ColorGetter?.Invoke() ?? this._selectedColor;
        Rectangle Rect = new(UIManager.DefaultIconButtonSprite.Width / 2 - 5, UIManager.DefaultIconButtonSprite.Height / 2 - 5, 10, 10);
        //ColorPickerBoxNew Picker = new();
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
            //this.Picker.Label.Text = $"Choose {cc.Name} color";

            //var oldColor = cc.Color;
            //this.Picker.CancelAction = () => cc.Color = oldColor;
            //this.Picker.SelectColor(btn.SelectedColor);
            //this.Picker.Callback = c => (btn.Tag as CharacterColor).Color = c;
            //this.Picker.ColorChangedFunc = c => cc.Color = c;

            //if (!pickerPanel.IsOpen)
            //    pickerPanel.SetLocation(btn.ScreenLocation + Vector2.UnitX * btn.Width).Show();
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

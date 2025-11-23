using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class IconButton : ButtonBase
    {
        static public readonly Texture2D Small = UIManager.Icon16Background;
        static public readonly Texture2D Large = UIManager.DefaultIconButtonSprite;
        protected virtual void OnIconChanged() { }

        readonly string Character;
        protected Icon _Icon;
        public Icon Icon
        {
            get { return _Icon ?? this.IconStack.FirstOrDefault(); }
            set
            {
                _Icon = value;
                OnIconChanged();
                this.Invalidate();
            }
        }

        readonly List<Icon> IconStack = new();
        public IconButton AddOverlay(Icon icon)
        {
            this.IconStack.Add(icon);
            return this;
        }
        public override int Width
        {
            get
            {
                if (this.BackgroundTexture == null)
                    return Icon.SourceRect.Width;
                if (Icon == null)
                    return this.BackgroundTexture.Width;
                return Math.Max(this.BackgroundTexture.Width, Icon.SourceRect.Width);
            }
            set
            {
            }
        }

        public override int Height
        {
            get
            {
                if (this.BackgroundTexture == null)
                    return Icon.SourceRect.Height;
                if (Icon == null)
                    return this.BackgroundTexture.Height;
                return Math.Max(this.BackgroundTexture.Height, Icon.SourceRect.Height);
            }
            set
            {
            }
        }

        Action _PaintAction = () => { };
        public Action PaintAction
        {
            get { return _PaintAction; }
            set
            {
                _PaintAction = value;
                this.Invalidate();
            }
        }
        public override void OnPaint(SpriteBatch sb)
        {
            if (this.BackgroundTexture != null)
                DrawSprite(sb, new Rectangle(0, 0, Width, Height), null, Color, (MouseHover || IsPressed) ? 1 : 0.5f);
            if (!string.IsNullOrEmpty(this.Character))
            {
                UIManager.DrawStringOutlined(sb, this.Character.ToString(), this.Center, new Vector2(.5f), this.Font);
                return;
            }
            if (this.IconStack.Any())
                foreach (var ic in this.IconStack)
                    ic.Draw(sb, this.Dimensions * 0.5f, new Vector2(0.5f));
            else if (this.Icon != null)
                this.Icon.Draw(sb, this.Dimensions * 0.5f, new Vector2(0.5f));
        }

        public override void OnAfterPaint(SpriteBatch sb)
        {
            base.OnAfterPaint(sb);
            this.PaintAction();
        }
        public IconButton(string character)
            : this()
        {
            this.Character = character;
        }
        public IconButton(char character, Action action = null) : this(character.ToString())
        {
            this.Font = UIManager.Symbols;
            this.LeftClickAction = action ?? this.LeftClickAction;
        }
        public IconButton(Icon icon) : this()
        {
            this.Icon = icon;
        }
        public IconButton(Icon icon, Action action) : this()
        {
            this.Icon = icon;
            this.LeftClickAction = action;
        }
        public IconButton(Texture2D backgroundText) : this() { BackgroundTexture = backgroundText; }
        public IconButton() : this(Vector2.Zero)
        {
            this.Color = Color.White * .5f;
        }
        public IconButton(Vector2 location) : base(location)
        {
            BackgroundTexture = UIManager.DefaultIconButtonSprite;
        }

        public IconButton(params Icon[] icons) : this()
        {
            //this.IconStack = icons;
            foreach (var i in icons)
                this.IconStack.Add(i);
        }
        public override Vector2 ScreenLocation { get => base.ScreenLocation + new Vector2(0, (Pressed ? 1 : 0)); }
        public override void Validate(bool cascade = false)
        {
            base.Validate(cascade);
        }
        public IconButton AddLabel(string text)
        {
            this.AddControls(new Label(text) { Location = this.BottomCenter, Anchor = new Vector2(.5f, 1), MouseThrough = true });
            return this;
        }
        public override void DrawSprite(SpriteBatch sb, Rectangle destRect, Rectangle? sourceRect, Color color, float opacity)
        {
            sb.Draw(this.BackgroundTexture, destRect, sourceRect, color * opacity, 0, Vector2.Zero, SpriteEffects.None, Depth);
        }
        static public IconButton CreateCloseButton()
        {
            return new IconButton(Icon.Cross) { BackgroundTexture = UIManager.Icon16Background };
        }
        public static IconButton CreateCloseButtonNew()
        {
            var icon = new IconButton(Icon.Cross) { BackgroundTexture = UIManager.Icon16Background };
            icon.LocationFunc = () => new(icon.Parent.Width - icon.Width - UIManager.BorderPx - icon.Parent.ClientLocation.X, UIManager.BorderPx - icon.Parent.ClientLocation.Y);
            icon.LeftClickAction = () => icon.Parent.Hide();
            return icon;
        }
        static public IconButton CreateRandomizeButton(Action action = null)
        {
            return new IconButton(Icon.Dice)
            {
                BackgroundTexture = UIManager.Icon16Background,
                HoverText = "Randomize",
                LeftClickAction = action ?? (() => { })
            };
        }
        static public IconButton CreateSmall(Icon icon, Action leftClickAction, string hoverText = "")
        {
            return new IconButton(icon)
            {
                BackgroundTexture = UIManager.Icon16Background,
                HoverText = hoverText,
                LeftClickAction = leftClickAction ?? (() => { })
            };
        }
        static public IconButton CreateSmall(char symbol, Action leftClickAction, string hoverText = "")
        {
            return new IconButton(symbol)
            {
                Font = UIManager.Font,
                BackgroundTexture = UIManager.Icon16Background,
                HoverText = hoverText,
                LeftClickAction = leftClickAction ?? (() => { })
            };
        }
    }
}

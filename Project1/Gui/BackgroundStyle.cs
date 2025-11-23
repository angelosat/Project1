using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    /// <summary>
    /// load these externally
    /// </summary>
    public class BackgroundStyle
    {
        public string Name;
        public Rectangle
                TopLeft,
                TopRight,
                BottomLeft,
                BottomRight,
                Top,
                Left,
                Right,
                Bottom,
                Center;
        public Color Color;
        public Texture2D SpriteSheet;
        public int Border;

        public override string ToString()
        {
            return Name;
        }

        public static readonly BackgroundStyle Window = new BackgroundStyle()
        {
            TopLeft = new Rectangle(0, 0, 11, 11),
            TopRight = new Rectangle(12, 0, 11, 11),
            BottomLeft = new Rectangle(0, 12, 11, 11),
            BottomRight = new Rectangle(12, 12, 11, 11),
            Top = new Rectangle(12, 0, 1, 11),
            Left = new Rectangle(0, 12, 11, 1),
            Right = new Rectangle(12, 11, 11, 1),
            Bottom = new Rectangle(11, 12, 1, 11),
            Center = new Rectangle(11, 11, 1, 1),
            Color = Color.White,
            SpriteSheet = UIManager.FrameSprite,
            Border = 11,// 5;// 11;
            Name = "Window"
        };
        public static readonly BackgroundStyle PanelNew = new BackgroundStyle()
        {
            TopLeft = new Rectangle(0, 0, 11, 11),
            TopRight = new Rectangle(12, 0, 11, 11),
            BottomLeft = new Rectangle(0, 12, 11, 11),
            BottomRight = new Rectangle(12, 12, 11, 11),
            Top = new Rectangle(12, 0, 1, 11),
            Left = new Rectangle(0, 12, 11, 1),
            Right = new Rectangle(12, 11, 11, 1),
            Bottom = new Rectangle(11, 12, 1, 11),
            Center = new Rectangle(11, 11, 1, 1),
            Color = Color.White,
            SpriteSheet = UIManager.FrameSprite,
            Border = 5,// 5;// 11;
            Name = "PanelNew"
        };
        public static BackgroundStyle Tooltip = new BackgroundStyle()
        {
            TopLeft = new Rectangle(0, 0, 11, 11),
            TopRight = new Rectangle(12, 0, 11, 11),
            BottomLeft = new Rectangle(0, 12, 11, 11),
            BottomRight = new Rectangle(12, 12, 11, 11),
            Top = new Rectangle(12, 0, 1, 11),
            Left = new Rectangle(0, 12, 11, 1),
            Right = new Rectangle(12, 11, 11, 1),
            Bottom = new Rectangle(11, 12, 1, 11),
            Center = new Rectangle(11, 11, 1, 1),
            Color = Color.Black,
            SpriteSheet = UIManager.FrameSprite,
            Border = 11,
            Name = "Tooltip"
        };

        public static BackgroundStyle LargeButton = new BackgroundStyle()
        {
            Left = new Rectangle(0, 0, 6, 45),
            Right = new Rectangle(7, 0, 6, 45),
            BottomLeft = new Rectangle(0, 0, 0, 0),
            BottomRight = new Rectangle(0, 0, 0, 0),
            Top = new Rectangle(0, 0, 0, 0),
            TopLeft = new Rectangle(0, 0, 0, 0),
            TopRight = new Rectangle(0, 0, 0, 0),
            Bottom = new Rectangle(0, 0, 0, 0),
            Center = new Rectangle(6, 0, 1, 45),
            Color = Color.White,
            SpriteSheet = UIManager.LargeButton,
            Name = "LargeButton"
        };

        public static BackgroundStyle ButtonMedium = new BackgroundStyle()
        {
            Left = new(0, 0, 4, 23),
            Right = new(5, 0, 4, 23),
            BottomLeft = new Rectangle(0, 0, 0, 0),
            BottomRight = new Rectangle(0, 0, 0, 0),
            Top = new Rectangle(0, 0, 0, 0),
            TopLeft = new Rectangle(0, 0, 0, 0),
            TopRight = new Rectangle(0, 0, 0, 0),
            Bottom = new Rectangle(0, 0, 0, 0),
            Center = new(4, 0, 1, 23),
            Color = Color.White,
            SpriteSheet = UIManager.DefaultButtonSprite,
            Name = "ButtonMedium"
        };

        public static readonly BackgroundStyle Panel =  new BackgroundStyle()
        {
                TopLeft = new Rectangle(0, 0, 19, 19),
                TopRight = new Rectangle(19, 0, 19, 19),
                BottomLeft = new Rectangle(0, 19, 19, 19),
                BottomRight = new Rectangle(19, 19, 19, 19),
                Top = new Rectangle(19, 0, 1, 19),
                Left = new Rectangle(0, 19, 19, 1),
                Right = new Rectangle(19, 19, 19, 1),
                Bottom = new Rectangle(19, 19, 1, 19),
                Center = new Rectangle(18, 18, 1, 1),
                Color = Color.White,
                SpriteSheet = UIManager.SlotSprite,
                Border = 5,
                Name = "Panel"
        };

        const int TickBoxCorner = 11;
        public static readonly BackgroundStyle TickBox = new BackgroundStyle()
        {
            Border = 5,
            TopLeft = new Rectangle(0, 0, TickBoxCorner, TickBoxCorner),
            TopRight = new Rectangle(TickBoxCorner, 0, TickBoxCorner, TickBoxCorner),
            BottomLeft = new Rectangle(0, TickBoxCorner, TickBoxCorner, TickBoxCorner),
            BottomRight = new Rectangle(TickBoxCorner, TickBoxCorner, TickBoxCorner, TickBoxCorner),
            Top = new Rectangle(TickBoxCorner, 0, 1, TickBoxCorner),
            Left = new Rectangle(0, TickBoxCorner, TickBoxCorner, 1),
            Right = new Rectangle(TickBoxCorner, TickBoxCorner, TickBoxCorner, 1),
            Bottom = new Rectangle(TickBoxCorner, TickBoxCorner, 1, TickBoxCorner),
            Center = new Rectangle(TickBoxCorner - 1, TickBoxCorner - 1, 1, 1),
            Color = Color.White,
            SpriteSheet = UIManager.TextureTickBox,
            Name = "TickBox"
        };

        public void Draw(SpriteBatch sb, int width, int height, Color color)
        {
            sb.Draw(SpriteSheet, new Vector2(0), TopLeft, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Vector2(width - 11, 0), TopRight, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Vector2(0, height - 11), BottomLeft, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Vector2(width - 11, height - 11), BottomRight, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);

            //top, left, right, bottom
            sb.Draw(SpriteSheet, new Rectangle(11, 0, width - 22, 11), Top, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Rectangle(0, 11, 11, height - 22), Left, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Rectangle(width - 11, 11, 11, height - 22), Right, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Rectangle(11, height - 11, width - 22, 11), Bottom, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);

            //center
            sb.Draw(SpriteSheet, new Rectangle(11, 11, width - 22, height - 22), Center, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
        }

        public void Draw(SpriteBatch sb, Rectangle bounds, Color color, SpriteEffects sprFx = SpriteEffects.None)
        {
            sb.Draw(SpriteSheet, new Vector2(bounds.X, bounds.Y), TopLeft, color, 0, new Vector2(0, 0), 1, sprFx, 0);//.01f);
            sb.Draw(SpriteSheet, new Vector2(bounds.X + bounds.Width - TopRight.Width, bounds.Y), TopRight, color, 0, new Vector2(0, 0), 1, sprFx, 0);// 0.01f);
            sb.Draw(SpriteSheet, new Vector2(bounds.X, bounds.Y + bounds.Height - BottomLeft.Height), BottomLeft, color, 0, new Vector2(0, 0), 1, sprFx, 0);// 0.01f);
            sb.Draw(SpriteSheet, new Vector2(bounds.X + bounds.Width - BottomRight.Height, bounds.Y + bounds.Height - BottomLeft.Height), BottomRight, color, 0, new Vector2(0, 0), 1, sprFx, 0);// 0.01f);

            //top, left, right, bottom
            sb.Draw(SpriteSheet, new Rectangle(bounds.X + Left.Width, bounds.Y, bounds.Width - Left.Width - Right.Width, Top.Height), Top, color, 0, new Vector2(0, 0), sprFx, 0);// 0.01f);
            sb.Draw(SpriteSheet, new Rectangle(bounds.X, bounds.Y + Top.Height, Left.Width, bounds.Height - Top.Height - Bottom.Height), Left, color, 0, new Vector2(0, 0), sprFx, 0);// 0.01f);
            sb.Draw(SpriteSheet, new Rectangle(bounds.X + bounds.Width - Right.Width, bounds.Y + Top.Height, Right.Width, bounds.Height - Top.Height - Bottom.Height), Right, color, 0, new Vector2(0, 0), sprFx, 0);// 0.01f);
            sb.Draw(SpriteSheet, new Rectangle(bounds.X + Left.Width, bounds.Y + bounds.Height - Bottom.Height, bounds.Width - Left.Width - Right.Width, Bottom.Height), Bottom, color, 0, new Vector2(0, 0), sprFx, 0);// 0.01f);

            //center
            sb.Draw(SpriteSheet, new Rectangle(bounds.X + Left.Width, bounds.Y + Top.Height, bounds.Width - Left.Width - Right.Width, bounds.Height - Top.Height - Bottom.Height), Center, color, 0, new Vector2(0, 0), sprFx, 0);// 0.01f);
        }

        public void Draw(SpriteBatch sb, Rectangle bounds, float opacity = 1)
        {
            Color color = Color.Lerp(Color.Transparent, Color, opacity);

            sb.Draw(SpriteSheet, new Vector2(bounds.X, bounds.Y), TopLeft, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Vector2(bounds.X + bounds.Width - TopRight.Width, bounds.Y), TopRight, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Vector2(bounds.X, bounds.Y + bounds.Height - BottomLeft.Height), BottomLeft, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Vector2(bounds.X + bounds.Width - BottomRight.Height, bounds.Y + bounds.Height - BottomLeft.Height), BottomRight, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);

            //top, left, right, bottom
            sb.Draw(SpriteSheet, new Rectangle(bounds.X + Left.Width, bounds.Y, bounds.Width - Left.Width - Right.Width, Top.Height), Top, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Rectangle(bounds.X, bounds.Y + Top.Height, Left.Width, bounds.Height - Top.Height - Bottom.Height), Left, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Rectangle(bounds.X + bounds.Width - Right.Width, bounds.Y + Top.Height, Right.Width, bounds.Height - Top.Height - Bottom.Height), Right, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Rectangle(bounds.X + Left.Width, bounds.Y + bounds.Height - Bottom.Height, bounds.Width - Left.Width - Right.Width, Bottom.Height), Bottom, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);

            //center
            sb.Draw(SpriteSheet, new Rectangle(bounds.X + Left.Width, bounds.Y + Top.Height, bounds.Width - Left.Width - Right.Width, bounds.Height - Top.Height - Bottom.Height), Center, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
        }

        public void Draw(SpriteBatch sb, Rectangle bounds, Color tint, float opacity)
        {
            Color color = Color.Lerp(Color.Transparent, Color.Lerp(Color , tint,0.1f), opacity);

            sb.Draw(SpriteSheet, new Vector2(bounds.X, bounds.Y), TopLeft, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Vector2(bounds.X + bounds.Width - TopRight.Width, bounds.Y), TopRight, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Vector2(bounds.X, bounds.Y + bounds.Height - BottomLeft.Height), BottomLeft, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Vector2(bounds.X + bounds.Width - BottomRight.Height, bounds.Y + bounds.Height - BottomLeft.Height), BottomRight, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);

            //top, left, right, bottom
            sb.Draw(SpriteSheet, new Rectangle(bounds.X + Left.Width, bounds.Y, bounds.Width - Left.Width - Right.Width, Top.Height), Top, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Rectangle(bounds.X, bounds.Y + Top.Height, Left.Width, bounds.Height - Top.Height - Bottom.Height), Left, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Rectangle(bounds.X + bounds.Width - Right.Width, bounds.Y + Top.Height, Right.Width, bounds.Height - Top.Height - Bottom.Height), Right, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(SpriteSheet, new Rectangle(bounds.X + Left.Width, bounds.Y + bounds.Height - Bottom.Height, bounds.Width - Left.Width - Right.Width, Bottom.Height), Bottom, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);

            //center
            sb.Draw(SpriteSheet, new Rectangle(bounds.X + Left.Width, bounds.Y + Top.Height, bounds.Width - Left.Width - Right.Width, bounds.Height - Top.Height - Bottom.Height), Center, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
        }
    }
}

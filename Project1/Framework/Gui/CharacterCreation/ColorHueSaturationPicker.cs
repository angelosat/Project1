using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Start_a_Town_.UI
{
    class ColorHueSaturationPicker : PictureBox
    {
        static readonly Color[] Colors = new Color[360 * 256];

        static readonly Texture2D ColorWheel = CreateTexture();

        bool PressedNew;
        public Color Selected;
        Vector2 SelectedCoords;

        static Texture2D CreateTexture()
        {
            for (int v = 0; v < 256; v++)
                for (int h = 0; h < 360; h++)
                {
                    Color color = HSVtoRGB(h, 1 - v / 256f, 1f);
                    Colors[v * 360 + h] = color;
                }
            var texture = new Texture2D(Game1.Instance.GraphicsDevice, 360, 256);
            texture.SetData(Colors);
            return texture;
        }

        public void SelectColor(Color rgb)
        {
            float h, s, v;
            h = s = v = 0;
            RGBtoHSV(rgb, ref h, ref s, ref v);
            var pos = new Vector2(h, (1 - s) * 255);
            this.Selected = Colors[(int)pos.Y * ColorWheel.Width + (int)pos.X];
            this.SelectedCoords = pos;
        }

        public Action<Color> Callback;

        public ColorHueSaturationPicker()
            : base(Vector2.Zero, ColorWheel, ColorWheel.Bounds)
        {
            this.Callback = c => { };
        }

        public override void Update()
        {
            base.Update();
            if (!this.PressedNew)
                return;
            //if (!this.MouseHover)
            //    return;
            var pos = UIManager.Mouse - this.ScreenLocation;
            var x = (int)Math.Max(0, Math.Min(ColorWheel.Width - 1, pos.X));
            var y = (int)Math.Max(0, Math.Min(ColorWheel.Height - 1, pos.Y));

            this.Selected = Colors[y * ColorWheel.Width + x];
            this.SelectedCoords = new Vector2(x, y);
            this.Callback(this.Selected);
        }

        public override void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (!this.MouseHover)
                return;
            this.PressedNew = true;
        }
        public override void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.PressedNew = false;
        }
        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
            var rect = new Rectangle((int)this.ScreenLocation.X + (int)this.SelectedCoords.X - 1, (int)this.ScreenLocation.Y + (int)this.SelectedCoords.Y - 1, 3, 3);
            rect.DrawHighlightBorder(sb, Color.Black, Vector2.Zero, 0);
        }

        public static void RGBtoHSV(Color rgb, ref float h, ref float s, ref float v)
        {
            float r = rgb.R, g = rgb.G, b = rgb.B;
            RGBtoHSV(r, g, b, ref h, ref s, ref v);
        }
        public static Color HSVtoRGB(float h, float s, float v)
        {
            float r, g, b;
            r = g = b = 0;
            HSVtoRGB(ref r, ref g, ref b, h, s, v);
            return new Color(r, g, b);
        }

        /// <summary>
        /// https://www.programmingalgorithms.com/algorithm/rgb-to-hsv/
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="h"></param>
        /// <param name="s"></param>
        /// <param name="v"></param>
        public static void RGBtoHSV(float r, float g, float b, ref float h, ref float s, ref float v)
        {
            float delta, min;
            //double h = 0, s, v;
            h = 0;
            min = Math.Min(Math.Min(r, g), b);
            v = Math.Max(Math.Max(r, g), b);
            delta = v - min;

            if (v == 0.0)
                s = 0;
            else
                s = delta / v;

            if (s == 0)
                h = 0f;

            else
            {
                if (r == v)
                    h = (g - b) / delta;
                else if (g == v)
                    h = 2 + (b - r) / delta;
                else if (b == v)
                    h = 4 + (r - g) / delta;

                h *= 60;

                if (h < 0.0)
                    h += 360;
            }
        }
        public static void HSVtoRGB(ref float r, ref float g, ref float b, float h, float s, float v)
        {
            int i;
            float f, p, q, t;
            if (s == 0)
            {
                // achromatic (grey)
                r = g = b = v;
                return;
            }
            h /= 60;            // sector 0 to 5
            i = (int)Math.Floor(h);
            f = h - i;          // factorial part of h
            p = v * (1 - s);
            q = v * (1 - s * f);
            t = v * (1 - s * (1 - f));
            switch (i)
            {
                case 0:
                    r = v;
                    g = t;
                    b = p;
                    break;
                case 1:
                    r = q;
                    g = v;
                    b = p;
                    break;
                case 2:
                    r = p;
                    g = v;
                    b = t;
                    break;
                case 3:
                    r = p;
                    g = q;
                    b = v;
                    break;
                case 4:
                    r = t;
                    g = p;
                    b = v;
                    break;
                default:        // case 5:
                    r = v;
                    g = p;
                    b = q;
                    break;
            }
        }
    }
}

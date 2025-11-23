using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.UI
{
    public class Label : ButtonBase
    {
        public static int DefaultHeight = UIManager.Font.LineSpacing + 2;
        //public static int DefaultHeight = 15;

        Alignment.Horizontal _Halign;
        public Alignment.Horizontal Halign
        {
            get => this._Halign;
            set => this._Halign = value;
        }
        public Alignment.Vertical Valign;
        public Color TextBackground = Color.Transparent;
        public Func<Color> TextBackgroundFunc = () => Color.Transparent;

        public override string ToString()
        {
            return "Label: " + this.Text ?? this.Name;
        }

        public override int Width
        {
            get => base.Width;
            set
            {
                base.Width = value;
                if (value > 0)
                    this.Invalidate();
            }
        }

        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            var c = this.TextBackgroundFunc();
            this.BoundsScreen.DrawHighlight(sb, c);
            base.Draw(sb, viewport);
        }

        public Color ActiveColor = Color.Lime;
        public override void OnPaint(SpriteBatch sb)
        {
            var pos = new Vector2((int)this.Halign * .5f, .5f);
            var outlineOffset = this.Halign == Alignment.Horizontal.Left ? 1 : (this.Halign == Alignment.Horizontal.Right ? -1 : 0);
            UIManager.DrawStringOutlined(
                sb,
                this.Text,
                pos * this.Dimensions + new Vector2(outlineOffset, 0) + ((this.Active && this.Pressed) ? Vector2.UnitY : Vector2.Zero),
                pos,
                //(this.MouseHover && this.Active) ? this.ActiveColor : this.TextColor,
                ((this.IsPushed || this.MouseHover) && this.Active) ? this.ActiveColor : this.TextColor,
                this.TextOutline,
                this.Font);
        }


        protected override void OnTextChanged()
        {
            base.OnTextChanged();
            this.Height = Math.Max(this.Height, Label.DefaultHeight);

            switch (this.Halign)
            {
                case Alignment.Horizontal.Left:
                    this.Origin.X = 0;
                    break;
                case Alignment.Horizontal.Center:
                    this.Origin.X = this.Width / 2;
                    break;
                case Alignment.Horizontal.Right:
                    this.Origin.X = this.Width;
                    break;
                default:
                    break;
            }
            switch (this.Valign)
            {
                case Alignment.Vertical.Top:
                    this.Origin.Y = 0;
                    break;
                case Alignment.Vertical.Center:
                    this.Origin.Y = this.Height / 2;
                    break;
                case Alignment.Vertical.Bottom:
                    this.Origin.Y = this.Height;
                    break;
                default:
                    break;
            }
        }
        public Label(int width) : base()
        {
            this.Text = "";
            this.Width = width;
            this.Height = Label.DefaultHeight;
            this.Active = false;
        }
        public Label() : this(Vector2.Zero, "") { }
        public Label(Func<string> textFunc) : this(Vector2.Zero)
        {
            this.AutoSize = true; // TESTING PUTTING THIS HERE. if there are any problems, this here could be the case
            this.TextFunc = textFunc;
        }
        
        public Label(string text) : this(Vector2.Zero, text) { }
        public Label(object obj)
        {
            if (obj is not null)
            {
                if (obj is Inspectable objdetails)
                {
                    this.Active = true;
                    this.LeftClickAction = () => Inspector.Refresh(objdetails);
                    this.Font = UIManager.FontBold;
                    this.Text = $"[{objdetails.Label}]";
                    this.TextColor = Color.LightBlue;
                }
                else
                {
                    this.Active = false;
                    this.Text = obj?.ToString() ?? "null";
                }
                this.HoverText = this.Text;
            }
        }
     
        public Label(object obj, Action action) : this(Vector2.Zero, obj.ToString())
        {
            this.Active = true;
            this.LeftClickAction = action;
        }
        public Label(Func<string> textGetter, Action action) : this(textGetter)
        {
            this.Active = true;
            this.LeftClickAction = action;
        }
        public Label(Vector2 location, string text = "", Color? fill = null, Color? outline = null, SpriteFont font = null)// System.Drawing.FontStyle style = System.Drawing.FontStyle.Regular)
            : base(location)
        {
            if (fill.HasValue)
                this.Fill = fill.Value;
            if (outline.HasValue)
                this.TextOutline = outline.Value;
            this.Text = text;
            this.Font = font ?? UIManager.Font;
            this.Active = false;
        }
        public Label(string text = "", Color? fill = null, Color? outline = null, SpriteFont font = null)// System.Drawing.FontStyle style = System.Drawing.FontStyle.Regular)
            : base()
        {
            if (fill.HasValue)
                this.Fill = fill.Value;
            if (outline.HasValue)
                this.TextOutline = outline.Value;
            this.Text = text;
            this.Font = font ?? UIManager.Font;
            this.Active = false;
        }
        public Label(Vector2 location, string text, Alignment.Horizontal halign)
            : base(location)
        {
            this.Halign = halign;
            this.Text = text;
        }
        public Label(Vector2 location, string text, Alignment.Horizontal halign, Alignment.Vertical valign)
            : base(location)
        {
            this.Halign = halign;
            this.Valign = valign;
            this.Text = text;
            switch (halign)
            {
                case Alignment.Horizontal.Left:
                    this.Origin.X = 0;
                    break;
                case Alignment.Horizontal.Center:
                    this.Origin.X = this.Width / 2;
                    break;
                case Alignment.Horizontal.Right:
                    this.Origin.X = this.Width;
                    break;
                default:
                    break;
            }
            switch (valign)
            {
                case Alignment.Vertical.Top:
                    this.Origin.Y = 0;
                    break;
                case Alignment.Vertical.Center:
                    this.Origin.Y = this.Height / 2;
                    break;
                case Alignment.Vertical.Bottom:
                    this.Origin.Y = this.Height;
                    break;
                default:
                    break;
            }
            this.Location -= this.Origin;
        }

        public Label(string text, string format)
            : this(text)
        {
            this.Text = text;
            this.TextFormat = format ?? text;
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
            this.DrawText(sb, this.ScreenLocation, null, this.MouseHover && this.Active ? Color.Lime : Color.White, this.Opacity); //Color.White
        }
        public static void DrawText(SpriteBatch sb, Texture2D textSprite, Vector2 position, int width, Color color, float opacity, Alignment.Horizontal hAlign)
        {
            var c = color * opacity;
            // TODO: fix the origin so it's not always at the center
            Vector2 offset = new(1, 1), origin = Vector2.Zero;
            switch (hAlign)
            {
                case Alignment.Horizontal.Left:
                    offset = new Vector2(1, 1);
                    origin = Vector2.Zero;
                    break;
                case Alignment.Horizontal.Center:
                    offset = new Vector2((float)Math.Floor(width * 0.5), 1);
                    origin = new Vector2(textSprite.Width / 2, 0);
                    break;
                case Alignment.Horizontal.Right:
                    offset = new Vector2(width - 2, 1);
                    origin = new Vector2(textSprite.Width, 0);
                    break;
                default:
                    break;
            }

            sb.Draw(textSprite, position + offset, null, c, 0, origin, 1, SpriteEffects.None, 0);
        }
        public static void DrawText(SpriteBatch sb, Texture2D textSprite, Vector2 position, Rectangle? sourceRect, int width, Color color, float opacity, Alignment.Horizontal hAlign)
        {
            var c = color * opacity;

            if (!sourceRect.HasValue)
                sourceRect = textSprite.Bounds;
            // TODO: fix the origin so it's not always at the center

            Vector2 offset = new(1, 1), origin = Vector2.Zero;
            switch (hAlign)
            {
                case Alignment.Horizontal.Left:
                    offset = new Vector2(1, 1);
                    origin = Vector2.Zero;
                    break;
                case Alignment.Horizontal.Center:
                    offset = new Vector2((float)Math.Floor(width * 0.5), 1);
                    origin = new Vector2(textSprite.Width / 2, 0);
                    break;
                case Alignment.Horizontal.Right:
                    offset = new Vector2(width - 2, 1);
                    origin = new Vector2(textSprite.Width, 0);
                    break;
                default:
                    break;
            }

            sb.Draw(textSprite, position + offset, Rectangle.Intersect(textSprite.Bounds, sourceRect.Value), c, 0, origin, 1, SpriteEffects.None, 0);
        }
        public override void OnMouseEnter()
        {
            base.OnMouseEnter();
            if (this.Active) 
                this.Invalidate();
        }
        public override void OnMouseLeave()
        {
            base.OnMouseLeave();
            if (this.Active) 
                this.Invalidate();
        }

        public Alignment.Horizontal TextHAlign { get; set; }

        public override Control SetLeftClickAction(Action<ButtonBase> action)
        {
            this.Active = true;
            return base.SetLeftClickAction(action);
        }
        public static IEnumerable<Label> Parse(string text)
        {
            var array = text.Split(' ');
            for (int i = 0; i < array.Length; i++)
            {
                var txt = array[i];
                if (txt.First() == '[' && txt.Last() == ']')
                {
                    var token = txt.Substring(1, txt.Length - 2).Split(',');
                    var inner = token[0];
                    var lbl = new Label($"{inner}");
                    if (inner.Length > 1)
                        if (token[1].TryParseColor(out var col))
                            lbl.TextColor = col;
                    yield return lbl;
                }
                else
                    yield return new Label(txt);
            }
        }
        static Label ParseToken(string txt)
        {
            var token = txt.Split(',');
            var inner = token[0];
            var lbl = new Label($"{inner}") { TextColor = Color.Gold, Font = UIManager.FontBold };
            if (token.Length > 1)
            {
                if (token[1].TryParseColor(out var col))
                    lbl.TextColor = col;
            }
            return lbl;
        }
        public static IEnumerable<Label> ParseNew(string text)
        {
            var posCurrent = 0;
            int posFrom = 0;
            do
            {
                posFrom = text.IndexOf('[', posCurrent);
                if (posFrom != -1)
                {
                    var plainText = text.Substring(posCurrent, posFrom - posCurrent);
                    
                    var f = plainText.Split(' ');
                    foreach (var i in f)
                        if (!i.IsNullEmptyOrWhiteSpace())
                            yield return new Label(i);
                    var posTo = text.IndexOf(']', posFrom + 1);
                    if (posTo != -1)
                    {
                        var token = text.Substring(posFrom + 1, posTo - posFrom - 1);
                        posCurrent = posTo + 1;
                        yield return ParseToken(token);
                    }
                }
                else
                {
                    var plainText = text.Substring(posCurrent, text.Length - posCurrent);
                    foreach (var i in plainText.Split(' '))
                        if (!i.IsNullEmptyOrWhiteSpace())
                            yield return new Label(i);
                }
            } while (posFrom != -1);
        }
        internal static IEnumerable<Label> ParseNewNew(params object[] values)
        {
            return values.SelectMany(ParseNewNew);
        }
        internal static IEnumerable<Label> ParseNewNew(object value)
        {
            //if (value is string str)
            //    return ParseNew(str);
            //else if (value is Inspectable objInspectable)
            //    return new Label[] { new Label(value) };
            //else if (value is IEnumerable<string> strEnum)
            //    return strEnum.SelectMany(s => ParseNew(s));
            //else if (value is IEnumerable<object> objEnum)
            //    return objEnum.Select(o => new Label(o));
            //else if (value is Func<string> textFunc)
            //    return new Label[] { new Label(textFunc) };
            //else
            //    return new Label[] { new Label(value) };

            if (value is string str)
                foreach (var i in ParseNew(str)) yield return i;
            else if (value is Inspectable objInspectable)
                yield return new Label(value);
            else if (value is IEnumerable<string> strEnum)
                foreach (var i in strEnum.SelectMany(s => ParseNew(s))) yield return i;
            else if (value is IEnumerable<object> objEnum)
                foreach (var i in objEnum.Select(o => new Label(o))) yield return i;
            else if (value is Func<string> textFunc)
                yield return new Label(textFunc);
            else
                yield return new Label(value);
        }
        internal static GroupBox ParseWrap(int wrapWidth, params object[] values)
        {
            return new GroupBox().AddControlsLineWrap(ParseNewNew(values), wrapWidth);
        }
        internal static GroupBox ParseWrap(params object[] values)
        {
            return new GroupBox().AddControlsLineWrap(ParseNewNew(values));
        }
        public static IEnumerable<T> ParseBest<T>(string text) where T : Label, new()
        {
            var posCurrent = 0;
            int posFrom = 0;
            do
            {
                posFrom = text.IndexOf('[', posCurrent);
                if (posFrom != -1)
                {
                    var posTo = text.IndexOf(']', posFrom + 1);
                    if (posTo != -1)
                    {
                        var token = text.Substring(posFrom + 1, posTo - posFrom - 1);
                        posCurrent = posTo + 1;
                        var parsed = Token.Parse(token);
                        yield return new T() { Text = parsed.Text, TextColor = parsed.Color };
                    }
                }
                else
                {
                    var plainText = text.Substring(posCurrent, text.Length - posCurrent);
                    foreach (var i in plainText.Split(' '))
                        if (!i.IsNullEmptyOrWhiteSpace())
                            yield return new T() { Text = i };// new Label(i);
                }
            } while (posFrom != -1);
        }
        public static IEnumerable<Label> ParseBest(string text)
        {
            var posCurrent = 0;
            int posFrom = 0;
            do
            {
                posFrom = text.IndexOf('[', posCurrent);
                if (posFrom != -1)
                {
                    var posTo = text.IndexOf(']', posFrom + 1);
                    if (posTo != -1)
                    {
                        var token = text.Substring(posFrom + 1, posTo - posFrom - 1);
                        posCurrent = posTo + 1;
                        yield return Token.Parse(token).GetLabel();
                    }
                }
                else
                {
                    var plainText = text.Substring(posCurrent, text.Length - posCurrent);
                    foreach (var i in plainText.Split(' '))
                        if (!i.IsNullEmptyOrWhiteSpace())
                            yield return new Label(i);
                }
            } while (posFrom != -1);
        }

        struct Token
        {
            public string Text;
            public Color Color;

            public Label GetLabel()
            {
                return new Label(this.Text) { TextColor = this.Color };
            }

            public Token(string value, IEnumerable<(string, string)> atts) : this()
            {
                var dic = atts.ToDictionary(i => i.Item1, i => i.Item2);
                this.Text = value.ToString();
                if (dic.TryGetValue("color", out var item))
                    ColorHelper.TryParseColor(item, out this.Color);
                else
                    this.Color = UIManager.DefaultTextColor;
            }

            Token(IEnumerable<(string, string)> atts) : this()
            {
                var dic = atts.ToDictionary(i => i.Item1, i => i.Item2);

                if (dic.TryGetValue("text", out var text))
                    this.Text = text.ToString();
                else
                    this.Text = "";

                if (dic.TryGetValue("color", out var col))
                    ColorHelper.TryParseColor(col, out this.Color);
                else
                    this.Color = UIManager.DefaultTextColor;
            }

            public static Token Parse(string text)
            {
                var originText = text;
                List<(string, string)> atts = new();
                do
                {
                    text = text.TrimStart(' ');
                    var nextEqualSignIndex = text.IndexOf('=');
                    var key = text.Substring(0, nextEqualSignIndex);
                    text = text.Substring(nextEqualSignIndex + 1, text.Length - nextEqualSignIndex - 1);
                    var quoteMarksBeginIndex = text.IndexOf('\'', 0);
                    text = text.Substring(quoteMarksBeginIndex + 1, text.Length - quoteMarksBeginIndex - 1);
                    var quoteMarksEndIndex = text.IndexOf('\'', 0);
                    var inner = text.Substring(0, quoteMarksEndIndex);
                    text = text.Substring(quoteMarksEndIndex + 1, text.Length - quoteMarksEndIndex - 1);
                    atts.Add((key, inner));
                } while (text.Length > 0);// (posFrom != -1);
                return new(atts);
            }
        }
    }
}

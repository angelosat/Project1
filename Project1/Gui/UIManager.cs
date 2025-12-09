using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Start_a_Town_.UI
{
    public class UIManager : IDisposable, IKeyEventHandler
    {
        static float _scale = 1;
        public static float Scale
        {
            get => _scale;
            set
            {
                if (_scale == value)
                    return;
                foreach (var manager in WindowManagers)
                    //    foreach (var layer in manager.Layers)
                    //        foreach (var ctrl in layer.Value)
                    foreach (var ctrl in manager.ControlsInMemory)
                        ctrl.OnUIScaleChanged(_scale, value);
                _scale = value;
                foreach (var manager in WindowManagers)
                    foreach (var ctrl in manager.ControlsInMemory)
                        ctrl.OnUIScaleChanged();
                UITexture = new RenderTarget2D(Game1.Instance.GraphicsDevice, Width, Height);
            }
        }
        public static int FlashingTimer { get; private set; }

        public static float FlashingValue
        {
            get
            {
                var lerp = (float)Math.Cos(Math.PI * 2 * FlashingTimer / 120f);
                lerp += 1;
                lerp /= 2;
                return lerp;
            }
        }
        public readonly DialogBlock DialogBlock = new();

        public static Rectangle Bounds => new(0, 0, Width, Height);
        public static Vector2 Center => new Vector2(Game1.Bounds.Width, Game1.Bounds.Height) / (2 * Scale);
        
        public static int Width => (int)(Game1.Bounds.Width / Scale);
        public static int Height => (int)(Game1.Bounds.Height / Scale);

        public static Vector2 Size => new(Width, Height);
        public static Vector2 Mouse => Controller.Instance.MouseLocation;// / Scale;
        public static Vector2 MouseTrue => Controller.Instance.MouseLocation / Scale;
        public static Rectangle MouseRect => new((int)(Mouse.X / Scale), (int)(Mouse.Y / Scale), 1, 1);
        public static SortedList<float, Control> MouseOverList;
        public static SpriteFont Font = Game1.Instance.Content.Load<SpriteFont>("DefaultFont");
        public static SpriteFont FontBold = Game1.Instance.Content.Load<SpriteFont>("BoldFont");
        public static SpriteFont Symbols = Game1.Instance.Content.Load<SpriteFont>("Symbols");

        public static readonly Color TintPrimaryDefault = Color.SteelBlue;
        public static readonly Color TintSecondaryDefault = Color.DarkSlateGray;
        public static Color TintPrimary = TintPrimaryDefault;
        public static Color TintSecondary = TintSecondaryDefault;
        static UIManager()
        {
            TintPrimary.PackedValue = uint.TryParse(InterfaceSettings.XTintPrimary.Value, System.Globalization.NumberStyles.HexNumber, null, out uint packed) ? packed : TintPrimaryDefault.PackedValue;
            TintSecondary.PackedValue = uint.TryParse(InterfaceSettings.XTintSecondary.Value, System.Globalization.NumberStyles.HexNumber, null, out packed) ? packed : TintSecondaryDefault.PackedValue;
            /// hack to quickly test another color
            TintPrimary = new Color(20,20,20, 255);
            TintSecondary = TintPrimary;
        }

        public static Texture2D FrameSprite, SlotSprite, DefaultButtonSprite, DefaultIconButtonSprite, ItemSheet, DefaultProgressBar, DefaultProgressBarStrip, ProgressBarBorder, DefaultDropdownSprite, DefaultLoadingSprite,
            DefaultVScrollbarSprite, DefaultHScrollbarSprite,
            DefaultTrackBarSprite, DefaultTrackBarThumbSprite, SampleButton, Icons16x16, LargeButton, Icons32, Icon16Background, TextureTickBox;
        public static int DefaultButtonHeight = 23, LineHeight;
        public static int BorderPx = 5, TitlePx = 26;
        public static Color DefaultTextColor = Color.LightGray;

        public static List<UIManager> WindowManagers = new();

        public static Window MouseOverWindow;
        public Window ActiveWindow => this.ActiveControl?.GetWindow();

        public static readonly GuiLayer
            LayerNameplates = new("Nameplates"),
            LayerSpeechbubbles = new("Speechbubbles"),
            LayerHud = new("Hud"),
            LayerWindows = new("Windows"),
            LayerDialog = new("Dialog");
        readonly Dictionary<GuiLayer, List<Control>> _layers = new()
        {
            { LayerNameplates, new() },
            { LayerSpeechbubbles, new() },
            { LayerHud, new() },
            { LayerWindows, new() },
            { LayerDialog, new() }
        };
        public Dictionary<GuiLayer, List<Control>> Layers => this._layers;
        public List<Control> this[GuiLayer layer] => this.Layers[layer];

        public int LastScreenWidth, LastScreenHeight;
        public Window Dialog;

        static Texture2D _highlight;
        public static Texture2D Highlight
        {
            get
            {
                if (_highlight is null)
                {
                    _highlight = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
                    _highlight.SetData(new Color[] { Color.White });
                }
                return _highlight;
            }
        }
        static Texture2D _transparentHighlight;
        public static Texture2D TransparentHighlight
        {
            get
            {
                if (_transparentHighlight is null)
                {
                    _transparentHighlight = new Texture2D(Game1.Instance.GraphicsDevice, 3, 3);
                    _transparentHighlight.SetData(new Color[] { Color.White, Color.White, Color.White, Color.White, Color.Transparent, Color.White, Color.White, Color.White, Color.White });
                }
                return _transparentHighlight;
            }
        }

        static Texture2D _shade;
        public static Texture2D Shade
        {
            get
            {
                if (_shade is null)
                {
                    _shade = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
                    _shade.SetData(new Color[] { Color.Black });
                }
                return _shade;
            }
        }

        public static Texture2D Cursor;
        public RenderTarget2D DimScreen;

        public static readonly Atlas Atlas = new("UI");
        public static readonly Atlas.Node.Token Btn16 = Atlas.Load("Graphics/Gui/gui-icon");
        public static readonly Atlas.Node.Token ArrowUp = Atlas.Load("Graphics/Gui/gui-up1");
        public static readonly Atlas.Node.Token ArrowDown = Atlas.Load("Graphics/Gui/gui-down1");
        public static readonly Atlas.Node.Token ArrowLeft = Atlas.Load("Graphics/Gui/gui-left1");
        public static readonly Atlas.Node.Token ArrowRight = Atlas.Load("Graphics/Gui/gui-right1");
        public static readonly Atlas.Node.Token IconX = Atlas.Load("Graphics/Gui/gui-x");
        public static readonly Atlas.Node.Token IconSave = Atlas.Load("Graphics/Gui/gui-save");
        public static readonly Atlas.Node.Token IconLoad = Atlas.Load("Graphics/Gui/gui-load");
        public static readonly Atlas.Node.Token IconDisk = Atlas.Load("Graphics/Gui/gui-saveload");
        public static readonly Atlas.Node.Token Tickbox = Atlas.Load("Graphics/Gui/gui-tickbox2a");
        public static readonly Atlas.Node.Token MousePointer = Atlas.Load("Graphics/cursor-default");
        public static readonly Atlas.Node.Token MousePointerGrayscale = Atlas.Load("Graphics/cursor-default", true);
        public static readonly Atlas.Node.Token SpriteConstruction = Atlas.Load("Graphics/Icons/spr-constr");

        public static void LoadContent()
        {
            Cursor = Game1.Instance.Content.Load<Texture2D>("Graphics/cursor-default");
            var txtOutlinePixels = 1;
            LineHeight = Font.LineSpacing + txtOutlinePixels * 2;
            SlotSprite = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/gui-frame1");
            FrameSprite = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/gui-window2");
            DefaultButtonSprite = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/guiButton");
            DefaultDropdownSprite = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/ui_dropdown_strip");
            ItemSheet = Game1.Instance.Content.Load<Texture2D>("Graphics/ItemSheet");
            DefaultProgressBar = Game1.Instance.Content.Load<Texture2D>("Graphics/progressbarBW");
            DefaultProgressBarStrip = Game1.Instance.Content.Load<Texture2D>("Graphics/barBwStrip");
            ProgressBarBorder = Game1.Instance.Content.Load<Texture2D>("Graphics/progressbarBorder");
            DefaultVScrollbarSprite = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/vscrollbar");
            DefaultHScrollbarSprite = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/hscrollbar");
            DefaultTrackBarSprite = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/gui-tickbox2a");
            Icon16Background = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/gui-icon");
            DefaultTrackBarThumbSprite = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/gui-down1");
            DefaultIconButtonSprite = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/iconbutton");
            SampleButton = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/samplebutton");
            Icons16x16 = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/Icons16x16");
            Icons32 = Game1.Instance.Content.Load<Texture2D>("Graphics/IconSheet");
            LargeButton = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/LargeButton");
            TextureTickBox = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/CheckBox");
            DefaultLoadingSprite = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/spinning3");

            var scaleNode = Engine.Config.Descendants("Scale").FirstOrDefault();
            if (float.TryParse(scaleNode.Value, out float scale)) // do i need to check for scalenode == null?
                Scale = scale;
            else
                Scale = 1;
        }

        internal void OnSelectedTargetChanged(TargetArgs target)
        {
            foreach(var c in this.ControlsInMemory)
                    c.OnSelectedTargetChanged(target);
        }

        static RenderTarget2D UITexture;
        public UIManager()
        {
            WindowManagers.Add(this);

            Game1.Instance.GraphicsDevice.DeviceReset += new EventHandler<EventArgs>(this.graphics_DeviceReset);
            this.LastScreenWidth = Game1.Instance.GraphicsDevice.PresentationParameters.BackBufferWidth;
            this.LastScreenHeight = Game1.Instance.GraphicsDevice.PresentationParameters.BackBufferHeight;

            this.DimScreen = new RenderTarget2D(Game1.Instance.GraphicsDevice, 1, 1);
            Game1.Instance.GraphicsDevice.SetRenderTarget(this.DimScreen);
            Game1.Instance.GraphicsDevice.Clear(Color.Black);
            Game1.Instance.GraphicsDevice.SetRenderTarget(null);
        }

        public List<Control> ControlsInMemory = new();
        void graphics_DeviceReset(object sender, EventArgs e)
        {
            this.Reset();
        }

        private void Reset()
        {
            int newWidth = Game1.Instance.graphics.PreferredBackBufferWidth;
            int newHeight = Game1.Instance.graphics.PreferredBackBufferHeight;
            //int newWidth = Game1.ScreenSize.Width;
            //int newHeight = Game1.ScreenSize.Height;
            float wRatio = newWidth / (float)this.LastScreenWidth;
            float hRatio = newHeight / (float)this.LastScreenHeight;

            this.LastScreenWidth = newWidth;
            this.LastScreenHeight = newHeight;

            UITexture = new RenderTarget2D(Game1.Instance.GraphicsDevice, newWidth, newHeight);
            var t = Game1.Instance;
            foreach (Control ctrl in this.ControlsInMemory)
            {
                ctrl.Reposition(new Vector2(wRatio, hRatio));
                ctrl.OnResolutionChanged();
                ctrl.Invalidate(true);
            }

            this.DimScreen = new RenderTarget2D(Game1.Instance.GraphicsDevice, 1, 1);
            Game1.Instance.GraphicsDevice.SetRenderTarget(this.DimScreen);
            Game1.Instance.GraphicsDevice.Clear(Color.Black);
            Game1.Instance.GraphicsDevice.SetRenderTarget(null);
        }

        public Control FocusedControl;
        internal static Color DefaultListItemBackgroundColor = Color.SlateGray * .2f;

        public Control ActiveControl
        {
            get { return Controller.Instance.Mouseover.Object as Control; }
            set { Controller.Instance.Mouseover.Object = value; }
        }

        public IntVec2 TopCenter => new(Width / 2, 0);
        public IntVec2 TopRight => new(Width, 0);

        public IntVec2 CenterLeft => new(0, Height / 2);
        public IntVec2 CenterCenter => new(Width / 2, Height / 2);
        public IntVec2 CenterRight => new(Width, Height / 2);

        public IntVec2 BottomLeft => new(0, Height);
        public IntVec2 BottomCenter => new(Width / 2, Height);
        public IntVec2 BottomRight => new(Width, Height);

        public void Screen_MouseMove()
        {
            MouseOverWindow = null;
        }

        public void Update(Game1 game, GameTime gt)
        {
            FlashingTimer++;
            Control lastactive = Controller.Instance.Mouseover.Object as Control;// ActiveControl;
            foreach (var layer in this.Layers)
            {
                foreach (var ctrl in layer.Value.ToList())
                {
                    ctrl.Update();
                    ctrl.Update(new Rectangle(0, 0, Game1.Bounds.Width, Game1.Bounds.Height));
                }
            }

            Control nextactive = Controller.Instance.MouseoverNext.Object as Control;

            if (nextactive != lastactive)
            {
                nextactive?.OnMouseEnter();
                lastactive?.OnMouseLeave();
            }
        }
        public bool CloseAll(GuiLayer layer)
        {
            bool closedsomething = false;
            foreach (var win in this.Layers[layer].OfType<Window>().ToList())
                if(win.Closable)
                    closedsomething |= win.Close();
            return closedsomething;
        }
        public bool CloseAll()
        {
            bool closedsomething = false;
            var toclose = this.Layers.Values.SelectMany(l => l.OfType<Window>()).ToList();
            if (toclose.Any())
                foreach (var win in toclose)
                    if(win.Closable)
                        closedsomething |= win.Close();
            return closedsomething;
        }

        public void DrawOnCamera(SpriteBatch sb, Camera camera)
        {
            foreach (var layer in this.Layers)
                foreach (var ctrl in layer.Value)
                    ctrl.DrawOnCamera(sb, camera);
            SelectionManager.Instance.DrawOnCamera(sb, camera);
        }
        public void DrawWorld(MySpriteBatch sb, Camera camera)
        {
            foreach (var layer in this.Layers)
                foreach (var ctrl in layer.Value)
                    ctrl.DrawWorld(sb, camera);
            SelectionManager.Instance.DrawWorld(sb, camera);
        }

        public void Draw(SpriteBatch sb, Camera camera)
        {
            GraphicsDevice gd = Game1.Instance.GraphicsDevice;

            if (UITexture != null)
                if (UITexture.Width != Width || UITexture.Height != Height)
                    UITexture = null;
            RenderTarget2D uiTexture = UITexture ??= new RenderTarget2D(Game1.Instance.GraphicsDevice, Width, Height); // wtf

            gd.SetRenderTarget(uiTexture);
            gd.Clear(Color.Transparent);
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);

            //if (camera is not null)
            //    this.DrawOnCamera(sb, camera); // moving this to outside the ui rendertarget draw call, because we dont want it drawn to ui scale

            foreach (var layer in this.Layers)
                foreach (var ctrl in layer.Value) // TODO: FIX: WARNING: has thrown collection was modified during receiving map packets?
                    ctrl.Draw(sb, Bounds);

            TooltipManager.Instance.Draw(sb);
            DragDropManager.Instance.Draw(sb);
            sb.End();
            gd.SetRenderTarget(null);
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Scale % 1 > 0 ? SamplerState.AnisotropicClamp : SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
            if (camera is not null)
                this.DrawOnCamera(sb, camera); //moved this here, because we dont want it drawn to ui scale
            sb.Draw(uiTexture, new Rectangle(0, 0, Game1.Bounds.Width, Game1.Bounds.Height), Color.White);
            //DrawMouseCoords(sb);
            sb.End();
        }

        private static void DrawMouseCoords(SpriteBatch sb)
        {
            sb.DrawString(Font, Mouse.ToString(), Mouse, Color.White);
        }

        public static void DrawStringOutlined(SpriteBatch sb, string text, Vector2 position, Vector2 texOrigin, Color fill, Color outline, SpriteFont font)
        {
            if (text.IsNullEmptyOrWhiteSpace())
                return;
            position = position.ToRounded();
            position.Y -= 2; //monogame hack
            var txtBounds = font.MeasureString(text);
            txtBounds.Y -= 2;
            var origin = txtBounds * texOrigin;
            origin = new Vector2((int)origin.X, (int)origin.Y);
            foreach (var n in Vector2.Zero.GetNeighbors8())
                sb.DrawString(font, text, position + n, outline, 0, origin, 1, SpriteEffects.None, 0);
            sb.DrawString(font, text, position, fill, 0, origin, 1, SpriteEffects.None, 0);
        }
        public static void DrawStringOutlined(SpriteBatch sb, string text, Vector2 position, Vector2 texOrigin, Color fill, Color outline)
        {
            DrawStringOutlined(sb, text, position, texOrigin, fill, outline, UIManager.Font);
        }
        public static void DrawStringOutlined(SpriteBatch sb, string text, Vector2 position)
        {
            DrawStringOutlined(sb, text, position, Vector2.Zero, DefaultTextColor, Color.Black);
        }
        public static void DrawStringOutlined(SpriteBatch sb, string text, Vector2 position, Vector2 texOrigin)
        {
            DrawStringOutlined(sb, text, position, texOrigin, DefaultTextColor, Color.Black);
        }
        public static void DrawStringOutlined(SpriteBatch sb, string text, Vector2 position, Vector2 texOrigin, SpriteFont font)
        {
            DrawStringOutlined(sb, text, position, texOrigin, DefaultTextColor, Color.Black, font);
        }
        public static void DrawStringOutlined(SpriteBatch sb, string text, Vector2 position, Alignment.Horizontal hAlign = Alignment.Horizontal.Left, Alignment.Vertical vAlign = Alignment.Vertical.Top, float opacity = 1f, float depth = 0f)
        {
            DrawStringOutlined(sb, text, position, Color.Black, DefaultTextColor, hAlign, vAlign, opacity, depth);
        }
        public static void DrawStringOutlined(SpriteBatch sb, string text, Vector2 position, Color outline, Color fill, Alignment.Horizontal hAlign = Alignment.Horizontal.Left, Alignment.Vertical vAlign = Alignment.Vertical.Top, float opacity = 1f, float depth = 0)
        {
            var size = UIManager.Font.MeasureString(text);
            int xx = 0, yy = 0;
            switch (hAlign)
            {
                case Alignment.Horizontal.Center:
                    xx = (int)size.X / 2;
                    break;
                case Alignment.Horizontal.Right:
                    xx = (int)size.X;
                    break;
                default:
                    break;
            }
            switch (vAlign)
            {
                case Alignment.Vertical.Center:
                    yy = (int)size.Y / 2;
                    break;
                case Alignment.Vertical.Bottom:
                    yy = (int)size.Y;
                    break;
                default:
                    break;
            }
            var origin = new Vector2(xx, yy);

            foreach (var n in Vector2.Zero.GetNeighbors8())
                sb.DrawString(Font, text, position + n, new Color(0f, 0f, 0f, opacity), 0, origin, 1, SpriteEffects.None, depth);

            sb.DrawString(Font, text, position, new Color(1f, 1f, 1f, opacity), 0, origin, 1, SpriteEffects.None, depth);
        }
        public static void DrawStringOutlined(SpriteBatch sb, string text, Vector2 position, Color outline, Color fill, float scale, Alignment.Horizontal hAlign = Alignment.Horizontal.Left, Alignment.Vertical vAlign = Alignment.Vertical.Top, float opacity = 1f)
        {
            Vector2 size = UIManager.Font.MeasureString(text);
            int xx = 0, yy = 0;
            switch (hAlign)
            {
                case Alignment.Horizontal.Center:
                    xx = (int)size.X / 2;
                    break;
                case Alignment.Horizontal.Right:
                    xx = (int)size.X;
                    break;
                default:
                    break;
            }
            switch (vAlign)
            {
                case Alignment.Vertical.Center:
                    yy = (int)size.Y / 2;
                    break;
                case Alignment.Vertical.Bottom:
                    yy = (int)size.Y;
                    break;
                default:
                    break;
            }
            var origin = new Vector2(xx, yy);
            foreach (var n in Vector2.Zero.GetNeighbors8())
                sb.DrawString(Font, text, position + n, new Color(0f, 0f, 0f, opacity), 0, origin, scale, SpriteEffects.None, 0);

            sb.DrawString(Font, text, position, new Color(1f, 1f, 1f, opacity), 0, origin, scale, SpriteEffects.None, 0);
        }
        public static Control GetMouseOver()
        {
            return MouseOverList.Count == 0 ? null : MouseOverList[MouseOverList.Keys.Max()];
        }
        public static bool IsMouseOver(Control control)
        {
            return MouseOverList[MouseOverList.Keys.Max()] == control;
        }

        public bool RemoveWindow(Window window)
        {
            /// if the window was a dialog, move the dialogblock behind the next topmost window, or remove it if there are no other dialogs
            if (this.Layers[window.Layer].Remove(window))
            {
                if (window.Layer == LayerDialog)
                {
                    this.Layers[LayerDialog].Remove(this.DialogBlock);
                    int index = this.Layers[LayerDialog].Count - 1;
                    if (index < 0)
                        return true;

                    this.Layers[LayerDialog].Insert(index, this.DialogBlock);
                }
                return true;
            }
            return false;
        }
        public bool Remove(Control control)
        {
            /// if the window was a dialog, move the dialogblock behind the next topmost window, or remove it if there are no other dialogs
            if (this.Layers[control.Layer].Remove(control))
            {
                if (control.Layer == LayerDialog)
                {
                    this.Layers[LayerDialog].Remove(this.DialogBlock);
                    int index = this.Layers[LayerDialog].Count - 1;
                    if (index < 0)
                        return true;

                    this.Layers[LayerDialog].Insert(index, this.DialogBlock);
                }
                return true;
            }
            return false;
        }
  
        public void Dispose()
        {
            WindowManagers.Remove(this);
            Game1.Instance.graphics.DeviceReset -= this.graphics_DeviceReset;
        }
        public void Initialize()
        {
            foreach (var layer in this.Layers)
                foreach (Control ctrl in layer.Value)
                    ctrl.Dispose();
        }

        public void OnGotFocus() { }
        public void OnLostFocus() { }

        public void HandleKeyPress(System.Windows.Forms.KeyPressEventArgs e)
        {
            foreach (var layer in this.Layers.Reverse())
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                    c.HandleKeyPress(e);
            }
        }
        public void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Handled)
                return;

            if (e.KeyCode == System.Windows.Forms.Keys.Escape)
            {
                if (ToolManager.Clear() || SelectionManager.ClearTargets() || this.CloseAll())
                    e.Handled = true;
            }

            foreach (var layer in this.Layers.Reverse())
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                    c.HandleKeyDown(e);
            }
        }
        public void HandleKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Handled)
                return;

            foreach (var layer in this.Layers.Reverse())
            {
                List<Control> wins = layer.Value.ToList();
                foreach (Control c in wins)
                    c.HandleKeyUp(e);
            }
        }
        public virtual void HandleMouseMove(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.ActiveControl is null)
                return;

            this.ActiveControl.HandleMouseMove(e);
            e.Handled = true;
        }
        public virtual void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (var layer in this.Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                    c.HandleLButtonDown(e);
            }

            if (this.ActiveControl != null)
                e.Handled = true;
        }
        public virtual void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (var layer in this.Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                    c.HandleLButtonUp(e);
            }
        }
        public virtual void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (var layer in this.Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                    c.HandleRButtonDown(e);
            }
            if (this.ActiveControl is not null)
                e.Handled = true;
        }
        public virtual void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (var layer in this.Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                    c.HandleRButtonUp(e);
            }
        }
        public virtual void HandleMiddleUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (var layer in this.Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                    c.HandleMiddleUp(e);
            }
        }
        public virtual void HandleMiddleDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (var layer in this.Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                    c.HandleMiddleDown(e);
            }
        }
        public virtual void HandleMouseWheel(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (var layer in this.Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                    c.HandleMouseWheel(e);
            }
        }
        public virtual void HandleLButtonDoubleClick(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (var layer in this.Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                    c.HandleLButtonDoubleClick(e);
            }
        }
        internal void OnGameEvent(GameEvent e)
        {
            foreach (var layer in this.Layers)
            {
                List<Control> wins = layer.Value.ToList();
                wins.Reverse();
                foreach (Control c in wins)
                    c.OnGameEvent(e);
            }
            SelectionManager.Instance.OnGameEvent(e);
        }

        static List<Rectangle> DivideScreenQuad(Rectangle screen, Rectangle rect)
        {
            var right = rect.X + rect.Width;
            var bottom = rect.Y + rect.Height;
            var dx = screen.X + screen.Width - right;
            var dy = screen.Y + screen.Height - bottom;

            var list = new List<Rectangle>();
            if (rect.Y > screen.Y)
                list.Add(new Rectangle(screen.X, screen.Y, screen.Width, rect.Y - screen.Y));

            if (bottom < screen.Y + screen.Height)
                list.Add(new Rectangle(screen.X, bottom, screen.Width, dy));

            if (rect.X > screen.X)
                list.Add(new Rectangle(screen.X, screen.Y, rect.X - screen.X, screen.Height));

            if (right < screen.X + screen.Width)
                list.Add(new Rectangle(right, screen.Y, dx, screen.Height));

            return list;
        }
        List<Rectangle> GetFreeRectangles()
        {
            var freeRects = new List<Rectangle>() { UIManager.Bounds };
            var divided = true;
            var windows = this.Layers[LayerWindows].Where(c => c is Window).ToList();
            windows.Reverse();
            while (divided)
            {
                divided = false;

                foreach (var control in windows) // TODO: fix HUD being a control itself
                {
                    foreach (var rect in freeRects.ToList())
                    {
                        var bounds = control.BoundsScreen;
                        if (rect.Intersects(bounds))
                        {
                            freeRects.Remove(rect);
                            var divisions = DivideScreenQuad(rect, bounds);
                            foreach (var div in divisions)
                            {
                                if (!freeRects.Contains(div))
                                    freeRects.Add(div);
                            }
                        }
                    }
                }
            }
            return freeRects;
        }
        public Rectangle FindBestUncoveredRectangle(Vector2 dimensions)
        {
            int w = (int)dimensions.X, h = (int)dimensions.Y;
            var freeRects = this.GetFreeRectangles();
            var bestRect = new Rectangle(0, 0, w, h);
            var size = w * h;
            var ordered = freeRects.OrderBy(r => new Vector2(r.X, r.Y).LengthSquared()).ToList();
            foreach (var rect in ordered)
            {
                if (w <= rect.Width && h <= rect.Height)
                    return rect;
            }
            return bestRect;
        }
        public void Add(Element element)
        {
            var control = element as Control;

            if(control.Layer == LayerDialog)
            {
                this.Layers[LayerDialog].Remove(this.DialogBlock);
                this.Layers[LayerDialog].Add(this.DialogBlock);
            }

            this.Layers[control.Layer].Add(control);
            if (!this.ControlsInMemory.Contains(control))
                this.ControlsInMemory.Add(control);
        }
        public bool Remove(Element element)
        {
            var control = element as Control;
            if (control.Layer == LayerDialog)
                this.Layers[LayerDialog].Remove(this.DialogBlock);
            if (this.ControlsInMemory.Contains(control)) // TODO: why do i remove it immediately? maintain a fixed size buffer?
                this.ControlsInMemory.Remove(control);
            return this.Layers[control.Layer].Remove(control);
        }
        public bool Contains(Element element)
        {
            var control = element as Control;
            return this.Layers[control.Layer].Contains(control);
        }
        public void Block(bool enabled)
        {
            this.Remove(this.DialogBlock);
            if (enabled)
                this.Add(this.DialogBlock);
        }
    }
}

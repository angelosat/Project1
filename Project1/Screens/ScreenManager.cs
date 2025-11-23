using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using System.Windows.Forms;
using System.Linq;

namespace Start_a_Town_
{
    public class ScreenManager
    {
        //public UIManager WindowManager;

        public static Stack<GameScreen> GameScreens = new Stack<GameScreen>();
        static ScreenManager _Instance;
        public static ScreenManager Instance => _Instance ??= new ScreenManager();

        static public void LoadContent()
        {
            MainScreen.LoadContent();
        }

        static public void Initialize()
        {
            //Game1.Instance.Window.TextInput += Window_TextInput;
            Game1.TextInput.KeyPress += new KeyPressEventHandler(Instance.TextInput_KeyPress);
            Game1.TextInput.KeyDown += new KeyEventHandler(Instance.TextInput_KeyDown);
            Game1.TextInput.KeyUp += new KeyEventHandler(Instance.TextInput_KeyUp);
            Game1.TextInput.MouseMove += new EventHandler<HandledMouseEventArgs>(TextInput_MouseMove);
            Game1.TextInput.LButtonDown += new EventHandler<HandledMouseEventArgs>(TextInput_LButtonDown);
            Game1.TextInput.LMouseUp += new EventHandler<HandledMouseEventArgs>(TextInput_LMouseUp);
            Game1.TextInput.RMouseDown += new EventHandler<HandledMouseEventArgs>(TextInput_RMouseDown);
            Game1.TextInput.RMouseUp += new EventHandler<HandledMouseEventArgs>(TextInput_RMouseUp);
            Game1.TextInput.MMouseUp += new EventHandler<HandledMouseEventArgs>(TextInput_MiddleUp);
            Game1.TextInput.MMouseDown += new EventHandler<HandledMouseEventArgs>(TextInput_MiddleDown);
            Game1.TextInput.MouseWheel += new EventHandler<HandledMouseEventArgs>(TextInput_MouseWheel);
            Game1.TextInput.LButtonDblClk += new EventHandler<HandledMouseEventArgs>(TextInput_LButtonDblClk);
        }

        //private static void Window_TextInput(object sender, TextInputEventArgs e)
        //{
        //    e.Character.ToConsole();
        //}

        private static void TextInput_LButtonDblClk(object sender, HandledMouseEventArgs e)
        {
            if (!GameScreens.Any())
                return;
            GameScreens.Peek().HandleLButtonDoubleClick(e);
        }

        static void TextInput_RMouseUp(object sender, HandledMouseEventArgs e)
        {
            if (!GameScreens.Any())
                return;
            GameScreens.Peek().HandleRButtonUp(e);
        }

        static void TextInput_MouseWheel(object sender, HandledMouseEventArgs e)
        {
            if (!GameScreens.Any())
                return;
            GameScreens.Peek().HandleMouseWheel(e);
        }
        static void TextInput_MiddleDown(object sender, HandledMouseEventArgs e)
        {
            if (!GameScreens.Any())
                return;
            GameScreens.Peek().HandleMiddleDown(e);
        }
        static void TextInput_MiddleUp(object sender, HandledMouseEventArgs e)
        {
            if (!GameScreens.Any())
                return;
            GameScreens.Peek().HandleMiddleUp(e);
        }
        static void TextInput_RMouseDown(object sender, HandledMouseEventArgs e)
        {
            if (!GameScreens.Any())
                return;
            DragDropManager.Instance.HandleRButtonDown(e);
            GameScreens.Peek().HandleRButtonDown(e);
        }

        static void TextInput_LMouseUp(object sender, HandledMouseEventArgs e)
        {
            if (!GameScreens.Any())
                return;
            DragDropManager.Instance.HandleLButtonUp(e);
            GameScreens.Peek().HandleLButtonUp(e);
        }

        static void TextInput_LButtonDown(object sender, HandledMouseEventArgs e)
        {
            if (!GameScreens.Any())
                return;
            DragDropManager.Instance.HandleLButtonDown(e);
            GameScreens.Peek().HandleLButtonDown(e);
        }

        static void TextInput_MouseMove(object sender, HandledMouseEventArgs e)
        {
            if (!GameScreens.Any())
                return;
            GameScreens.Peek().HandleMouseMove(e);
        }

        void TextInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (!GameScreens.Any())
                return;
            GameScreens.Peek().HandleKeyDown(e);
        }

        void TextInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (!GameScreens.Any())
                return;
            GameScreens.Peek().HandleKeyUp(e);
        }

        void TextInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!GameScreens.Any())
                return;
            GameScreens.Peek().HandleKeyPress(e);
        }

        public ScreenManager()
        {
            //this.WindowManager = new UIManager();
        }

        public static T GetCurrent<T>() where T : GameScreen
        {
            return GameScreens.Count > 0 ? GameScreens.Peek() as T : null;
        }

        public static GameScreen Current
        {
            get { return GameScreens.Count > 0 ? GameScreens.Peek() : null; }
        }

        static public GameScreen CurrentScreen
        {
            get
            {
                if (GameScreens.Count == 0)
                    return null;
                return GameScreens.Peek();
            }
        }

        static public InputState Input = new InputState();
        public void Update(Game1 game, GameTime gt)
        {
            if (GameScreens.Count == 0)
            {
                GameScreens.Push(MainScreen.Instance);
                GameScreens.Peek().Initialize(null);
            }
            GameScreen screen = GameScreens.Peek();
            TooltipManager.Instance.Update();
            DragDropManager.Instance.Update();

            screen.Update(game, gt);
           
            if (!Game1.Instance.IsActive)
                return;

            Controller.Input.Update();
            //this.WindowManager.Update(game, gt);
        }

        public static bool Add(GameScreen screen)
        {
            GameScreens.Push(screen);
            return true;
        }

        public void Draw(SpriteBatch sb)
        {
            if (GameScreens.Count == 0)
                return;
            GameScreens.Peek().Draw(sb);
        }
        
        /// <summary>
        /// Removes current screen
        /// </summary>
        /// <returns></returns>
        public static bool Remove()
        {
            if (GameScreens.Count == 1)
                return false;
            GameScreens.Pop().Dispose();
            return true;
        }
    }
}

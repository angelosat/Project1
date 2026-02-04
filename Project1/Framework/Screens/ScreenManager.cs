using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Input;
using Project1.Framework.Input;
using Start_a_Town_;
using Project1.Framework.Base;

namespace Project1.Framework.Screens
{
    public class ScreenManager
    {
        //public UIManager WindowManager;

        public static Stack<GameScreen> GameScreens = new Stack<GameScreen>();
        static ScreenManager _Instance;
        public static ScreenManager Instance => _Instance ??= new ScreenManager();
        static readonly ConcurrentQueue<Action> MouseInputQueue = [];
        static public void LoadContent()
        {
            MainScreen.LoadContent();
        }

        static public void Initialize()
        {
            //Game1.Instance.Window.TextInput += Window_TextInput;
            Game1.Input.KeyPress += new KeyPressEventHandler(Instance.TextInput_KeyPress);
            Game1.Input.KeyDown += new KeyEventHandler(Instance.TextInput_KeyDown);
            Game1.Input.KeyUp += new KeyEventHandler(Instance.TextInput_KeyUp);
            Game1.Input.MouseMove += new EventHandler<HandledMouseEventArgs>(TextInput_MouseMove);
            Game1.Input.LMouseDown += new EventHandler<HandledMouseEventArgs>(TextInput_LMouseDown);
            Game1.Input.LMouseUp += new EventHandler<HandledMouseEventArgs>(TextInput_LMouseUp);
            Game1.Input.RMouseDown += new EventHandler<HandledMouseEventArgs>(TextInput_RMouseDown);
            Game1.Input.RMouseUp += new EventHandler<HandledMouseEventArgs>(TextInput_RMouseUp);
            Game1.Input.MMouseUp += new EventHandler<HandledMouseEventArgs>(TextInput_MiddleUp);
            Game1.Input.MMouseDown += new EventHandler<HandledMouseEventArgs>(TextInput_MiddleDown);
            Game1.Input.MouseWheel += new EventHandler<HandledMouseEventArgs>(TextInput_MouseWheel);
            Game1.Input.LButtonDblClk += new EventHandler<HandledMouseEventArgs>(TextInput_LButtonDblClk);
        }


        //private static void Window_TextInput(object sender, TextInputEventArgs e)
        //{
        //    e.Character.ToConsole();
        //}

        private static void TextInput_LButtonDblClk(object sender, HandledMouseEventArgs e)
        {
            MouseInputQueue.Enqueue(() => GameScreens.Peek()?.HandleLButtonDoubleClick(e));
        }

        static void TextInput_RMouseUp(object sender, HandledMouseEventArgs e)
        {
            MouseInputQueue.Enqueue(() => GameScreens.Peek()?.HandleRButtonUp(e));
        }

        static void TextInput_MouseWheel(object sender, HandledMouseEventArgs e)
        {
            MouseInputQueue.Enqueue(() => GameScreens.Peek()?.HandleMouseWheel(e));
        }
        static void TextInput_MiddleDown(object sender, HandledMouseEventArgs e)
        {
            MouseInputQueue.Enqueue(() => GameScreens.Peek()?.HandleMiddleDown(e));
        }
        static void TextInput_MiddleUp(object sender, HandledMouseEventArgs e)
        {
            MouseInputQueue.Enqueue(() => GameScreens.Peek()?.HandleMiddleUp(e));
        }
        static void TextInput_RMouseDown(object sender, HandledMouseEventArgs e)
        {
            MouseInputQueue.Enqueue(() =>
            {
                DragDropManager.Instance.HandleRButtonDown(e);
                GameScreens.Peek()?.HandleRButtonDown(e);
            });
        }

        static void TextInput_LMouseUp(object sender, HandledMouseEventArgs e)
        {
            MouseInputQueue.Enqueue(() => { 
                DragDropManager.Instance.HandleLButtonUp(e);
                GameScreens.Peek()?.HandleLButtonUp(e);
            });
        }

        static void TextInput_LMouseDown(object sender, HandledMouseEventArgs e)
        {
            MouseInputQueue.Enqueue(() => { 
                DragDropManager.Instance.HandleLButtonDown(e);
                GameScreens.Peek()?.HandleLButtonDown(e);
            });
        }
        static void TextInput_MouseMove(object sender, HandledMouseEventArgs e)
        {
            MouseInputQueue.Enqueue(() => GameScreens.Peek()?.HandleMouseMove(e));
        }

        void TextInput_KeyDown(object sender, KeyEventArgs e)
        {
            GameScreens.Peek()?.HandleKeyDown(e);
        }

        void TextInput_KeyUp(object sender, KeyEventArgs e)
        {
            GameScreens.Peek()?.HandleKeyUp(e);
        }

        void TextInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            GameScreens.Peek()?.HandleKeyPress(e);
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

            while(MouseInputQueue.TryDequeue(out var e))
                e.Invoke();

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

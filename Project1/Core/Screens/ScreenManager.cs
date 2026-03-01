using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.UI;
using Project1.Framework.Input;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Project1.Core.Screens
{
    public class ScreenManager
    {
        public static Stack<GameScreen> GameScreens = new();
        static ScreenManager _Instance;
        public static ScreenManager Instance => _Instance ??= new ScreenManager();
        static readonly ConcurrentQueue<Action> InputQueue = [];
        static public void LoadContent()
        {
            MainScreen.LoadContent();
        }

        static public void Initialize()
        {
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

        private static void TextInput_LButtonDblClk(object sender, HandledMouseEventArgs e)
            => InputQueue.Enqueue(() => GameScreens.Peek()?.InputRouter.HandleLButtonDoubleClick(e));

        static void TextInput_MouseWheel(object sender, HandledMouseEventArgs e)
            => InputQueue.Enqueue(() => GameScreens.Peek()?.InputRouter.HandleMouseWheel(e));
        
        static void TextInput_MiddleDown(object sender, HandledMouseEventArgs e)
            => InputQueue.Enqueue(() => GameScreens.Peek()?.InputRouter.HandleMiddleDown(e));
        
        static void TextInput_MiddleUp(object sender, HandledMouseEventArgs e)
            => InputQueue.Enqueue(() => GameScreens.Peek()?.InputRouter.HandleMiddleUp(e));
        
        static void TextInput_MouseMove(object sender, HandledMouseEventArgs e)
            => InputQueue.Enqueue(() => GameScreens.Peek()?.InputRouter.HandleMouseMove(e));

        void TextInput_KeyDown(object sender, KeyEventArgs e)
            => InputQueue.Enqueue(() => GameScreens.Peek()?.InputRouter.HandleKeyDown(e));

        void TextInput_KeyUp(object sender, KeyEventArgs e)
            => InputQueue.Enqueue(() => GameScreens.Peek()?.InputRouter.HandleKeyUp(e));

        void TextInput_KeyPress(object sender, KeyPressEventArgs e)
            => InputQueue.Enqueue(() => GameScreens.Peek()?.InputRouter.HandleKeyPress(e));

        static void TextInput_LMouseUp(object sender, HandledMouseEventArgs e)
            => InputQueue.Enqueue(() => {
                DragDropManager.Instance.HandleLButtonUp(e);
                GameScreens.Peek()?.InputRouter.HandleLButtonUp(e);
            });


        static void TextInput_LMouseDown(object sender, HandledMouseEventArgs e)
            => InputQueue.Enqueue(() => {
                DragDropManager.Instance.HandleLButtonDown(e);
                GameScreens.Peek()?.InputRouter.HandleLButtonDown(e);
            });

        static void TextInput_RMouseDown(object sender, HandledMouseEventArgs e)
            => InputQueue.Enqueue(() =>
            {
                DragDropManager.Instance.HandleRButtonDown(e);
                GameScreens.Peek()?.InputRouter.HandleRButtonDown(e);
            });

        static void TextInput_RMouseUp(object sender, HandledMouseEventArgs e)
            => InputQueue.Enqueue(() =>
            {
                DragDropManager.Instance.HandleRButtonUp(e);
                GameScreens.Peek()?.InputRouter.HandleRButtonUp(e);
            });
            
        public ScreenManager()
        {
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

        public void Update(Game1 game, GameTime gt)
        {
            if (GameScreens.Count == 0)
            {
                GameScreens.Push(MainScreen.Instance);
                GameScreens.Peek().Initialize(null);
            }

            if (!Game1.Instance.IsActive)
                return;

            InputState.Instance.Update();
            while (InputQueue.TryDequeue(out var e))
                e.Invoke();

            TooltipManager.Instance.Update();
            DragDropManager.Instance.Update();

            GameScreens.Peek()?.Update(game, gt);
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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Input;
using Project1.Core.Networking;
using Project1.Framework.Events;
using Project1.Framework.Input;
using Project1.Framework.UI;
using System;
using System.Windows.Forms;

namespace Project1.Core.Screens
{
    public abstract class GameScreen : IDisposable, IInputEventHandler
    {
        public float LoadingPercentage;
        public SpriteBatch spriteBatch;
        public UIManager WindowManager;
        public ToolManager ToolManager;
        internal readonly InputRouter InputRouter = new();

        public virtual Camera Camera => Client.Instance.Map.Camera;

        public GameScreen()
        {
            this.WindowManager = new();
        }

        public virtual void HandleKeyPress(KeyPressEventArgs e) { }
        public virtual void HandleKeyDown(KeyEventArgs e) { }
        public virtual void HandleKeyUp(KeyEventArgs e) { }
        public virtual void HandleMouseMove(HandledMouseEventArgs e) { }
        public virtual void HandleLButtonDown(HandledMouseEventArgs e) { }
        public virtual void HandleLButtonUp(HandledMouseEventArgs e) { }
        public virtual void HandleRButtonDown(HandledMouseEventArgs e) { }
        public virtual void HandleRButtonUp(HandledMouseEventArgs e) { }
        public virtual void HandleMiddleUp(HandledMouseEventArgs e) { }
        public virtual void HandleMiddleDown(HandledMouseEventArgs e) { }
        public virtual void HandleMouseWheel(HandledMouseEventArgs e) { }
        public virtual void HandleLButtonDoubleClick(HandledMouseEventArgs e) { }

        public virtual GameScreen Initialize(NetEndpoint net)
        {
            return this;
        }
        public virtual void Update(Game1 game, GameTime gt)
        {
        }
        public abstract void Draw(SpriteBatch sb);


        public virtual void Dispose()
        {
            GC.Collect();
        }

        internal virtual void OnGameEvent(GameEvent e)
        {
            this.WindowManager.OnGameEvent(e);
        }
    }
}

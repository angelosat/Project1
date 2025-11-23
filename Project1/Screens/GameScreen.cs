using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Start_a_Town_
{
    public abstract class GameScreen : IDisposable, IKeyEventHandler
    {
        public float LoadingPercentage;
        public SpriteBatch spriteBatch;
        public UIManager WindowManager;
        public ToolManager ToolManager;
        public Stack<IKeyEventHandler> KeyHandlers;

        public virtual Camera Camera => Net.Client.Instance.Map.Camera;

        public GameScreen()
        {
            this.WindowManager = new();
            this.KeyHandlers = new Stack<IKeyEventHandler>();
        }

        public virtual void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            foreach (var handler in this.KeyHandlers)
                handler.HandleKeyDown(e);
        }

        public virtual void HandleKeyPress(System.Windows.Forms.KeyPressEventArgs e)
        {
            foreach (var handler in this.KeyHandlers)
                if (!e.Handled)
                    handler.HandleKeyPress(e);
        }

        public virtual void HandleKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            foreach (var handler in this.KeyHandlers)
                handler.HandleKeyUp(e);
        }
        public virtual void HandleMouseMove(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (var handler in this.KeyHandlers)
                handler.HandleMouseMove(e);
        }
        public virtual void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (var handler in this.KeyHandlers)
                handler.HandleLButtonDown(e);
        }
        public virtual void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (var handler in this.KeyHandlers)
                handler.HandleLButtonUp(e);
        }
        public virtual void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (var handler in this.KeyHandlers)
                handler.HandleRButtonDown(e);
        }
        public virtual void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (var handler in this.KeyHandlers)
                handler.HandleRButtonUp(e);
        }
        public virtual void HandleMiddleUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (var handler in this.KeyHandlers)
                handler.HandleMiddleUp(e);
        }
        public virtual void HandleMiddleDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            foreach (var handler in this.KeyHandlers)
                handler.HandleMiddleDown(e);
        }

        public virtual void HandleMouseWheel(HandledMouseEventArgs e)
        {
            foreach (var handler in this.KeyHandlers)
                if (!e.Handled)
                    handler.HandleMouseWheel(e);
        }
        public void HandleLButtonDoubleClick(HandledMouseEventArgs e)
        {
            foreach (var handler in this.KeyHandlers)
                if (!e.Handled)
                    handler.HandleLButtonDoubleClick(e);
        }

        public virtual GameScreen Initialize(INetwork net)
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

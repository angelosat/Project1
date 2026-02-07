using System;
using Microsoft.Xna.Framework.Graphics;

namespace Project1.Core
{
    public class DragEventArgs : EventArgs
    {
        public object Item;
        public object Source;
        public DragDropEffects Effects;
        public virtual void Draw(SpriteBatch sb) { }
    }
}

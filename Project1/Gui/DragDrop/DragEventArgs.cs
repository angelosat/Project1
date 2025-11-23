using System;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_
{
    public class DragEventArgs : EventArgs
    {
        public object Item;
        public object Source;
        public DragDropEffects Effects;
        public virtual void Draw(SpriteBatch sb) { }
    }
}

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public enum DrawMode
    {
        Normal = 0x0, OwnerDrawFixed = 0x1, OwnerDrawVariable = 0x2
    }

    public class DrawItemEventArgs : EventArgs
    {
        public SpriteBatch SpriteBatch;
        public Rectangle Bounds;

        public DrawItemEventArgs(SpriteBatch sb, Rectangle bounds)
        {
            SpriteBatch = sb;
            Bounds = bounds;
        }
        public DrawItemEventArgs(SpriteBatch sb)
        {
            SpriteBatch = sb;
        }
    }

    public class MeasureItemEventArgs : EventArgs
    {
        public int Index;
        public int ItemHeight;
        public MeasureItemEventArgs(int index)
        {
            Index = index;
        }
    }
}

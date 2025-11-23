using Microsoft.Xna.Framework;
using System;

namespace Start_a_Town_
{
    public abstract class Element
    {
        Func<Vector2> _LocationFunc = () => Vector2.Zero;
        public virtual Func<Vector2> LocationFunc
        {
            get => _LocationFunc; 
            set => _LocationFunc = value;
        }

        public Vector2 Location = Vector2.Zero;

        public Vector2 MouseOffset;
        public virtual int Width { get; set; }
        public virtual int Height { get; set; }
        public virtual Vector2 Dimensions
        {
            get => new Vector2(Width, Height);
            set
            {
                Width = (int)value.X;
                Height = (int)value.Y;
            }
        }
        Vector2 _Anchor = Vector2.Zero;
        public virtual Vector2 Anchor
        {
            get => _Anchor; 
            set => _Anchor = value;
        }
        bool _MouseHover;
        public bool MouseHover
        {
            get => _MouseHover;
            set => this._MouseHover = value;
        }
        public bool MouseHoverLast;
        public int Depth = 0;
        int _padding;
        public virtual int Padding { get => this._padding; set => this._padding = value; }
        protected int z = 0;
        public int Z => z; 

        public virtual void Update()
        {
        }

        protected virtual void OnParentChanged()
        {
        }

        public abstract void Unselect();
    }
}

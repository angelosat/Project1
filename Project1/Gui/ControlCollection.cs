using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    public class ControlCollection : Collection<Control>
    {
        readonly Control Parent;
        public ControlCollection(Control owner)
        {
            Parent = owner;
        }

        public Vector2 BottomRight
        {
            get
            {
                float xMax = 0, yMax = 0;
                foreach(var c in this)
                {
                    xMax = Math.Max(xMax, c.Location.X + c.Width);
                    yMax = Math.Max(yMax, c.Location.Y + c.Height);
                }
                return new Vector2(xMax, yMax);
            }
        }
        public Vector2 TopRight
        {
            get
            {
                float xMax = 0, y = 0;
                foreach (var c in this)
                {
                    xMax = Math.Max(xMax, c.Location.X + c.Width);
                    y = Math.Min(y, c.Location.Y);
                }
                return new Vector2(xMax, y);
            }
        }
        public Vector2 BottomLeft
        {
            get
            {
                if (this.Count == 0)
                    return Vector2.Zero;

                int x = 0, y = 0;
                foreach (var c in this)
                {
                    x = Math.Min(x, c.Left);
                    y = Math.Max(y, c.Bottom);
                }
                return new Vector2(x, y);
            }
        }
        public int Bottom
        {
            get
            {
                if (this.Count == 0)
                    return 0;

                int y = 0;
                foreach (var c in this)
                {
                    y = Math.Max(y, c.Bottom);
                }
                return y;
            }
        }

        public void Add(params Control[] controls)
        {
            foreach (var control in controls)
            {
                if (this.Contains(control))
                    throw new Exception();
                if (control == this.Parent)
                    throw new Exception();
                if (control == null)
                    throw new Exception();
                control.Parent?.Controls.Remove(control);
                control.Parent = Parent;
                base.Add(control);
                //control.Parent = Parent;
                //Parent.OnControlAdded(control);
            }
            //this.Parent.Validate();
        }
        public void Insert(int index, IEnumerable<Control> controls)
        {
            foreach (var c in controls)
                this.InsertItem(index++, c);
        }
        protected override void InsertItem(int index, Control item)
        {
            base.InsertItem(index, item);
            item.Parent = Parent;
            Parent.OnControlAdded(item);
        }
        public void AlignVertically(int spacing = 0)
        {
            //var oldsize = this.Parent.ClientSize;

            var prev = 0;
            foreach (var c in this)
            {
                c.Location.Y = prev;
                prev = c.Bottom + spacing;
            }
            if (this.Parent.AutoSize)
                this.Parent.ClientSize = this.Parent.PreferredClientSize;

            //if (this.Parent.ClientSize != oldsize)
            //    this.Parent.Parent?.OnControlResized(this.Parent);
        }
        public void AlignHorizontally(int spacing = 0)
        {
            var prev = 0;
            foreach (var c in this)
            {
                c.Location.X = prev;
                prev = c.Right + spacing;
            }
            if (this.Parent.AutoSize)
                this.Parent.ClientSize = this.Parent.PreferredClientSize;
        }
        public void AlignCenterHorizontally()
        {
            var maxheight = this.Max(c => c.Height);
            foreach (var c in this)
            {
                c.Location = new Vector2(c.Location.X, maxheight / 2);
                c.Anchor = new Vector2(c.Anchor.X, .5f); // DONT reset contrl's x anchor
            }
        }
        public void RemoveAll(Func<Control, bool> predicate)
        {
            var toremove = this.Items.Where(predicate).ToList();
            foreach (var c in toremove)
                this.Remove(c);
        }
        protected override void RemoveItem(int index)
        {
            var ctrl = this[index];
            base.RemoveItem(index);
            this.Parent.OnControlRemoved(ctrl);
        }
        public int FindIndex(Func<Control, bool> p)
        {
            return this.IndexOf(this.Find(p));
        }

        public Control Find(Func<Control, bool> p)
        {
            return this.Items.First(p);
        }
    }
}

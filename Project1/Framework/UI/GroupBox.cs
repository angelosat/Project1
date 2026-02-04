using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Project1.Framework.Base;
using Start_a_Town_.UI;

namespace Project1.Framework.UI
{
    public class GroupBox : Control
    {
        public GroupBox() { MouseThrough = true; AutoSize = true; }
        public GroupBox(string name)
            : this()
        {
            this.Name = name;
        }
        public GroupBox(int size) : this(size, size) { }

        public GroupBox(int width, int height)
            :this(null, width, height)
        {
            
        }
        public GroupBox(string name, int width, int height)
        {
            this.Name = name;
            this.AutoSize = false;
            this.Size = new Rectangle(0, 0, width, height);
        }
        internal override void OnControlAdded(Control control)
        {
            base.OnControlAdded(control);
            if (this.AutoSize)
                this.Parent?.OnControlResized(this);
        }
        internal override void OnControlRemoved(Control control)
        {
            base.OnControlRemoved(control);
            if (this.AutoSize)
                this.Parent?.OnControlResized(this);
        }
        internal override void OnControlResized(Control control)
        {
            //this.ClientSize = PreferredClientSize;
            this.ApplyAutoSize();
            this.Parent?.OnControlResized(this);
        }
       
        public GroupBox AddControlsLineWrap(int width, params ButtonBase[] labels)
        {
            return this.AddControlsLineWrap(labels, width);
        }
        public GroupBox AddControlsLineWrap(params ButtonBase[] labels)
        {
            return this.AddControlsLineWrap(labels as IEnumerable<ButtonBase>);
        }
        public virtual GroupBox AddControlsLineWrap(IEnumerable<ButtonBase> labels, int width = int.MaxValue)
        {
            if (!this.Controls.Any() && labels.Count() == 1)
                return this.AddControls(labels.First()) as GroupBox;

            var lastControl = this.Controls.LastOrDefault();
            var currentX = lastControl?.Right ?? 0;
            var currentY = lastControl?.Top ?? 0;
            var space = (int)UIManager.Font.MeasureString(" ").X;
            foreach (var l in labels)
            {
                if (currentX + l.Width > width)
                {
                    currentX = 0;
                    currentY += l.Height;
                }
                l.Location = new IntVec2(currentX, currentY);
                currentX += l.Width + space;
                this.AddControls(l);
            }
            //if (width != int.MaxValue)
            //    this.Width = width;
            return this;
        }
        public GroupBox AddControlsLineWrap(int width, params ButtonBaseNew[] labels)
        {
            return this.AddControlsLineWrap(labels, width);
        }
        public GroupBox AddControlsLineWrap(params ButtonBaseNew[] labels)
        {
            return this.AddControlsLineWrap(labels as IEnumerable<ButtonBaseNew>);
        }
        public virtual GroupBox AddControlsLineWrap(IEnumerable<ButtonBaseNew> labels, int width = int.MaxValue)
        {
            if (!this.Controls.Any() && labels.Count() == 1)
                return this.AddControls(labels.First()) as GroupBox;

            var lastControl = this.Controls.LastOrDefault();
            var currentX = lastControl?.Right ?? 0;
            var currentY = lastControl?.Top ?? 0;
            var space = (int)UIManager.Font.MeasureString(" ").X;
            foreach (var l in labels)
            {
                if (currentX + l.Width > width)
                {
                    currentX = 0;
                    currentY += l.Height;
                }
                l.Location = new IntVec2(currentX, currentY);
                currentX += l.Width + space;
                this.AddControls(l);
            }
            return this;
        }
        internal void CenterControlsAlignmentVertically()
        {
            var maxh = this.Controls.Max(c => c.Height);
            foreach (var c in this.Controls)
                c.Location.Y = (maxh - c.Height) / 2;
        }

        public override void OnLayout(int availableWidth, int availableHeight)
        {
            base.OnLayout(availableWidth, availableHeight);
            var w = availableWidth - this.Padding - this.Padding;
            var h = availableHeight - this.Padding - this.Padding;
            foreach(var child in this.Controls)
                child.Layout(w, h);
        }

        internal Control ToScrollableBox(int boundsW, int boundsH)
        {
            if (this.Width <= boundsW && this.Height <= boundsH)
                return new GroupBox().AddControls(this);
            var mode = ScrollModes.None;
            if (this.Width > boundsW)
                mode = ScrollModes.Horizontal;
            if (this.Height > boundsH)
                mode |= ScrollModes.Vertical;
            return new ScrollableBoxNewNewNew(boundsW, boundsH, mode).AddControls(this);
        }
    }
}

using System;
using System.Windows.Forms;

namespace Start_a_Town_
{
    class ToolSelectRectangleBlocks : ToolDigging
    {
        public ToolSelectRectangleBlocks()
        {
        }
        public ToolSelectRectangleBlocks(IntVec3 origin, Action<IntVec3, IntVec3, bool> callback) : base(callback)
        {
            this.Begin = origin;
            this.End = this.Begin;
            this.Width = this.Height = 1;
            this.Enabled = true;
        }
        public override Messages MouseLeftUp(HandledMouseEventArgs e)
        {
            base.MouseLeftUp(e);
            return Messages.Remove;
        }
        public override Messages MouseRightUp(HandledMouseEventArgs e)
        {
            return Messages.Remove;
        }
    }
}

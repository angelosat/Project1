using System;

namespace Start_a_Town_
{
    [Obsolete]
    public class InteractionChopping : Interaction
    {
        public InteractionChopping()
            : base(
                "Chop",
                1
                )
        {
            this.Verb = "Chopping";
            this.ToolUse = ToolUseDefOf.Chopping;
        }
      
        public override void Perform()
        {
            var a = this.Actor;
            var t = this.Target; 
            var cell = a.Map.GetCell(t.Global);
            var block = cell.Block;
            block.Break(a, t.Global);
        }

        public override object Clone()
        {
            return new InteractionChopping();
        }
    }
}

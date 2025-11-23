using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class UINpcFrameContainer : GroupBox
    {
        List<Actor> PrevActors = new List<Actor>();
        const int Spacing = 1;//5;
        public UINpcFrameContainer()
        {
        }
        public override void Update()
        {
            if (!Camera.DrawnOnce)
                return;
            var actors = Net.Client.Instance.Map.Town.GetAgents().Where(a => a != null).ToList();
            var toInit = actors.Where(a => !this.PrevActors.Contains(a));
            foreach (var a in toInit)
                this.AddControlsTopRight(Spacing, new UINpcFrame(a));
            var toRemove = this.PrevActors.Where(a => !actors.Contains(a));
            if (toRemove.Any())
            {
                foreach (var a in toRemove)
                {
                    this.Controls.RemoveAll(c => c.Tag == a);
                }
                this.AlignLeftToRight();
            }
            this.PrevActors = actors;
            base.Update();
        }
    }
}

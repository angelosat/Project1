using System;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    class TreeComponent : EntityComponent
    {
        public class States
        {
            static public void FreshlyPlanted(GameObject parent)
            {
                var growth = parent.GetComponent<TreeComponent>().Growth;
                growth.Percentage = InitialGrowthPercentage;
                parent.Body.Scale = growth.Percentage;
            }
        }

        public Progress GrowthNew = new(0, 100, 5);
        int GrowthTick;
        int GrowthRate = Ticks.PerSecond;
        const float InitialGrowthPercentage = .05f;
        public Growth Growth = new Growth(100);
        public void FinishGrowing(GameObject parent)
        {
            this.Growth.Set(parent, this.Growth.Max);
        }
        public override string Name { get; } = "Tree"; 
        public override object Clone()
        {
            return new TreeComponent();
        }

        public TreeComponent()
        {

        }
        public TreeComponent(float initialGrowth)
        {
            this.GrowthNew.Percentage = initialGrowth;
        }
        public override void MakeChildOf(GameObject parent)
        {
            parent.Body.ScaleFunc = () => .25f + .75f * this.GrowthNew.Percentage;
        }

        public override void Tick()
        {
            var parent = this.Parent;
            if (this.GrowthNew.IsFinished)
                return;
            this.GrowthTick++;
            if (this.GrowthTick >= this.GrowthRate)
            {
                this.GrowthTick = 0;
                this.GrowthNew.Value++;
            }
        }

        public override void Write(System.IO.BinaryWriter w)
        {
            this.GrowthNew.Write(w);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.GrowthNew.Read(r);
        }
        internal override void SaveExtra(SaveTag tag)
        {
            tag.Add(this.GrowthNew.Save("GrowthNew"));
        }
        internal override void LoadExtra(SaveTag tag)
        {
            tag.TryGetTag("GrowthNew", t => this.GrowthNew = new Progress(t));
        }
        static public bool IsGrown(GameObject obj)
        {
            var comp = obj.GetComponent<TreeComponent>();
            return (comp != null && comp.Growth.IsFinished);
        }
        internal override void SyncWrite(System.IO.BinaryWriter w)
        {
            w.Write(this.Growth.Value);
        }
        internal override void SyncRead(GameObject parent, System.IO.BinaryReader r)
        {
            this.Growth.Set(parent, r.ReadInt32());
        }
        
        internal override void GetSelectionInfo(IUISelection info, GameObject parent)
        {
            info.AddInfo(new Bar(this.GrowthNew) { Color = Color.MediumAquamarine, Name = "Growth: ", TextFunc = () => this.GrowthNew.Percentage.ToString("##0%") });
        }
        public override void OnTooltipCreated(GameObject parent, Control tooltip)
        {
            tooltip.Controls.Add(new Bar()
            {
                Width = 200,
                Name = "Growth: ",
                Location = tooltip.Controls.BottomLeft,
                Object = this.GrowthNew,
                TextFunc = () => this.GrowthNew.Percentage.ToString("##0%")
            });
        }

        public class Props : ComponentProps
        {
            public override Type CompClass => typeof(TreeComponent);
        }
    }
}

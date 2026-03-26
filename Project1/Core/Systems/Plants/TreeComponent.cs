using Microsoft.Xna.Framework;
using Project1.Framework;
using Project1.Framework.UI;
using Project1.Framework.Serialization;
using Project1.Framework.Helpers;
using Project1.Core.Entities;

namespace Project1.Core.Systems.Plants
{
    class TreeComponent : EntityComp
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
        public override EntityCompDef CompDef => EntityCompDefOf.Tree;

        public ProgressFloat GrowthNew = new(0, 100, 5);
        int GrowthTick;
        int GrowthRate = Ticks.PerSecond;
        const float InitialGrowthPercentage = .05f;
        public Growth Growth = new Growth(100);
        public void FinishGrowing(GameObject parent)
        {
            this.Growth.Set(parent, this.Growth.Max);
        }
        public override string Name { get; } = "Tree";
        public TreeComponent()
        {

        }
        public TreeComponent(float initialGrowth)
        {
            this.GrowthNew.Percentage = initialGrowth;
        }
        internal override void Resolve()
        {
            this.Owner.Body.ScaleFunc = () => .25f + .75f * this.GrowthNew.Percentage;
        }

        public override void Tick()
        {
            var parent = this.Owner;
            if (this.GrowthNew.IsFinished)
                return;
            this.GrowthTick++;
            if (this.GrowthTick >= this.GrowthRate)
            {
                this.GrowthTick = 0;
                this.GrowthNew.Value++;
            }
        }

        public override void Write(IDataWriter w)
        {
            this.GrowthNew.Write(w);
        }
        public override void Read(IDataReader r)
        {
            this.GrowthNew.Read(r);
        }
        internal override void SaveExtra(SaveTag tag)
        {
            tag.Add(this.GrowthNew.Save("GrowthNew"));
        }
        internal override void LoadExtra(SaveTag tag)
        {
            tag.TryGetTag("GrowthNew", t => this.GrowthNew = new ProgressFloat(t));
        }
        static public bool IsGrown(GameObject obj)
        {
            var comp = obj.GetComponent<TreeComponent>();
            return (comp != null && comp.Growth.IsFinished);
        }
        internal override void SyncWrite(IDataWriter w)
        {
            w.Write(this.Growth.Value);
        }
        internal override void SyncRead(GameObject parent, IDataReader r)
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
        public new class Spec : Spec<TreeComponent> { }
    }
}
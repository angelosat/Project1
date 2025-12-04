using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Start_a_Town_
{
    public class MoodComp : EntityComponent, IProgressBar
    {
        readonly ObservableCollection<Moodlet> Moodlets = new();
        const float BaseMood = 50;
        public float Mood = BaseMood;
        readonly float Rate = 1f;
        private int TicksRemaining = Ticks.PerSecond;
        private readonly int TicksDelay = Ticks.PerSecond;

        private float ValueTarget => BaseMood + this.Moodlets.Sum(m => m.Def.Value);

        public override string Name { get; } = "Mood";
        public float Min => 0;

        public float Max => 100;

        public float Value => this.Mood;

        public float Percentage => this.Value / this.Max;

        public override void Tick()
        {
            var parent = this.Parent;

            if (this.TicksRemaining <= 0)
            {
                var target = this.ValueTarget;
                var resilience = StatDefOf.MoodChangeRate.GetValue(parent);
                var rate = this.Rate;
                if (target < this.Mood)
                    this.Mood = Math.Max(this.Mood - rate, target);
                else if (target > this.Mood)
                    this.Mood = Math.Min(this.Mood + rate, target);
                this.TicksRemaining = (int)Math.Round(this.TicksDelay * resilience);
            }
            this.TicksRemaining--;
            var actor = parent as Actor;
            foreach (var m in MoodletDef.All)
                m.TryAssignOrRemove(actor);
            var count = this.Moodlets.Count;
            var nextMoodlets = new List<Moodlet>(this.Moodlets);
            for (int i = 0; i < count; i++)
            {
                var m = nextMoodlets[i];
                var result = m.Tick(actor);
                if (!result)
                    this.Moodlets.Remove(m);
            }
        }

        public void Add(Moodlet m)
        {
            this.Moodlets.Add(m);
        }
        public void Remove(MoodletDef mdef)
        {
            this.Moodlets.Remove(this.Moodlets.First(m => m.Def == mdef));
        }
        public bool Contains(MoodletDef mdef)
        {
            return this.Moodlets.Any(m => m.Def == mdef);
        }
        public override object Clone()
        {
            return new MoodComp();
        }

        internal override void GetInterface(GameObject actor, Control box)
        {
            var panelMoodValue = new PanelLabeled("Mood");
            var bar = new Bar(this);
            bar.ColorFunc = () => Color.Lerp(Color.Red, Color.Lime, this.Mood / 100f);
            bar.HoverFunc = () => this.Mood.ToString();
            bar.NameFunc = () => this.Percentage.ToString("##0%");
            bar.OnDrawAction = (sb, bounds) =>
            {
                var indicator = new Rectangle(bounds.X + (int)Math.Round(bounds.Width * this.ValueTarget / this.Max), bounds.Y, 1, bounds.Height);
                indicator.DrawHighlight(sb);
            };

            panelMoodValue.AddControlsBottomLeft(bar);

            var panelMoodlets = new TableObservable<Moodlet>() { BackgroundStyle = BackgroundStyle.TickBox };
            panelMoodlets.AddColumn("Test", panelMoodValue.Width * 2, m => m.GetUI());
            panelMoodlets.Bind(this.Moodlets);
            //panelMoodlets.Build(this.Moodlets, false);

            panelMoodlets.Location = panelMoodValue.BottomLeft;
            box.AddControls(panelMoodValue, panelMoodlets);

            //panelMoodlets.OnGameEventAction = (e) =>
            //{
            //    if (e.Type == Message.Types.MoodletsUpdated && e.Parameters[0] == actor)
            //    {
            //        panelMoodlets.Build(this.Moodlets, false);
            //    }
            //};
        }

        internal override void SaveExtra(SaveTag tag)
        {
            this.Moodlets.SaveAsList(tag, "Moodlets");
            tag.Add(this.Mood.Save("Value"));
        }
        internal override void LoadExtra(SaveTag save)
        {
            this.Moodlets.LoadNewNew(save, "Moodlets");
            save.TryGetTagValueNew("Value", ref this.Mood);
        }
        public override void Write(BinaryWriter w)
        {
            this.Moodlets.Write(w);
            w.Write(this.Mood);
        }
        public override void Read(IDataReader r)
        {
            this.Moodlets.Read(r);
            this.Mood = r.ReadSingle();
        }
    }
}

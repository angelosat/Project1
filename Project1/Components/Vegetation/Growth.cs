using System.IO;

namespace Start_a_Town_
{
    public class Growth : Progress
    {
        const int InitialGrowth = 10;
        const float GrowthMax = 100;
        public int GrowthTimer = 0;

        public Growth()
            : base(0, GrowthMax, InitialGrowth)
        {
        }
        public Growth(float initialGrowthPercentage)
            : base(0, GrowthMax, 0)
        {
            this.Percentage = initialGrowthPercentage;
        }
        public void Update(GameObject parent)
        {
            if (this.IsFinished)
                return;
            this.GrowthTimer++;
            if (this.GrowthTimer == Ticks.PerSecond)
            {
                this.GrowthTimer = 0;
                this.Value++;
                parent.Body.Scale = this.Percentage;
            }
        }
        public override string ToString()
        {
 	         return string.Format("Growth: {0:P0}", this.Percentage);
        }
        
        static public Growth Create(SaveTag tag)
        {
            var gr = new Growth();
            gr.Load(tag);
            return gr;
        }
        public Growth(SaveTag tag)
        {
            this.Load(tag);
        }
        static public Growth Create(IDataReader r)
        {
            var gr = new Growth();
            gr.Read(r);
            return gr;
        }

        internal void Set(GameObject parent, float value)
        {
            this.Value = value;
            parent.Body.Scale = this.Percentage;
        }
    }
}

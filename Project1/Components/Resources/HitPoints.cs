using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class HitPoints : ResourceWorker
    {
        public HitPoints(ResourceDef def) : base(def)
        {
            this.AddThreshold("Hit points");
        }

        private const string _description = "Hit Points";
        private const string _format = "##0";
        public override string Format => _format;
        public override string Description => _description; 

        public override Color GetBarColor(Resource resource)
        {
            return Color.SeaGreen;
        }
    }
}

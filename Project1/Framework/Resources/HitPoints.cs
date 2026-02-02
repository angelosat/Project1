using Microsoft.Xna.Framework;

namespace Project1.Framework.Resources
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
        protected override void OnDepleted(Resource res)
        {
            var entity = res.Owner;
            entity.Kill();
        }
        public override Color GetBarColor(Resource resource)
        {
            return Color.SeaGreen;
        }
    }
}

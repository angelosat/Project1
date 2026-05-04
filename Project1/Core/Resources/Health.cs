using Microsoft.Xna.Framework;
using Project1.Core.Animations;
using Project1.Core.Entities;
using Project1.Core.Rendering;
using Project1.Core.UI.NamePlates;
using Project1.Framework.UI;
using Project1.Framework.UI.Primitives;
using System;
using System.Linq;

namespace Project1.Core.Resources
{
    sealed class Health : ResourceWorker
    {
        public Health(ResourceDef def) : base(def)
        {
            this.AddThreshold("Dying", .25f);
            this.AddThreshold("Critical", .5f);
            this.AddThreshold("Injured", .75f);
            this.AddThreshold("Healthy", 1f);
        }
        public override string Format { get; } = "##0.00";
        public override string Description { get; } = "Basic health resource";

        public float TickRate = Ticks.PerSecond / 2f;

        float SpriteFlashTimer;
        protected override void OnDepleted(ResourceRuntime res)
        {
            var entity = res.Owner;
            entity.Kill();
        }
        protected override void TickExtra(ResourceRuntime values)
        {
            FlashSprite(values.Owner);
        }
        protected override void updateRec(ResourceRuntime resource)
        {
            if (resource.RechargingDelay.Value > 0)
            {
                resource.RechargingDelay.Value--;
                return;
            }
        }
        private void FlashSprite(GameObject parent)
        {
            if (this.SpriteFlashTimer > 0)
            {
                this.SpriteFlashTimer--;
                if (this.SpriteFlashTimer <= 0)
                {
                    parent.TryGetComponent<SpriteComp>(t => t.Tint = Color.White);

                }
            }
        }
        protected override float GetRegenRate(ResourceRuntime values)
        {
            return 0;
            float rate = ((float)Math.Pow(values.Percentage, 2)) / TickRate;

            return rate;
        }
        public override void OnHealthBarCreated(GameObject parent, Nameplate plate, ResourceRuntime values)
        {
            plate.AlwaysShow = true;
            var bar = new BarImmediate()
            {
                Location = plate.Controls.Last().BottomLeft,
                Width = 50,
                Height = 3,
                MouseThrough = true,
                ColorFunc = () => Color.Lerp(Color.Red, Color.Lime, values.Percentage),
                Tag = values,
                Object = values
            };
            plate.AddControls(bar);
            plate.SetMousethrough(true, true);
        }
        public override void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Renderer camera, GameObject parent)
        {
            base.DrawUI(sb, camera, parent);
        }
        public override Color GetBarColor(ResourceRuntime resource)
        {
            return Color.Orange;
        }

    }
}
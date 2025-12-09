using Microsoft.Xna.Framework;
using SharpDX.Direct3D9;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.Components.Resources
{
    class Health : ResourceWorker
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

        protected override void TickExtra(Resource values)
        {
            FlashSprite(values.Parent);
        }
        protected override void updateRec(Resource resource)
        {
            if (resource.Rec.Value > 0)
            {
                resource.Rec.Value--;
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
                    parent.TryGetComponent<SpriteComponent>(t => t.Tint = Color.White);

                }
            }
        }
       
        protected override float GetRegenRate(Resource values)
        {
            float rate = ((float)Math.Pow(values.Percentage, 2)) / TickRate;

            return rate;
        }

        //public override bool HandleMessage(Resource resource, GameObject parent, ObjectEventArgs e = null)
        //{
        //    switch (e.Type)
        //    {
        //        case Message.Types.HitGround:
        //            float zForce = (float)e.Parameters[0];
        //            this.HitGround(resource, parent, zForce);
        //            return true;

        //        default:
        //            return base.HandleMessage(resource, parent, e);
        //    }
        //}

        //internal override void HandleRemoteCall(GameObject parent, ObjectEventArgs e, Resource values)
        //{
        //    switch (e.Type)
        //    {
        //        case Message.Types.HitGround:
        //            e.Data.Translate(parent.Net, r =>
        //            {
        //                float zForce = r.ReadSingle();
        //                HitGround(values, parent, zForce);
        //            });
        //            break;

        //        default: break;
        //    }
        //}


        private void HandleEntityHitGround(EntityHitGroundEvent e)
        {
            var actor = e.Entity as Actor;
            if (actor is null)
                return;
            var force = e.Force;
            var health = actor.GetResource(ResourceDefOf.Health);
            if (force > 1)
            {
                this.Modify(health, force);
                actor.Net.EventOccured((int)Message.Types.HealthLost, actor, (int)force);
            }
        }
        public override IEnumerable<(Type eventType, Action<EventPayloadBase> handler)> GetInterests()
        {
            yield return (typeof(EntityHitGroundEvent),e => this.HandleEntityHitGround((EntityHitGroundEvent)e));
        }
        //private void HitGround(Resource resource, GameObject parent, float zForce)
        //{
        //    float value = zForce * 0;
        //    if (value >= -3)
        //        return;
        //    value += 2;
        //    this.Modify(resource, value);
        //    parent.Net.EventOccured((int)Message.Types.HealthLost, parent, (int)value);
        //}

        public override void OnHealthBarCreated(GameObject parent, UI.Nameplate plate, Resource values)
        {
            plate.AlwaysShow = true;
            //plate.Controls.Add(new Label()
            //{
            //    Text = parent.Name,
            //    MouseThrough = true,
            //});
            var bar = new Bar()
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


        public override void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera, GameObject parent)
        {
            base.DrawUI(sb, camera, parent);
        }

        public override Color GetBarColor(Resource resource)
        {
            return Color.Orange;
        }
    }
}

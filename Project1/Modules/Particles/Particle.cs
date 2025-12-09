using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.Particles
{
    public class Particle
    {
        public Texture2D Texture = Block.ParticlePixel.Atlas.Texture;
        public Rectangle SourceRectangle = Block.ParticlePixel.Rectangle;

        public Vector3 Offset, Velocity;
        public float Lifetime, LifetimeMax;
        public Func<Particle, Color> ColorFunc = p => Color.White;
        public Func<Particle, float> ScaleFunc = p => 2;
        public Func<Particle, float> AlphaFunc = p => p.LifePercentage;

        public float LifePercentage { get { return this.Lifetime / this.LifetimeMax; } }

        public Particle(Vector3 startOffset, Vector3 startVelocity, float life)
        {
            this.Offset = startOffset;
            this.Velocity = startVelocity;
            this.Lifetime = this.LifetimeMax = life;
        }

        public void Update()
        {
            this.Lifetime--;
        }

        public void Draw(Camera cam, MySpriteBatch sb, MapBase map, Vector3 global)
        {
            var transformedGlobal = this.Offset + global;
            if (transformedGlobal.Z > cam.MaxDrawZ + 1)
                return;
            var rounded = transformedGlobal.ToRounded();
            map.GetLight(transformedGlobal, out byte skylight, out byte blocklight);
            var skyColor = map.GetAmbientColor() * ((skylight + 1) / 16f);
            skyColor.A = 255;
            var blockColorVector = Vector4.Lerp(new Vector4(0, 0, 0, 1), Vector4.One, (blocklight) / 15f);

            var screenpos = cam.GetScreenPositionFloat(transformedGlobal);
            var alpha = this.AlphaFunc(this);
            var scale = this.ScaleFunc(this);
            var color = this.ColorFunc(this);
            var depth = transformedGlobal.GetDrawDepth(map, cam);
            var finalscale = new Vector2(cam.Zoom) * scale;
            var finalcolor = color * alpha;
            var origin = new Vector2(this.SourceRectangle.Width, this.SourceRectangle.Height) / 2f;
            sb.Draw(
                this.Texture, screenpos, this.SourceRectangle,
                0, origin, finalscale,
                skyColor, blockColorVector,
                finalcolor, Color.Transparent,
                SpriteEffects.None, depth);
        }
    }
}

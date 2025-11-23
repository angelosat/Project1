using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components.Physics;
using Start_a_Town_.Net;
using Start_a_Town_.Components;

namespace Start_a_Town_.Particles
{
    public abstract class ParticleEmitter : ICloneable
    {
        public List<Particle> Particles = new List<Particle>();
        public Random Random = new Random();
        public float Radius;
        public Vector3 Offset = Vector3.Zero;
        public float ParticleWeight;
        public float Lifetime = Ticks.PerSecond;
        public float Friction = 0.5f;
        int _Rate = 1;
        public int Rate
        {
            get
            {
                return this.RateFunc?.Invoke() ?? this._Rate;
            }
            set
            {
                this._Rate = value;
            }
        }
        public Func<int> RateFunc;

        public Color ColorBegin = Color.White, ColorEnd = Color.White;
        public float AlphaBegin = 1, AlphaEnd = 0;
        public float SizeBegin = 1, SizeEnd = 1;
        public float SizeVariance = 0;
        public bool HasPhysics = true;
        public Vector3 Acceleration = Vector3.One;

        public ParticleEmitter SetRateFunc(Func<int> ratefunc)
        {
            this.RateFunc = ratefunc;
            return this;
        }

        public Texture2D Texture = Block.ParticlePixel.Atlas.Texture;
        public Rectangle SourceRectangle = Block.ParticlePixel.Rectangle;

        public Vector3 Source;

        public ParticleEmitter(Vector3 offset, float lifetime, float radius = 0, float particleWeight = 0)
        {
            this.Lifetime = lifetime;
            this.Radius = radius;
            this.Offset = offset;
            this.ParticleWeight = particleWeight;
        }
        public ParticleEmitter(float radius = 0)
        {
            this.Radius = radius;
        }

        /// <summary>
        /// Update particles with physics
        /// </summary>
        /// <param name="map"></param>
        /// <param name="global"></param>
        public void Update(MapBase map, Vector3 global)
        {
            if (map.Net is Server)
                throw new Exception();
            if (!HasPhysics)
            {
                this.Update(map);
                return;
            }
            foreach (var p in this.Particles.ToList())
            {
                if (p.Lifetime <= 0)
                    this.Particles.Remove(p);
                    var pos = global + this.Offset + p.Offset;
                    Vector3 nextpos, nextvel;
                    Physics.Update(this.ParticleWeight, this.Friction, map, pos, p.Velocity*this.Acceleration, out nextpos, out nextvel);
                    p.Offset = nextpos - global - this.Offset;
                    p.Velocity = nextvel;
             
                p.Update();
            }

            Emit(this.Rate);
        }
        /// <summary>
        /// Update particles without physics
        /// </summary>
        public void Update(MapBase map)
        {
            Vector3 adjustment;
            foreach (var p in this.Particles.ToList())
            {
                if (p.Lifetime <= 0)
                    this.Particles.Remove(p);
                adjustment = new Vector3(0, 0, map.Gravity * this.ParticleWeight);
                p.Velocity += adjustment;
                p.Velocity *= this.Acceleration;
                p.Offset += p.Velocity;
                p.Update();
            }
            Emit(this.Rate);
        }
        public void Emit(int count)
        {
            this.Emit(count, Vector3.Zero);
        }
        public void Emit(int count, Vector3 force)
        {
            count = (int)(count * ParticleDensityLevel.Current.Factor);
            for (int i = 0; i < count; i++)
            {
                var startVelocity = this.GetStartVelocity();
                var startOffset = this.GetStartOffset();
                var scalevariance = (float)this.Random.NextDouble() * this.SizeVariance * 2;
                scalevariance -= (this.SizeVariance / 2f);

                var particle = new Particle(startOffset, startVelocity + force, this.Lifetime)
                {
                    ScaleFunc = p =>
                    {
                        return (this.SizeBegin + scalevariance) * p.LifePercentage + (this.SizeEnd + scalevariance) * (1 - p.LifePercentage);
                    },
                    ColorFunc = p => Color.Lerp(this.ColorEnd, this.ColorBegin, p.LifePercentage),
                    Texture = this.Texture,
                    SourceRectangle = this.SourceRectangle,
                    AlphaFunc = p => AlphaBegin * p.LifePercentage + AlphaEnd * (1 - p.LifePercentage)
                };

                this.Particles.Add(particle);
                if (float.IsNaN(particle.Velocity.X) || float.IsNaN(particle.Velocity.Y))
                    throw new Exception();
            }
        }
        public void Emit(Texture2D texture, List<Rectangle> pieces, Vector3 force)
        {
            this.Texture = texture;
            foreach(var rect in pieces)
            {
                var startVelocity = this.GetStartVelocity();
                var startOffset = this.GetStartOffset();
                var scalevariance = (float)this.Random.NextDouble() * this.SizeVariance * 2;
                scalevariance -= (this.SizeVariance / 2f);

                var particle = new Particle(startOffset, startVelocity + force, this.Lifetime)
                {
                    ScaleFunc = p =>
                    {
                        return (this.SizeBegin + scalevariance) * p.LifePercentage + (this.SizeEnd + scalevariance) * (1 - p.LifePercentage);
                    },
                    ColorFunc = p => Color.Lerp(this.ColorEnd, this.ColorBegin, p.LifePercentage),
                    Texture = texture,
                    SourceRectangle = rect,
                    AlphaFunc = p => AlphaBegin * p.LifePercentage + AlphaEnd * (1 - p.LifePercentage)
                };

                this.Particles.Add(particle);
            }
        }
        protected abstract Vector3 GetStartVelocity();
        
        Vector3 GetStartOffset()
        {
            double θ = this.Random.NextDouble() * (Math.PI + Math.PI);
            var r = (float)this.Random.NextDouble() * this.Radius;
            float x = (float)Math.Cos(θ);
            float y = (float)Math.Sin(θ);
            var startOffset = new Vector3(x, y, 0) * r;
            return startOffset;
        }

       
        public void Draw(Camera cam, MapBase map, Vector3 global)
        {
            // TODO: slow if many emmiters
            // why do i have to sort?
            //SortParticlesByDepth(cam, map);

            // TODO: find better way to do this OPTIMIZE FIX
            if (this.Texture == Sprite.Atlas.Texture)
                foreach (var particle in this.Particles)
                    particle.Draw(cam, cam.ParticlesSpriteBatch, map, global + this.Offset);
            else if (this.Texture == Block.Atlas.Texture)
                foreach (var particle in this.Particles)
                    particle.Draw(cam, cam.BlockParticlesSpriteBatch, map, global + this.Offset);
        }
        private void SortParticlesByDepth(Camera cam, MapBase map)
        {
            this.Particles.Sort((p1, p2) =>
            {
                var d1 = p1.Offset.GetDrawDepth(map, cam);
                var d2 = p2.Offset.GetDrawDepth(map, cam);
                if (d1 < d2)
                    return -1;
                if (d1 > d2)
                    return 1;
                return 0;
            });
        }

        public abstract object Clone();

        static public ParticleEmitterSphere Fire
        {
            get
            {
                return new ParticleEmitterSphere()
                {
                    Offset = new Vector3(0, 0, .5f),
                    Lifetime = Ticks.PerSecond,
                    Radius = .2f,
                    ParticleWeight = -.1f,
                    Force = .01f,
                    Friction = .5f,
                    ColorBegin = Color.Yellow,
                    ColorEnd = Color.Red,
                    SizeBegin = 3,
                    SizeEnd = 1,
                    HasPhysics = false
                };
            }
        }
        static public ParticleEmitterSphere Dust
        {
            get
            {
                var emitter = new ParticleEmitterSphere() 
                {
                    Lifetime = Ticks.PerSecond / 2f,
                    Offset = Vector3.Zero,
                    Rate = 0,
                    ParticleWeight = 0f,//1f,
                    ColorEnd = Color.White * .5f,
                    ColorBegin = Color.White,
                    SizeEnd = 1,
                    SizeBegin = 3,
                    Force = .01f,
                    Friction = 0f
                };
                return emitter;
            }
        }
    }
}

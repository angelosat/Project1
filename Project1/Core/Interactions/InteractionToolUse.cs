using Microsoft.Xna.Framework;
using Project1.Core.Stats;
using Project1.Core.Graphics.Particles;
using System;
using System.Collections.Generic;
using Project1.Framework.Helpers;

namespace Project1.Core.Interactions
{
    abstract class InteractionToolUse : InteractionPerpetual
    {
        protected ParticleEmitterSphere EmitterStrike;
        protected List<Rectangle> ParticleRects;
        protected virtual Progress ProgressNew { get; }
        protected InteractionToolUse(string name) : base(name)
        {
            this.DrawProgressBar(() => this.Actor.Global, () => this.ProgressPercentage, () => this.Name);
        }
        protected void OnStart()
        {
            var a = this.Actor;
            var t = this.Target;
            this.CachedAnimation.Speed = StatDefOf.WorkSpeed.CalculateFor(a);
            var particleColor = this.GetParticleColor();
            this.EmitterStrike = new ParticleEmitterSphere
            {
                Source = t.Global + Vector3.UnitZ,
                SizeBegin = 1,
                SizeEnd = 1,
                ParticleWeight = 1,
                Radius = 1f,// .5f;
                Force = .1f,
                Friction = .5f,
                AlphaBegin = 1,
                AlphaEnd = 0,
                ColorBegin = particleColor,
                ColorEnd = particleColor,
                Lifetime = Ticks.PerSecond * 2,
                Rate = 0
            };
            this.ParticleRects = GetParticleRects();
            this.Init();

        }
        bool WillFinish(int amount) => this.Def.Logic.WillFinish(this.Context, amount);
        
        protected virtual void Init() { }
        [Obsolete($"use {nameof(this.OnAddProgress)}")]
        protected virtual void OnApplyWork(int workAmount) { }
        protected abstract List<Rectangle> GetParticleRects();
        protected abstract Color GetParticleColor();
        
    }
}
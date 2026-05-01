using System.Collections.Generic;
using Project1.Core.Entities;
using Project1.Core.Screens;

namespace Project1.Core.Graphics.Particles
{
    class ParticlesComponent : EntityComp
    {
        public override EntityCompDef CompDef => EntityCompDefOf.Particles;
        public override string Name { get; } = "ParticleSystem"; 

        List<ParticleEmitter> Emitters = new List<ParticleEmitter>();
        public ParticlesComponent()
        {
        }
       
        public ParticlesComponent(params ParticleEmitter[] emitters)
        {
            this.Emitters.AddRange(emitters);
        }
        public override void Tick()
        {
            var parent = this.Owner;

            foreach (var emitter in this.Emitters)
                emitter.Update(parent.Map, parent.Global);
        }
        public override void Draw(MySpriteBatch sb, RenderContext ctx)
        {
            var pos = this.Owner.Global;
            foreach (var emitter in this.Emitters)
                emitter.Draw(ctx, pos);
        }
    }
}
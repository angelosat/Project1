using Project1.Core.Blocks;
using Project1.Core.Blocks.Comps;
using Project1.Core.Graphics.Particles;
using Project1.Core.Simulation;
using Project1.Framework;
using System;
using System.Collections.Generic;

namespace Project1.Core
{
    class BlockEntityCompParticles : BlockComp
    {
        public override BlockCompDef CompDef => throw new NotImplementedException();

        public override string Name { get; } = "Particles";
        readonly HashSet<ParticleEmitter> Emitters = new();
        public BlockEntityCompParticles(params ParticleEmitter[] emitters)
        {
            for (int i = 0; i < emitters.Length; i++)
            {
                this.AddEmitter(emitters[i]);
            }
        }
        
        public void AddEmitter(ParticleEmitter emitter)
        {
            this.Emitters.Add(emitter);
        }
        
        public override void Tick()
        {
            foreach (var e in this.Emitters)
                e.Update(this.Parent.Map);
        }
        public override void Draw(Camera camera, MapBase map, IntVec3 global)
        {
            foreach (var e in this.Emitters)
                e.Draw(camera, map, global);
        }
    }
}

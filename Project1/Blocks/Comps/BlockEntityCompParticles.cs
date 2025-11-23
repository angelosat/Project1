using System.Collections.Generic;
using Start_a_Town_.Particles;

namespace Start_a_Town_
{
    class BlockEntityCompParticles : BlockEntityComp
    {
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

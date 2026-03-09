using Project1.Core.Graphics.Particles;
using Project1.Core.Simulation;
using Project1.Core.VFX;
using System;
using System.Collections.Generic;

namespace Project1.Core.Blocks.Comps
{
    public class BlockParticlesComp : BlockComp
    {
        public new class Spec(ParticleEmitter emitterType) : BlockComp.Spec
        {
            readonly ParticleEmitter EmitterType = emitterType;
            public override Type CompType => typeof(BlockParticlesComp);
            public override BlockParticlesComp CreateComp() => new(this.EmitterType.Clone() as ParticleEmitter);// { Emitters = [] };
        }
        public override BlockCompDef CompDef => BlockCompDefOf.Particles;

        readonly List<ParticleEmitter> Emitters = [];
        public BlockParticlesComp(params ParticleEmitter[] emitters)
        {
            foreach(var e in emitters)
                this.Emitters.Add(e);
        }
        internal override void OnSpawned(BlockEntity entity, MapBase map)
        {
            map.Events.Post(new BlockParticlesEvent(this, [..this.Emitters], []));
        }
        internal override void OnDespawned(BlockEntity entity, MapBase map)
        {
            map.Events.Post(new BlockParticlesEvent(this, [], [..this.Emitters]));
        }
        internal override void OnSwitched(bool on)
        {
            if(on)
                this.Map.Events.Post(new BlockParticlesEvent(this, [.. this.Emitters], []));
            else
                this.Map.Events.Post(new BlockParticlesEvent(this, [], [.. this.Emitters]));
        }
        //public override void Tick()
        //{
        //    foreach (var e in this.Emitters)
        //        e.Update(this.Parent.Map);
        //}
        //public override void Draw(Camera camera, MapBase map, IntVec3 global)
        //{
        //    foreach (var e in this.Emitters)
        //        e.Draw(camera, map, global);
        //}
    }
}

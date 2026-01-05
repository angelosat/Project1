using Start_a_Town_.Particles;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal static class VFXParticles
    {
        static VFXParticles()
        {
            Registry.MapEventHooksClient.Register<BlockHitEvent>(OnBlockHit);
            Registry.MapEventHooksClient.Register<BlockDestroyedEvent>(OnBlockDestroyed);

        }
        private static void OnBlockDestroyed(BlockDestroyedEvent e)
        {
            EmitBlockParticles(e.Block, e.Map, e.Global);

        }
        private static void OnBlockHit(BlockHitEvent e)
        {
            EmitBlockParticles(e.Block, e.Map, e.Global);
        }

        private static void EmitBlockParticles(Block block, MapBase map, IntVec3 global)
        {
            var emitter = new ParticleEmitterSphere
            {
                Source = global + IntVec3.UnitZ,
                SizeBegin = 1,
                SizeEnd = 1,
                ParticleWeight = 1,
                Radius = 1f,// .5f;
                Force = .1f,
                Friction = .5f,
                AlphaBegin = 1,
                AlphaEnd = 0,
                //ColorBegin = particleColor,
                //ColorEnd = particleColor,
                Lifetime = Ticks.PerSecond * 2,
                Rate = 0
            };

            //var emitter = block.GetEmitter();
            emitter.Texture = Block.Atlas.Texture;
            var rects = block.GetParticleRects(25);
            emitter.Emit(rects, Vector3.Zero);
            map.ParticleManager.AddEmitter(emitter);
        }
    }
}

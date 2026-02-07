using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Interactions;
using Project1.Core.Base;
using Project1.Core.Graphics.Particles;
using Project1.Core.Assets;
using Project1.Core.Helpers;
using Project1.Core.Simulation;

namespace Project1.Core.VFX
{
    [EnsureStaticCtorCall]
    internal static class VFXParticles
    {
        static VFXParticles()
        {
            Registry.MapEventHooksClient.Register<BlockHitEvent>(OnBlockHit);
            Registry.MapEventHooksClient.Register<BlockDestroyedEvent>(OnBlockDestroyed);
            Registry.MapEventHooksClient.Register<PlantChoppedEvent>(OnPlantChopped);
        }

        private static void OnPlantChopped(PlantChoppedEvent e)
        {
            var actor = e.Actor;
            var plant = e.Target.Object;
            var intensity = e.Intensity;
            var global = plant.Global;
            var rects = ItemContent.LogsGrayscale.AtlasToken.Rectangle.Divide(25);
            var emitter = NewEmitter(global);
            emitter.Texture = ItemContent.LogsGrayscale.Texture;
            emitter.ColorBegin = plant.PrimaryMaterial.Color;
            emitter.ColorEnd = plant.PrimaryMaterial.Color;
            emitter.Emit(rects, Vector3.Zero);
            plant.Map.ParticleManager.AddEmitter(emitter);
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
            var emitter = NewEmitter(global);
            emitter.Texture = Block.Atlas.Texture;
            var rects = block.GetParticleRects(25);
            emitter.Emit(rects, Vector3.Zero);
            map.ParticleManager.AddEmitter(emitter);
        }

        private static ParticleEmitterSphere NewEmitter(IntVec3 global)
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
            return emitter;
        }
    }
}

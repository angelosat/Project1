using Microsoft.Xna.Framework;
using Project1.Core.Assets;
using Project1.Core.Blocks;
using Project1.Core.Blocks.Comps;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Graphics.Particles;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Core.Systems.Crafting;
using Project1.Core.Systems.Effects;
using Project1.Core.Systems.Magic;
using Project1.Core.Systems.Plants;
using Project1.Core.Systems.Presentation;
using Project1.Core.Systems.Quality;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Helpers;

namespace Project1.Core.Systems.Particles;

public record struct BlockParticlesEvent(BlockParticlesComp Comp, ParticleEmitter[] Added, ParticleEmitter[] Removed) : IEventPayload { }
internal sealed class PresentationParticles : IPresentationWorker
{
    public void Register()
    {
        Registry.MapEventHooksClient.Register<BlockDamagedEvent>(OnBlockDamaged);
        Registry.MapEventHooksClient.Register<PlantChoppedEvent>(OnPlantChopped);
        Registry.MapEventHooksClient.Register<ActorFootStepEvent>(OnEntityFootStep);
        Registry.MapEventHooksClient.Register<BlockParticlesEvent>(OnBlockParticles);
        Registry.MapEventHooksClient.Register<SpellCastEvent>(OnSpellCast);
        Registry.WorldEventHooksClient.Register<ActorEffectAppliedEvent>(OnEffectApplied);
        Registry.WorldEventHooksClient.Register<ActorFinishedCraftingEvent>(OnActorFinishedCrafting);
    }

    private void OnActorFinishedCrafting(ActorFinishedCraftingEvent e)
    {
        var resolver = Client.Instance.World;
        var actor = resolver.Get<Actor>(e.Actor);
        if (!actor.IsSpawned)
            return;
        var item = resolver.Get(e.Product);
        EmitSpellParticles(item, item.GetComponent<QualityComp>().Tier.Color);
    }

    private void OnEffectApplied(ActorEffectAppliedEvent e)
    {
        var actor = e.Actor;
        if (!actor.IsSpawned)
            return;
        if (e.Effect.Def.School is not SpellSchoolDef school)
            return;
        EmitSpellParticles(actor, school.Color);
    }

    private void OnSpellCast(SpellCastEvent e)
    {
        if (e.Target.Entity is not Entity entity)
            return;
        var school = e.Spell.School;
        EmitSpellParticles(entity, school.Color);
    }

    private static void EmitSpellParticles(Entity entity, Color color)
    {
        //var entity = e.Caster;
        var map = entity.Map;
        var emitter = NewEmitter(entity.Global + Vector3.UnitZ);
        emitter.HasPhysics = false;
        emitter.Radius = .5f;
        //emitter.Force = .2f;
        //emitter.Acceleration = Vector3.One * .8f;
        emitter.Force = .3f;
        emitter.Acceleration = Vector3.One * .7f;
        emitter.Source = entity.Global + Vector3.UnitZ;
        emitter.SizeBegin = emitter.SizeEnd = 4;
        emitter.ParticleWeight = -.5f;// -.1f;
        emitter.ColorBegin = emitter.ColorEnd = color;// Color.White;// Teal;
        emitter.Lifetime = Ticks.PerGameMinute;
        emitter.Emit(100);

        map.ParticleManager.AddEmitter(emitter);
    }

    private static void OnBlockParticles(BlockParticlesEvent e)
    {
        foreach (var em in e.Added)
        {
            em.Rate = 1;
            em.Source = e.Comp.Parent.OriginGlobal;
            e.Comp.Map.ParticleManager.AddEmitter(em);
        }
        foreach (var em in e.Removed)
            em?.Rate = 0;
    }

    private static void OnEntityFootStep(ActorFootStepEvent e)
    {
        var entity = e.Entity;
        var map = entity.Map;
        var query = map.Query(entity.Cell);
        var emitter = query.Cell.Block.GetEmitter();
        emitter.Source = entity.Global;
        emitter.Emit(10, entity.Velocity);
        entity.Map.ParticleManager.AddEmitter(emitter);
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

    //private static void OnBlockDestroyed(BlockDestroyedEvent e)
    //{
    //    EmitBlockParticles(e.Block, e.Map, e.Global);

    //}
    //private static void OnBlockHit(BlockHitEvent e)
    //{
    //    EmitBlockParticles(e.Block, e.Map, e.Global);
    //}
    private static void OnBlockDamaged(BlockDamagedEvent e)
    {
        var query = e.Map.Query(e.Cell);
        EmitBlockParticles(query.Cell.Block, e.Map, e.Cell);
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
//[EnsureStaticCtorCall]
//internal static class VFXParticles
//{
//    static VFXParticles()
//    {
//        Registry.MapEventHooksClient.Register<BlockDamagedEvent>(OnBlockDamaged);
//        Registry.MapEventHooksClient.Register<PlantChoppedEvent>(OnPlantChopped);
//        Registry.MapEventHooksClient.Register<ActorFootStepEvent>(OnEntityFootStep);
//        Registry.MapEventHooksClient.Register<BlockParticlesEvent>(OnBlockParticles);
//    }

//    private static void OnBlockParticles(BlockParticlesEvent e)
//    {
//        foreach (var em in e.Added)
//        {
//            em.Rate = 1;
//            em.Source = e.Comp.Parent.OriginGlobal;
//            e.Comp.Map.ParticleManager.AddEmitter(em);
//        }
//        foreach(var em in e.Removed)
//            em?.Rate = 0;
//    }

//    private static void OnEntityFootStep(ActorFootStepEvent e)
//    {
//        var entity = e.Entity;
//        var map = entity.Map;
//        var query = map.Query(entity.Cell);
//        var emitter = query.Cell.Block.GetEmitter();
//        emitter.Source = entity.Global;
//        emitter.Emit(10, entity.Velocity);
//        entity.Map.ParticleManager.AddEmitter(emitter);
//    }

//    private static void OnPlantChopped(PlantChoppedEvent e)
//    {
//        var actor = e.Actor;
//        var plant = e.Target.Object;
//        var intensity = e.Intensity;
//        var global = plant.Global;
//        var rects = ItemContent.LogsGrayscale.AtlasToken.Rectangle.Divide(25);
//        var emitter = NewEmitter(global);
//        emitter.Texture = ItemContent.LogsGrayscale.Texture;
//        emitter.ColorBegin = plant.PrimaryMaterial.Color;
//        emitter.ColorEnd = plant.PrimaryMaterial.Color;
//        emitter.Emit(rects, Vector3.Zero);
//        plant.Map.ParticleManager.AddEmitter(emitter);
//    }

//    //private static void OnBlockDestroyed(BlockDestroyedEvent e)
//    //{
//    //    EmitBlockParticles(e.Block, e.Map, e.Global);

//    //}
//    //private static void OnBlockHit(BlockHitEvent e)
//    //{
//    //    EmitBlockParticles(e.Block, e.Map, e.Global);
//    //}
//    private static void OnBlockDamaged(BlockDamagedEvent e)
//    {
//        var query = e.Map.Query(e.Cell);
//        EmitBlockParticles(query.Cell.Block, e.Map, e.Cell);
//    }
//    private static void EmitBlockParticles(Block block, MapBase map, IntVec3 global)
//    {
//        var emitter = NewEmitter(global);
//        emitter.Texture = Block.Atlas.Texture;
//        var rects = block.GetParticleRects(25);
//        emitter.Emit(rects, Vector3.Zero);
//        map.ParticleManager.AddEmitter(emitter);
//    }

//    private static ParticleEmitterSphere NewEmitter(IntVec3 global)
//    {
//        var emitter = new ParticleEmitterSphere
//        {
//            Source = global + IntVec3.UnitZ,
//            SizeBegin = 1,
//            SizeEnd = 1,
//            ParticleWeight = 1,
//            Radius = 1f,// .5f;
//            Force = .1f,
//            Friction = .5f,
//            AlphaBegin = 1,
//            AlphaEnd = 0,
//            //ColorBegin = particleColor,
//            //ColorEnd = particleColor,
//            Lifetime = Ticks.PerSecond * 2,
//            Rate = 0
//        };
//        return emitter;
//    }
//}

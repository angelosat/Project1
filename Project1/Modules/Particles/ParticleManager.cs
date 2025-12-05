using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Net;

namespace Start_a_Town_.Particles
{
    public class ParticleManager
    {
        // HashSet cause we dont want to add the same emitter twice
        readonly HashSet<ParticleEmitter> Emitters = new();
        readonly MapBase Map;
        public ParticleManager(MapBase map)
        {
            this.Map = map;
            this.Map.World.Events.ListenTo<EntityFootStepEvent>(this.EntityFootStep);
        }
        public void AddEmitter(ParticleEmitter emitter)
        {
            if (this.Map.Net is Net.Server)
                return;
            this.Emitters.Add(emitter);
        }
        public void Update()
        {
            foreach (var e in this.Emitters.ToList())
            {
                e.Update(this.Map, e.Source);
                if (e.Particles.Count == 0)
                    this.Emitters.Remove(e);
            }
        }
        public void Draw(Camera camera)
        {
            foreach (var e in this.Emitters)
                e.Draw(camera, this.Map, e.Source);
        }
        
        public void OnGameEvent(GameEvent e)
        {
            if (this.Map.Net is Server)
                return;
            switch ((Components.Message.Types)e.Type)
            {
                case Message.Types.EntityHitCeiling:
                    this.EntityHitCeiling(e);
                    break;

                case Message.Types.EntityHitGround:
                    this.EntityHitGround(e);
                    break;

                //case Message.Types.EntityFootStep:
                //    this.EntityFootStep(e);
                //    break;

                default:
                    break;
            }
        }

        void EntityHitGround(GameEvent e)
        {
            var entity = e.Parameters[0] as GameObject;
            var vector3 = (Vector3)e.Parameters[1];
            var block = entity.Map.GetBlock(vector3);
            var emitter = block.GetEmitter();
            emitter.Source = entity.Global;
            emitter.Emit(10);
            this.Emitters.Add(emitter);
        }

        void EntityHitCeiling(GameEvent e)
        {
            var entity = e.Parameters[0] as GameObject;
            var vector3 = (Vector3)e.Parameters[1];
            var block = entity.Map.GetBlock(vector3);
            if (block is null)
                return;
            var emitter = block.GetEmitter();
            emitter.Source = new Vector3(vector3.XY(), (float)Math.Floor(vector3.Z) - .1f);
            emitter.Emit(10);
            this.Emitters.Add(emitter);
        }

        //void EntityFootStep(GameEvent e)
        //{
        //    var entity = e.Parameters[0] as GameObject;
        //    var vec = new Vector3(entity.Global.X, entity.Global.Y, (int)Math.Ceiling(entity.Global.Z) - 1);
        //    var block = entity.Map.GetBlock(vec);
        //    var emitter = block.GetEmitter();
        //    emitter.Source = entity.Global;
        //    emitter.Emit(10, -entity.Velocity * .1f);
        //    this.Emitters.Add(emitter);
        //}
        void EntityFootStep(EntityFootStepEvent e)
        {
            var entity = e.Entity;// e.Parameters[0] as GameObject;
            var vec = new Vector3(entity.Global.X, entity.Global.Y, (int)Math.Ceiling(entity.Global.Z) - 1);
            var block = entity.Map.GetBlock(vec);
            var emitter = block.GetEmitter();
            emitter.Source = entity.Global;
            //emitter.Emit(10, -entity.Velocity * .1f);
            emitter.Emit(100, -entity.Velocity * .1f);
            this.Emitters.Add(emitter);
        }
    }
    class EntityFootStepEvent(Entity entity) : EventPayloadBase
    {
        public readonly Entity Entity = entity;
    }
}

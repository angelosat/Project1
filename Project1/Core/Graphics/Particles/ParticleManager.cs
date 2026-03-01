using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Core.Components;
using Project1.Core.Simulation;
using Project1.Core.Entities;
using Project1.Core.Networking;

namespace Project1.Core.Graphics.Particles
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
            if (this.Map.Net is Server)
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
       
        //void EntityHitGround(GameEvent e)
        //{
        //    var entity = e.Parameters[0] as GameObject;
        //    var vector3 = (Vector3)e.Parameters[1];
        //    var block = entity.Map.GetBlock(vector3);
        //    var emitter = block.GetEmitter();
        //    emitter.Source = entity.Global;
        //    emitter.Emit(10);
        //    this.Emitters.Add(emitter);
        //}

        //void EntityHitCeiling(GameEvent e)
        //{
        //    var entity = e.Parameters[0] as GameObject;
        //    var vector3 = (Vector3)e.Parameters[1];
        //    var block = entity.Map.GetBlock(vector3);
        //    if (block is null)
        //        return;
        //    var emitter = block.GetEmitter();
        //    emitter.Source = new Vector3(vector3.XY(), (float)Math.Floor(vector3.Z) - .1f);
        //    emitter.Emit(10);
        //    this.Emitters.Add(emitter);
        //}

        void EntityFootStep(EntityFootStepEvent e)
        {
            var entity = e.Entity;// e.Parameters[0] as GameObject;
            //var vec = new Vector3(entity.Global.X, entity.Global.Y, (int)Math.Ceiling(entity.Global.Z) - 1);
            var vec = entity.Cell;//
            var block = entity.Map.GetBlock(vec);
            var emitter = block.GetEmitter();
            emitter.Source = entity.Global;
            //emitter.Emit(10, -entity.Velocity * .1f);
            //emitter.Emit(100, -entity.Velocity * .1f);
            emitter.Emit(10, entity.Velocity);
            this.Emitters.Add(emitter);
        }
    }
    
}

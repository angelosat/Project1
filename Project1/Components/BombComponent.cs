using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Start_a_Town_.Particles;

namespace Start_a_Town_.Components
{
    class BombComponent : EntityComp
    {
        public override string Name { get; } = "Bomb";
        public override object Clone()
        {
            return new BombComponent(this.Radius, this.Fuse);
        }

        public int Radius, Fuse, FuseMax;
        public bool Exploded;
        int FuseLengthIndex;
        static int[] FuseLengths = new int[] { -1, 1, 2, 3 };

        public BombComponent(int radius, int fuse)
        {
            this.Radius = radius;
            this.Fuse = fuse;
        }

        public override void Tick()
        {
            var parent = this.Owner;
            if (this.FuseMax == -1)
                return;
            if (!this.Exploded)
            {
                this.Fuse--;
                if (this.Fuse <= 0)
                    this.Explode(parent);
                return;
            }
        }

        void Explode(GameObject parent)
        {
            this.Exploded = true;
            var origin = parent.Transform.Global.ToRounded();
            var r = this.Radius;
            var map = parent.Map;
            var rsquared = this.Radius * this.Radius;
            for (int i = -r; i <= r; i++)
                for (int j = -r; j <= r; j++)
                    for (int k = -r; k <= r; k++)
                    {
                        var pos = origin + new Vector3(i, j, k);
                        var block = map.GetBlock(pos);
                        if (block == null || block == BlockDefOf.Air)
                            continue;
            
                        var d = Vector3.DistanceSquared(pos, origin);
                        if (d < rsquared)
                            map.RemoveBlock(pos);
                    }
            parent.GetComponent<SpriteComp>().Hidden = true;
            var emitter = ParticleEmitter.Dust;
            emitter.ColorBegin = Color.White;
            emitter.ColorEnd = Color.Black;
            emitter.Source = parent.Global;
            emitter.Force = 3f;
            emitter.SizeBegin = 20;
            emitter.Lifetime = Ticks.PerSecond * 4;
            emitter.ParticleWeight = -.5f;
            emitter.Acceleration = new Vector3(.5f, .5f, .5f);
            emitter.HasPhysics = false;
            emitter.Emit(20);
            parent.Map.ParticleManager.AddEmitter(emitter);
            this.PushEntities(parent);
        }

        void PushEntities(GameObject parent)
        {
            var rsquared = this.Radius * this.Radius;
            var global = parent.Transform.Global;
            var entities = parent.Map.GetObjectsAtChunk(global);
            foreach(var entity in entities)
            {
                var distance = entity.Transform.Global + entity.Physics.Height * Vector3.UnitZ * .5f - global;
                var d = distance.LengthSquared();
                if (d < rsquared)
                {
                    var t = 1 - d / (float)rsquared; //1 is ground zero , 0 is edge of radius
                    var force = t * distance.Normalized() *.5f; // TODO: factor entity weight
                    entity.Velocity += force;
                }
            }
        }

        internal override void GetEquippedActionsWithTarget(GameObject parent, GameObject actor, TargetArgs t, List<Interaction> list)
        {
            list.Add(new InteractionSetFuse(this.CycleFuse));
        }

        int CycleFuse()
        {
            this.FuseLengthIndex++;
            this.FuseLengthIndex %= FuseLengths.Length;
            this.FuseMax = FuseLengths[this.FuseLengthIndex];
            this.Fuse = this.FuseMax * Ticks.PerSecond;
            return this.FuseMax;
        }

        class InteractionSetFuse : Interaction
        {
            Func<int> FuseSet;
            public override object Clone()
            {
                return new InteractionSetFuse(this.FuseSet);
            }
            public InteractionSetFuse(Func<int> callback)
            {
                this.Name = "Set fuse";
                this.FuseSet = callback;
            }
            public override void Perform()
            {
                var fuse = this.FuseSet();
                //var client = a.Net as Net.Client;
                //if (client != null)
                //{
                //    client.GetConsole().Write(Color.Red, "Fuse set to: " + (fuse == -1 ? "none" : TimeSpan.FromSeconds(fuse).TotalSeconds.ToString("#s")));
                //}
            }
        }
    }
}

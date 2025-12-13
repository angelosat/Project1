using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Particles;
using Start_a_Town_.Net;

namespace Start_a_Town_.Components
{
    class BloodComponent : EntityComp
    {
        public override string Name { get; } = "Blood"; 
        public override object Clone()
        {
            return new BloodComponent();
        }

        static readonly ParticleEmitterSphere BloodEmitter = new ParticleEmitterSphere()
        {
            Lifetime = Ticks.PerSecond * 5,
            Offset = Vector3.UnitZ,
            Rate = 0,
            ParticleWeight = 1f,
            ColorEnd = Color.Red,
            ColorBegin = Color.Red,
            SizeEnd = 3,
            SizeBegin = 3,
            SizeVariance = 2,
            Force = .1f
        };
        List<ParticleEmitterSphere> Emitters = new List<ParticleEmitterSphere>();

        public BloodComponent()
        {

        }
        public override void Tick()
        {
            var parent = this.Parent;
            foreach (var e in this.Emitters.ToList())
            {
                e.Update(parent.Map, e.Source);
                if (e.Particles.Count == 0)
                    this.Emitters.Remove(e);
            }
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch(e.Type)
            {
                case Message.Types.Attacked:
                    if (parent.Net is Server)
                        break;
                    GameObject attacker = e.Parameters[0] as GameObject;
                    var direction = parent.Global - attacker.Global;
                    direction.Normalize();
                    direction *= .05f;
                    direction += attacker.Velocity;

                    var emitter = BloodEmitter.Clone() as ParticleEmitterSphere;
                    emitter.Source = parent.Global;
                    emitter.Emit(10, direction);
                    this.Emitters.Add(emitter);
                    break;

                default:
                    break;
            }
            return true;
        }

        public override void Draw(MySpriteBatch sb, GameObject parent, Camera camera)
        {
            foreach (var e in this.Emitters)
                e.Draw(camera, parent.Map, e.Source);
        }
    }
}

using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Graphics;
using Project1.Core.Graphics.Particles;
using Project1.Core.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Components;

class BloodComponent : EntityComp
{
    public override EntityCompDef CompDef => EntityCompDefOf.Blood;
    public override string Name { get; } = "Blood"; 
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
        var parent = this.Owner;
        foreach (var e in this.Emitters.ToList())
        {
            e.Update(parent.Map, e.Source);
            if (e.Particles.Count == 0)
                this.Emitters.Remove(e);
        }
    }
    /// <summary>
    /// dont delete
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private bool OnHit(Entity attacker)
    {
        var parent = this.Owner;
        if (parent.Net.IsServer)
            return false;
        var direction = parent.Global - attacker.Global;
        direction.Normalize();
        direction *= .05f;
        direction += attacker.Velocity;

        var emitter = BloodEmitter.Clone() as ParticleEmitterSphere;
        emitter.Source = parent.Global;
        emitter.Emit(10, direction);
        this.Emitters.Add(emitter);
        return true;
    }

    //public override void Draw(MySpriteBatch sb, RenderContext ctx)
    //{
    //    foreach (var e in this.Emitters)
    //        e.Draw(ctx, e.Source);
    //}
}

using Project1.Core.Networking;
using Project1.Core.Rendering;
using Project1.Core.Simulation;
using System.Collections.Generic;

namespace Project1.Core.Graphics.Particles;

public sealed class ParticleManager(MapBase map)
{
    readonly HashSet<ParticleEmitter> Emitters = [];
    readonly List<ParticleEmitter> _forRemoval = [];
    readonly MapBase Map = map;

    public void AddEmitter(ParticleEmitter emitter)
    {
        if (this.Map.Net is Server)
            return;
        this.Emitters.Add(emitter);
    }
    
    public void Update()
    {
        this._forRemoval.Clear();
        foreach (var e in this.Emitters)
        {
            e.Update(this.Map, e.Source);
            if (e.Particles.Count == 0)
                this._forRemoval.Add(e);
        }
        foreach (var e in this._forRemoval)
            this.Emitters.Remove(e);
    }
    public void Draw(RenderContext ctx)
    {
        foreach (var e in this.Emitters)
            e.Draw(ctx, e.Source);
    }
}

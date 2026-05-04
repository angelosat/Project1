using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Simulation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project1.Core.Rendering;

internal struct FrameData
{
    public RenderContext Ctx;

    public List<Chunk> VisibleChunks;
    public List<Entity> VisibleEntities;

    public Vector4 FogColor;
    public Vector2 FogOffset;
    public float Fog;

    public float RotCos;
    public float RotSin;

    public Color ClearColor;
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public abstract class WorldBase : Inspectable
    {
        internal float GroundAirThreshold;

        public string Name { get; set; }
        public override string Label => this.Name;
        public abstract MapBase GetMap(Vector2 mapCoords);
        public Random Random { get; set; }
        public virtual float Gravity { get; }
        public int Seed { get; set; }
        public int MaxHeight { get; set;  }
        public virtual ulong CurrentTick { get; set; }
        public virtual TimeSpan Clock { get; }
        public INetEndpoint Net { get; set; }

        public byte[] SeedArray { get; set; }

        public virtual Block DefaultBlock { get; set; }
        public virtual PopulationManager Population { get; }

        public virtual List<Terraformer> Terraformers { get; set; }

        public T GetTerraformer<T>() where T : Terraformer => this.Terraformers.First(t => t is T) as T;

        public abstract void WriteData(BinaryWriter w);

        public abstract MapCollection GetMaps();

        public abstract void Draw(SpriteBatch sb, Camera cam);
        public abstract void Tick(INetEndpoint net);
        public abstract void OnHudCreated(Hud hud);
        public abstract void OnTargetSelected(IUISelection info, ISelectable selection);
        public abstract void OnTargetSelected(SelectionManager info, ISelectable selection);

        public abstract void ResolveReferences();
    }
}

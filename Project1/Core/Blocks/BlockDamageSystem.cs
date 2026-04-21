using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Simulation;
using Project1.Framework.UI.Primitives;
using System;
using System.Collections.Generic;

namespace Project1.Core.Blocks
{
    internal class BlockDamageSystem(Chunk chunk) : ChunkSystem
    {
        static readonly float BlockTokenDrawThreshold = Ticks.FromSeconds(2);
        readonly Chunk Chunk = chunk;
        readonly Dictionary<IntVec3Local, BlockHealthToken> _blockTokens = [];
        public IBlockHealth GetBlockHealth(IntVec3Local local) => this._blockTokens.TryGetValue(local, out var token) ? token : null;

        internal override void Tick()
        {
            var keysToRemove = new List<IntVec3Local>(this._blockTokens.Count);
            foreach (var (pos, token) in this._blockTokens)
            {
                token.Tick();
                if (token.HasExpired)
                    keysToRemove.Add(pos);
            }
            foreach (var k in keysToRemove)
                this._blockTokens.Remove(k);
        }

        public BlockHealthToken.BlockDamageResult ApplyDamage(IntVec3Local pos, int work)
        {
            if (work == 0)
                return BlockHealthToken.BlockDamageResult.NoChange;
            if (!this._blockTokens.TryGetValue(pos, out var token))
            {
                var cell = this.Chunk.GetLocalCell(pos);
                if (cell.Block.BlockDef == BlockDefOf.Air)
                    throw new Exception();
                token = new(cell);
                this._blockTokens.Add(pos, token);
            }
            return token.ApplyWork(work);
        }

        public void Delete(IntVec3Local pos)
        {
            this._blockTokens.Remove(pos);
        }

        internal void DrawBlockTokens(SpriteBatch sb, Camera camera)
        {
            if (camera.Zoom < 1)
                return;
            foreach (var (pos, token) in this._blockTokens)
                if (token.Lifetime < BlockTokenDrawThreshold)
                    Bar.Draw(sb, camera, pos.ToGlobal(this.Chunk), "Block HitPoints", token.HealthPercentage, camera.Zoom * .2f);
        }
    }
}

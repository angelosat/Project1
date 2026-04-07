using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Simulation;
using Project1.Core.UI;
using Project1.Core.UI.Hud;
using Project1.Framework;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;

namespace Project1.Core.Input
{
    public record struct CellSelection(MapBase Map, IntVec3 Global, IntVec3 Face = default) : ISelectable
    {
        Cell _cell;
        BlockEntity _blockEntity;
        public Cell Cell => this._cell ??= this.Map.GetCell(this.Global);
        public Block Block => this.Cell.Block;
        public BlockEntity BlockEntity => this._blockEntity ??= this.Map.GetBlockEntity(this.Global);

        public readonly InteractionTarget ToTarget() => new(this.Map, this.Global);

        public readonly string Name => $"Cell {Global}";

        public readonly bool Exists => this.Map.Contains(this.Global);

        readonly Vector3 ISelectable.Global => this.Global;

        public IEnumerable<(string label, Type type)> GetSelectionTabs()
        {
            foreach (var i in this.Block.GetSelectionTabs())
                yield return i;
        }

        public IEnumerable<Control> GetSelectionDetails()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Control> GetSelectionInfo()
        {
            foreach (var ctrl in this.Cell.GetSelectionInfo())
                yield return ctrl;
        }
        public IEnumerable<IconButton> GetMiniButtons()
        {
            yield break;
        }
    }
}

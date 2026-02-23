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

        public readonly TargetArgs ToTarget() => new(this.Map, this.Global);

        public readonly string Name => $"Cell {Global}";

        public readonly bool Exists => this.Map.Contains(this.Global);

        readonly Vector3 ISelectable.Global => this.Global;

        public IEnumerable<(string name, Action action)> GetInfoTabs()
        {
            foreach (var i in this.Block.GetInfoTabs())
                yield return i;
        }

        public void GetQuickButtons(SelectionManager panel)
        {
            this.Block.GetQuickButtons(panel, this.Map, this.Global);
            var cell = this;
            this.Map.GetQuickButtons((name, guiType) =>
                    //info.AddTabAction(name, () => UIManager.ToggleUnique<WorkstationGuiNew>(new TargetArgs(this.Map, this.BlockEntity.OriginGlobal))), 
                    panel.AddTabAction(name, () => UIManager.ToggleUnique(guiType, cell)), this.Global);
        }

        public IEnumerable<Control> GetSelectionDetails()
        {
            throw new NotImplementedException();
        }

        public void GetSelectionInfo(IUISelection panel)
        {
            throw new NotImplementedException();
        }

        public void GetSelectionInfo(SelectionManager info)
        {
            return;
        }
    }
}

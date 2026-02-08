using Project1.Core.Entities.Actors;
using Project1.Core.Base;
using Project1.Core.Interfaces;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Legacy.Crafting.Gui;
using Project1.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Project1.Core.Simulation;
using Project1.Framework.IO;
using Project1.Framework.Math;
using Project1.Framework.UI;

namespace Project1.Core
{
    public class BlockEntityCompWorkstationOld : BlockEntityComp
    {
        public override string Name { get; } = "Workstation";
        static readonly string OperatingPositionUnreachableString = $"Interaction spot blocked";
        public bool OperatingPositionUnreachable;

        readonly ObservableCollection<CraftOrder> _orders = new();
        public ObservableCollection<CraftOrder> Orders => this._orders;
        readonly HashSet<IsWorkstation.Types> WorkstationTypes;
        static Window CraftingWindow;

        public BlockEntityCompWorkstationOld(params IsWorkstation.Types[] types)
        {
            this.WorkstationTypes = new HashSet<IsWorkstation.Types>(types);
        }
        public bool IsWorkstationType(IsWorkstation.Types type)
        {
            return this.WorkstationTypes.Contains(type);
        }

        internal CraftOrder GetOrder(string uniqueID)
        {
            return this.Orders.First(o => o.GetUniqueLoadID() == uniqueID);
        }
        internal CraftOrder GetOrder(int uniqueID)
        {
            return this.Orders.First(o => o.ID == uniqueID);
        }
        internal bool RemoveOrder(string orderID)
        {
            return this.Orders.Remove(this.GetOrder(orderID));
        }
        internal CraftOrder RemoveOrder(int orderID)
        {
            var order = this.GetOrder(orderID);
            this.Orders.Remove(order);
            order.Removed();
            return order;
        }
        internal bool Reorder(int orderID, bool increasePriority)
        {
            var order = this.GetOrder(orderID);
            var prevIndex = this.Orders.IndexOf(order);
            this.Orders.Remove(order);
            var newIndex = Math.Max(0, Math.Min(this.Orders.Count, prevIndex + (increasePriority ? -1 : 1)));
            this.Orders.Insert(newIndex, order);
            return true;
        }

        public void ShowUI(MapBase map, IntVec3 global)
        {
            if (CraftingWindow != null)
                CraftingWindow.Hide();

            CraftingWindow = new WorkstationGui(map, global, this).ToWindow("Crafting");
            CraftingWindow.ToggleSmart();
        }

        internal override void DrawSelected(MySpriteBatch sb, Camera cam, MapBase map, IntVec3 global)
        {
        }
        protected override void SaveExtra(SaveTag tag)
        {
            tag.TrySaveRefs(this.Orders, "Orders");
        }
        public override void Load(SaveTag tag)
        {
            tag.TryLoadRefs(this.Orders, "Orders");
        }
        public override void Write(IDataWriter w)
        {
            w.Write(this._orders);
        }
        public override ISerializable Read(IDataReader r)
        {
            r.ReadNewInto(this._orders);
            return this;
        }
        internal override void ResolveReferences(MapBase map, IntVec3 global)
        {
            foreach (var ord in this._orders)
            {
                ord.ResolveReferences(map);
                map.Town.CraftingManager.RegisterOrder(ord);
            }
        }
        internal override void OnNeighborChanged(MapBase map, IntVec3 source)
        {
            this.CheckOperatingPositions();
        }

        private void CheckOperatingPositions()
        {
            var prev = this.OperatingPositionUnreachable;

            /// we dont need to query the cell for the interaction spots, we already know that the block is a blockworkstation, we also know the originglobal of the blockentity
            /// BUT we dont know the orientation, so we still need the cell
            var orientation = this.Map.GetCell(this.Global).Orientation;
            var interactionSpots = BlockDefOf.Workbench.Worker.GetInteractionSpotsLocal(this.Parent.Map, this.Parent.OriginGlobal, orientation);// this.Map.GetCell(this.Global).GetOperatingPositions();
            this.OperatingPositionUnreachable = interactionSpots.All(p => !ActorDefOf.Npc.CanFitIn(this.Map, this.Global + p));
            if (!prev && this.OperatingPositionUnreachable)
                this.Errors.Add(OperatingPositionUnreachableString);
            else if (prev && !this.OperatingPositionUnreachable)
                this.Errors.Remove(OperatingPositionUnreachableString);
        }

        public override void OnSpawned(BlockEntity entity, MapBase map)
        {
            this.CheckOperatingPositions();
        }
    }
}
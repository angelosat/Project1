using Project1.Core.Blocks;
using Project1.Core.Blocks.Comps;
using Project1.Core.Entities.Actors;
using Project1.Core.Graphics;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Project1.Core.Legacy
{
    public class BlockEntityCompWorkstationOld : BlockComp
    {
        public override BlockCompDef CompDef => throw new NotImplementedException();

        static readonly string OperatingPositionUnreachableString = $"Interaction spot blocked";
        public bool OperatingPositionUnreachable;

        readonly ObservableCollection<CraftOrderOld> _orders = new();
        public ObservableCollection<CraftOrderOld> Orders => this._orders;
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

        internal CraftOrderOld GetOrder(string uniqueID)
        {
            return this.Orders.First(o => o.GetUniqueLoadID() == uniqueID);
        }
        internal CraftOrderOld GetOrder(int uniqueID)
        {
            return this.Orders.First(o => o.ID == uniqueID);
        }
        internal bool RemoveOrder(string orderID)
        {
            return this.Orders.Remove(this.GetOrder(orderID));
        }
        internal CraftOrderOld RemoveOrder(int orderID)
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


        internal override void DrawSelected(MySpriteBatch sb, Camera cam, MapBase map, IntVec3 global)
        {
        }
        protected override void SaveExtra(SaveTag tag)
        {
        }
        public override void Load(SaveTag tag)
        {
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
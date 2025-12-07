using System;
using Start_a_Town_.Components;
using Start_a_Town_.Animations;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using System.Linq;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    public class BlockBedEntity : BlockEntity
    {
        public enum Types { Citizen, Visitor };
        public bool Occupied { get { return this.CurrentOccupant != -1; } }
        public int CurrentOccupant = -1;
        public Actor Owner;
        public Types Type = Types.Citizen;
        public BlockBedEntity(IntVec3 originGlobal)
            : base(originGlobal)
        {

        }

        internal override void GetSelectionInfo(IUISelection info, MapBase map, IntVec3 vector3)
        {
            var room = map.GetRoomAt(vector3);
            if (room is not null)
                room.GetSelectionInfo(info);
            var roomOwner = room?.Owner;
            info.AddInfo(new ComboBoxNewNew<Actor>(128, "Owner", a => a?.Name ?? "none", setOwner, () => this.Owner, () => map.Town.GetMembers().Prepend(null)));
            info.AddInfo(new ComboBoxNewNew<Types>(128, "Type", t => t.ToString(), setType, () => this.Type, () => Enum.GetValues(typeof(Types)).Cast<Types>()));

            void setOwner(Actor newOwner) => Packets.SetOwner(Client.Instance, map.Net.GetPlayer(), vector3, newOwner);
            void setType(Types newType) => Packets.SetType(Client.Instance, map.Net.GetPlayer(), vector3, newType);

            UpdateQuickButtons();
        }

        protected override void WriteExtra(System.IO.BinaryWriter w)
        {
            w.Write(this.CurrentOccupant);
            w.Write((int)this.Type);
        }
        protected override void ReadExtra(IDataReader r)
        {
            this.CurrentOccupant = r.ReadInt32();
            this.Type = (Types)r.ReadInt32();
        }
        protected override void AddSaveData(SaveTag tag)
        {
            tag.Add(new SaveTag(SaveTag.Types.Int, "Occupant", this.CurrentOccupant));
            ((int)this.Type).Save(tag, "Type");
        }
        
        protected override void LoadExtra(SaveTag tag)
        {
            tag.TryGetTagValueOrDefault("Occupant", out this.CurrentOccupant);
            tag.TryGetTagValue<int>("Type", v => this.Type = (Types)v);
        }

        internal Color GetColorFromType()
        {
            return this.Type switch
            {
                Types.Citizen => Color.White,
                Types.Visitor => Color.Cyan,
                _ => throw new Exception(),
            };
        }

        private static void SetOwner(MapBase map, IntVec3 global, Actor owner)
        {
            map.GetBlockEntity<BlockBedEntity>(global).Owner = owner;
        }
        private static void SetType(MapBase map, IntVec3 global, BlockBedEntity.Types type)
        {
            var bentity = map.GetBlockEntity<BlockBedEntity>(global);
            bentity.Type = type;
            map.InvalidateCell(global);
            if (map.IsActive && SelectionManager.SingleSelectedCell == global)
                bentity.UpdateQuickButtons();
        }

        static readonly IconButton ButtonSetVisitor = new(Icon.Construction) { HoverText = "Set to visitor bed" };
        static readonly IconButton ButtonUnsetVisitor = new(Icon.Construction, Icon.Cross) { HoverText = "Set to citizen bed" };
        void UpdateQuickButtons()
        {
            var t = this.Type;
            var map = this.Map;
            var vector3 = this.OriginGlobal;
            switch (t)
            {
                case BlockBedEntity.Types.Citizen:
                    SelectionManager.RemoveButton(ButtonUnsetVisitor);
                    SelectionManager.AddButton(ButtonSetVisitor, t => Packets.SetType(map.Net, map.Net.GetPlayer(), vector3, BlockBedEntity.Types.Visitor), (map, vector3));
                    return;

                case BlockBedEntity.Types.Visitor:
                    SelectionManager.RemoveButton(ButtonSetVisitor);
                    SelectionManager.AddButton(ButtonUnsetVisitor, t => Packets.SetType(map.Net, map.Net.GetPlayer(), vector3, BlockBedEntity.Types.Citizen), (map, vector3));
                    return;

                default:
                    throw new Exception();
            }
        }
        [EnsureStaticCtorCall]
        static class Packets
        {
            static readonly int pOwner, pChangeType;
            static Packets()
            {
                pOwner = Registry.PacketHandlers.Register(SetOwner);
                pChangeType = Registry.PacketHandlers.Register(SetType);
            }

            internal static void SetOwner(NetEndpoint net, PlayerData playerData, IntVec3 global, Actor owner)
            {
                if (net is Server s)
                    BlockBedEntity.SetOwner(s.Map, global, owner);

                //net.GetOutgoingStreamOrderedReliable().Write(pOwner, playerData.ID, global, owner?.RefId ?? -1);

                var w = net.BeginPacket(pOwner);
                w
                    .Write(playerData.ID)
                    .Write(global)
                    .Write(owner?.RefId ?? -1);
            }

            private static void SetOwner(NetEndpoint net, Packet pck)
            {
                var r = pck.PacketReader;
                var player = net.GetPlayer(r.ReadInt32());
                var global = r.ReadIntVec3();
                var owner = r.ReadInt32() is int refID && refID > -1 ? net.World.GetEntity<Actor>(refID) : null;
                if (net is Client c)
                    BlockBedEntity.SetOwner(c.Map, global, owner);
                else
                    SetOwner(net, player, global, owner);
            }

            internal static void SetType(NetEndpoint net, PlayerData playerData, IntVec3 vector3, BlockBedEntity.Types type)
            {
                if (net is Server s)
                    BlockBedEntity.SetType(s.Map, vector3, type);

                //net.GetOutgoingStreamOrderedReliable().Write(pChangeType, playerData.ID, vector3, (int)type);
                net.BeginPacketNew(ReliabilityType.OrderedReliable, pChangeType).Write(playerData.ID, vector3, (int)type);
            }

            private static void SetType(NetEndpoint net, Packet pck)
            {
                var r = pck.PacketReader;
                var player = net.GetPlayer(r.ReadInt32());
                var vec = r.ReadIntVec3();
                var type = (BlockBedEntity.Types)r.ReadInt32();
                if (net is Client c)
                    BlockBedEntity.SetType(c.Map, vec, type);
                else
                    SetType(net, player, vec, type);
            }
        }
    }
}

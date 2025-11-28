using System;
using Start_a_Town_.Components;
using Start_a_Town_.Animations;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using System.Linq;
using Start_a_Town_.Net;
using System.IO;

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
        public void Sleep(GameObject agent)
        {
            if (this.Occupied)
                throw new Exception();
            this.CurrentOccupant = agent.RefID;
            agent.GetComponent<SpriteComponent>().Body = agent.Body.FindBone(BoneDefOf.Head);
        }
        public void Wake(GameObject agent)
        {
            if (agent.RefID != this.CurrentOccupant)
                throw new Exception();
            this.CurrentOccupant = -1;
            agent.GetComponent<SpriteComponent>().Body = null;
        }
        public void ToggleSleep(GameObject agent, IntVec3 bedGlobal)
        {
            if (this.CurrentOccupant != -1)
            {
                if (agent.RefID != this.CurrentOccupant)
                    throw new Exception();
                this.CurrentOccupant = -1;
                var body = agent.Body;
                var head = body[BoneDefOf.Head];
                body.SetEnabled(true, true);
                body.RestingFrame = new Keyframe(0, Vector2.Zero, 0);
                head.RestingFrame = new Keyframe(0, Vector2.Zero, 0);
                agent.GetNeed(NeedDef.Energy).Mod -= 1;
            }
            else
            {
                this.CurrentOccupant = agent.RefID;
                var body = agent.Body;
                var headBone = agent.Body.FindBone(BoneDefOf.Head);
                body.RestingFrame = new Keyframe(0, agent.Body[BoneDefOf.Head].GetTotalOffset(), 0);
                body.SetEnabled(false, true);
                headBone.SetEnabled(true, false);
                headBone.RestingFrame = new Keyframe(0, Vector2.Zero, -(float)(Math.PI / 3f));
                var bedPos = this.OriginGlobal;
                var cell = agent.Map.GetCell(bedPos);
                agent.SetPosition(bedPos + new IntVec3(0, 0, cell.Block.GetHeight(cell.BlockData, 0, 0)));
                agent.GetNeed(NeedDef.Energy).Mod += 1;
            }
        }

        internal override void GetSelectionInfo(IUISelection info, MapBase map, IntVec3 vector3)
        {
            var room = map.GetRoomAt(vector3);
            if (room is not null)
                room.GetSelectionInfo(info);
            var roomOwner = room?.Owner;
            info.AddInfo(new ComboBoxNewNew<Actor>(128, "Owner", a => a?.Name ?? "none", setOwner, () => this.Owner, () => map.Town.GetAgents().Prepend(null)));
            info.AddInfo(new ComboBoxNewNew<Types>(128, "Type", t => t.ToString(), setType, () => this.Type, () => Enum.GetValues(typeof(Types)).Cast<Types>()));

            void setOwner(Actor newOwner) => Packets.SetOwner(map.Net, map.Net.GetPlayer(), vector3, newOwner);
            void setType(Types newType) => Packets.SetType(map.Net, map.Net.GetPlayer(), vector3, newType);

            UpdateQuickButtons();
        }

        protected override void WriteExtra(System.IO.BinaryWriter w)
        {
            w.Write(this.CurrentOccupant);
            w.Write((int)this.Type);
        }
        protected override void ReadExtra(System.IO.BinaryReader r)
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
            tag.TryGetTagValue("Occupant", out this.CurrentOccupant);
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
                pOwner = Network.RegisterPacketHandler(SetOwner);
                pChangeType = Network.RegisterPacketHandler(SetType);
            }

            internal static void SetOwner(INetPeer net, PlayerData playerData, IntVec3 global, Actor owner)
            {
                if (net is Server)
                    BlockBedEntity.SetOwner(net.Map, global, owner);

                net.GetOutgoingStream().Write(pOwner, playerData.ID, global, owner?.RefID ?? -1);
            }

            private static void SetOwner(INetPeer net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var global = r.ReadIntVec3();
                var owner = r.ReadInt32() is int refID && refID > -1 ? net.GetNetworkObject<Actor>(refID) : null;
                if (net is Client)
                    BlockBedEntity.SetOwner(net.Map, global, owner);
                else
                    SetOwner(net, player, global, owner);
            }

            internal static void SetType(INetPeer net, PlayerData playerData, IntVec3 vector3, BlockBedEntity.Types type)
            {
                if (net is Server)
                    BlockBedEntity.SetType(net.Map, vector3, type);

                net.GetOutgoingStream().Write(pChangeType, playerData.ID, vector3, (int)type);
            }

            private static void SetType(INetPeer net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var vec = r.ReadIntVec3();
                var type = (BlockBedEntity.Types)r.ReadInt32();
                if (net is Client)
                    BlockBedEntity.SetType(net.Map, vec, type);
                else
                    SetType(net, player, vec, type);
            }
        }
    }
}

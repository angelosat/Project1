using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Start_a_Town_
{
    public class RoomManager : TownComponent
    {
        int RoomIDSequence;
        int GetNextRoomID()
        {
            return this.RoomIDSequence++;
        }
        public override string Name => "InsideManager";
        readonly Dictionary<int, Room> Rooms = new();
        public RoomManager(Town town)
        {
            this.Town = town;
        }

        internal override void OnBlocksChanged(IEnumerable<IntVec3> positions)
        {
            foreach (var pos in positions)
                this.Handle(pos);
        }
        internal Room GetRoom(int roomID)
        {
            return this.Rooms[roomID];
        }
        public bool TryGetRoom(int roomID, out Room room)
        {
            return this.Rooms.TryGetValue(roomID, out room);
        }
        internal Room FindRoom(int actorID)
        {
            return this.Rooms.Values.FirstOrDefault(r => r.OwnerRef == actorID);
        }
        internal IEnumerable<Room> GetRoomsByOwner(Actor actor)
        {
            return this.GetRoomsByOwner(actor.RefId);
        }
        internal IEnumerable<Room> GetRoomsByOwner(int actorID)
        {
            return this.Rooms.Values.Where(r => r.OwnerRef == actorID);
        }
        bool Valid = true;
        internal void Init()
        {
            var sw = Stopwatch.StartNew();
            foreach(var r in this.Rooms.Values)
                r.Validate();
            sw.Stop();
            $"rooms validated in {sw.ElapsedMilliseconds} ms".ToConsole();
            if (!this.Valid)
            {
                this.Valid = true;
                this.ScanMap();
            }
        }

        private void ScanMap()
        {
            var map = this.Map as StaticMap;
            if (map.Net is Client)
                return;
            HashSet<IntVec3> handled = new();
            var size = map.Size.Blocks;
            var maxh = MapBase.MaxHeight;
            this.Rooms.Clear();
            var sw = Stopwatch.StartNew();
            foreach (var chunk in map.ActiveChunks.Values)
            {
                foreach (var cell in chunk.Cells)
                {
                    var global = cell.GetGlobalCoords(chunk);
                    if (handled.Contains(global))
                        continue;
                    if (cell.IsRoomBorder)
                        continue;
                    var positions = FloodFill.BeginIncludeEdges(map, global, (cell, global) => !cell.IsRoomBorder);
                    bool isOutdoors = false;
                    foreach (var p in positions)
                    {
                        if (!isOutdoors)
                        {
                            if (map.IsAboveHeightMap(p))
                                isOutdoors = true;
                        }
                        handled.Add(p);
                    }

                    if (!isOutdoors)
                    {
                        var newroom = new Room(map, positions);
                        this.AddRoom(newroom);
                        $"Room found (size: {newroom.Size})".ToConsole();
                    }
                }
            }
            sw.Stop();
            $"{this.Rooms.Count} rooms found in {sw.ElapsedMilliseconds} ms".ToConsole();
        }

        internal void AddRoom(Room room)
        {
            room.ID = this.GetNextRoomID();
            this.Rooms.Add(room.ID, room);
        }
        internal void RemoveRoom(Room room)
        {
            this.Rooms.Remove(room.ID);
            room.Remove();
        }

        void Handle(IntVec3 global)
        {
            var map = this.Map;
            var block = map.GetBlock(global);
            if (!block.IsRoomBorder)
            {
                if (this.TryGetRoomAt(global, out var existing))
                {
                    existing.Invalidate();
                }
                this.TryConnectRoomsAtNew(global);
            }
            else
            {
                if (this.TryGetRoomAt(global, out var existing))
                {
                    existing.Invalidate();
                    if (existing.TryRemovePosition(global, out var newRooms)) // TODO this method is old and doesnt include edges
                    {
                        foreach (var newroom in newRooms)
                            this.AddRoom(newroom);
                    }
                    else
                    {
                        if (!existing.Interior.Any())
                            this.RemoveRoom(existing);
                    }
                    return; // temporary
                }
                else if (this.GetRoomBorderAt(global) is Room existingBorder)
                {
                    /// added this check here because the same room is detected twice when adding a door (which calls blockchanged for the 2 blocks that are part of the door)
                }
                else // the changed position wasn't part of a room, so no room has been split. check adjacent cells for newly formed rooms
                {
                    foreach (var n in global.GetAdjacentLazy())
                    {
                        // TODO check if a new indoors area has been created after placing this block
                        if (Room.TryCreate(map, n) is Room newRoom)
                            this.AddRoom(newRoom);
                    }
                }
            }
        }
        private void TryConnectRoomsAtNew(IntVec3 global)
        {
            // TODO can optimize below
            var adjGlobals = global.GetAdjacentLazy();
            var adjRooms = adjGlobals.Select(this.GetRoomAt);
            var nrooms = adjRooms.OfType<Room>().Distinct().ToArray();

            if (adjGlobals.Any(g => this.Map.GetCell(g) is Cell cell && !cell.IsRoomBorder && this.GetRoomAt(g) is null))
            // if an adjacent cell is NOT a room boundary and it's not contained in an existing room
            // it means that it adjacent to an outdoors area. so delete all adjacent rooms
            {
                foreach (var r in nrooms)
                    this.RemoveRoom(r);
                return;
            }

            if (!nrooms.Any()) // if the position was surrounded by room borders, create a new room of size 1
            {
                var newroom = new Room(this, new[] { global });
                this.AddRoom(newroom);
                return;
            }
            else if (nrooms.Length == 1)
            {
                var singleRoom = nrooms[0];
                singleRoom.AddPosition(global);
                singleRoom.Invalidate();
                return;
            }
            else
            {
                var bySize = nrooms.OrderByDescending(r => r.Size);
                var largestRoom = bySize.First();
                largestRoom.AddPosition(global);
                var otherRooms = bySize.Skip(1);
                foreach (var other in otherRooms)
                {
                    largestRoom.Absorb(other);
                    this.RemoveRoom(other);
                }
                largestRoom.Invalidate();
            }
        }

        internal override void OnTargetSelected(IUISelection info, TargetArgs selected)
        {
            if (this.GetRoomAt(selected.FaceGlobal) is Room r)
                info.AddTabAction("Roomm", () => r.ShowGUI(selected.FaceGlobal));
        }
        public override ISelectable QuerySelectable(TargetArgs selected)
        {
            return null; // instead of selecting the room itself, add a tab when selecting a block that is contained in the room
        }

        public override void DrawBeforeWorld(MySpriteBatch sb, MapBase map, Camera cam)
        {
            if (!Engine.DrawRooms)
                return;
            foreach (var room in this.Rooms.Values)
                room.Draw(cam);
        }

        public bool TryGetRoomAt(IntVec3 global, out Room room)
        {
            room = this.Rooms.Values.FirstOrDefault(r => r.Contains(global));
            return room != null;
        }
        public Room GetRoomAt(IntVec3 global)
        {
            return this.Rooms.Values.FirstOrDefault(r => r.Contains(global));
        }
        public Room GetRoomBorderAt(IntVec3 global)
        {
            return this.Rooms.Values.FirstOrDefault(r => r.ContainsBorder(global));
        }
        internal override void OnTooltipCreated(Control tooltip, TargetArgs targetArgs)
        {
            if (targetArgs.Type != TargetType.Position)
                return;
            var global = targetArgs.FaceGlobal;
            if (!this.TryGetRoomAt(global, out var room))
                return;
            var control = room.GetControl().ToPanelLabeled("Room");
            tooltip.AddControlsBottomLeft(control);
        }

        protected override void AddSaveData(SaveTag tag)
        {
            this.Rooms.Values.TrySaveNewBEST(tag, "Rooms");
            tag.Add(this.RoomIDSequence.Save("RoomIDSequence"));
            this.Valid.Save(tag, "Valid");
        }
        public override void Load(SaveTag tag)
        {
            this.Rooms.Load(tag, "Rooms", r => { r.Map = this.Map; return r.ID; });
            tag.TryGetTagValue<int>("RoomIDSequence", out this.RoomIDSequence);
            tag.TryGetTagValueNew("Valid", ref this.Valid);
        }
        public override void Write(BinaryWriter w)
        {
            w.Write(this.RoomIDSequence);
            w.Write(this.Rooms.Values.ToList());
            w.Write(this.Valid);
        }
        public override void Read(BinaryReader r)
        {
            this.RoomIDSequence = r.ReadInt32();
            this.Rooms.Read(r, room => room.ID, this.Map);
            this.Valid = r.ReadBoolean();
        }
    }
}

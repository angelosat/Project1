using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using System.Reflection.Metadata.Ecma335;

namespace Start_a_Town_
{
    public partial class Room : Inspectable, ISelectable, ISaveable, ISerializableNew
    {
        static Room()
        {
        }

        public Room()
        {
            this.Color = ColorHelper.GetRandomColor();
        }
        public Room(MapBase map) : this()
        {
            this.Map = map;
        }
        Room(MapBase map, HashSet<IntVec3> positions) : this(map)
        {
            this.Interior = positions;
        }
        public Room(MapBase map, IEnumerable<IntVec3> positions) : this(map)
        {
            this.Interior = new();
            this.Border = new();

            foreach (var p in positions)
            {
                if (map.GetCell(p).IsRoomBorder)
                    this.AddEdge(p);
                else
                    this.AddPosition(p);
            }
        }
        public Room(RoomManager manager, ICollection<IntVec3> positions) : this(manager.Map, new HashSet<IntVec3>(positions)) { }
        private RoomRoleDef roomRole;
        DrawableCellCollection Cells = new();
        public HashSet<IntVec3> Interior = new();
        public HashSet<IntVec3> Border = new();
        public Color Color;
        public bool Exists => true;
        static Control GUI;
        private bool Valid;

        internal void Remove()
        {
            this.InvalidateBorderCells(); /// invalidate wall cells to draw them normally again 

            if (GUI?.Tag == this && GUI.GetWindow() is Window win && win.IsOpen)
                win.Hide();
        }

        internal void SetWorkplace(Workplace wplace)
        {
            if (this.Workplace == wplace)
                return;
            if(this.Workplace is Workplace existing)
                existing.RemoveRoom(this);
            this.Workplace = wplace;
            if (wplace is not null)
                wplace.AddRoom(this);
            this.Owner = null;
            this.Valid = false;
        }
        
        internal void AddOwner(Actor actor)
        {
            this.OwnerRef = actor.RefId;
        }
        internal void ForceAddOwner(Actor actor)
        {
            this.Owner = actor;
            if (this.workplace != null)
                this.Workplace = null;
        }
        internal void RemoveOwner(Actor actor)
        {
            if (this.OwnerRef == actor.RefId)
                this.OwnerRef = -1;
        }
        
        public RoomRoleDef RoomRole
        {
            get => roomRole;
            set
            {
                roomRole = value;
                this.Workplace?.RoomChanged(this);
            }
        }

        private int value;
        public int Value
        {
            get
            {
                if (!this.Valid)
                    this.Validate();
                return value;
            }
        }

        public int Size => this.Interior.Count;

        public int ID;
        public int OwnerRef = -1;
        public HashSet<FurnitureDef> Furnitures = new();
        public MapBase Map;
        private int workplaceID =-1;
        private Workplace workplace;
        public Workplace Workplace
        {
            get => workplaceID != -1 ? (workplace ??= this.Map.Town.ShopManager.GetShop(workplaceID)) : null;
            set
            {
                workplace = value;
                workplaceID = value?.ID ?? -1;
            }
        }
        public bool HasRole(RoomRoleDef role)
        {
            return this.RoomRole == role;
        }
        public string GetName()
        {
            return $"Room {this.ID}";
        }
        public IEnumerable<IntVec3> GetFurniturePositions(FurnitureDef furniture)
        {
            return this.Interior.Where(g => this.Map.GetBlock(g).Furniture == furniture);
        }
        public void GetQuickButtons(SelectionManager panel)
        {
        }
        public void GetSelectionInfo(IUISelection panel)
        {
            panel.AddInfo(new Label(() => $"Owner: {this.Owner?.Name ?? "none"}"));
            panel.AddInfo(new Label(() => $"Workplace: {this.Workplace?.Name ?? "none"}"));
        }
        public void GetSelectionInfo(SelectionManager info)
        {
            info.AddInfo(new Label(() => $"Owner: {this.Owner?.Name ?? "none"}"));
            info.AddInfo(new Label(() => $"Workplace: {this.Workplace?.Name ?? "none"}"));
        }

        internal void AddEdge(IntVec3 global)
        {
        }
        internal void AddPosition(IntVec3 global)
        {
            this.Interior.Add(global);
        }
        internal void AddPositions(IEnumerable<IntVec3> positions)
        {
            foreach (var p in positions)
                this.AddPosition(p);
        }
        private void RemovePosition(IntVec3 global)
        {
            this.Interior.Remove(global);
        }
        private void RemovePositions(IEnumerable<IntVec3> globals)
        {
            foreach (var g in globals)
                this.RemovePosition(g);
        }
        internal void AddEdges(IEnumerable<IntVec3> edges)
        {
            foreach (var p in edges)
                this.AddPosition(p);
        }
        internal void Absorb(Room smallerRoom)
        {
            this.AddPositions(smallerRoom.Interior);
            this.AddEdges(smallerRoom.Border);
        }
        internal void Invalidate()
        {
            this.Valid = false;
        }

        static public Room TryCreate(MapBase map, IntVec3 begin)
        {
            if (map.GetCell(begin) is not Cell cell)
                return null;
            if (cell.IsRoomBorder)
                return null;
            if (map.IsAboveHeightMap(begin))
                return null;
            HashSet<IntVec3> interior = new();
            HashSet<IntVec3> edges = new();

            interior.Add(begin);

            Queue<IntVec3> toHandle = new();
            HashSet<IntVec3> handled = new() { begin };
            toHandle.Enqueue(begin);
            while (toHandle.Any())
            {
                var current = toHandle.Dequeue();
                foreach (var n in current.GetAdjacentLazy())
                {
                    if (handled.Contains(n))
                        continue;
                    handled.Add(n);
                    if (!map.TryGetCell(n, out var ncell))
                        continue;
                    if (map.IsAboveHeightMap(n))
                        return null;
                    if (ncell.IsRoomBorder)
                        edges.Add(n);
                    else
                    {
                        interior.Add(n);
                        toHandle.Enqueue(n);
                    }
                }
            }
            var room = new Room(map);
            foreach (var p in interior)
                room.AddPosition(p);
            foreach (var p in edges)
                room.AddEdge(p);
            room.Validate();
            return room;
        }
        public bool Contains(Vector3 global)
        {
            return this.Interior.Contains(global);
        }
        public bool ContainsBorder(Vector3 global)
        {
            return this.Border.Contains(global);
        }
        internal bool TryRemovePosition(IntVec3 global, out List<Room> newRooms)
        {
            var map = this.Map;
            if (!this.Contains(global))
                throw new Exception();
            newRooms = new List<Room>();
            if (!map.GetCell(global).IsRoomBorder)
                return false;
            this.RemovePosition(global);
            foreach(var n in global.GetAdjacentLazy())
            { 
                if (!map.TryGetCell(n, out var cell))
                    continue;
                if (cell.IsRoomBorder)
                    continue;
                //check if still connected
                var area = FloodFill.BeginExclusiveAsList(map, n);
                if(area is not null)
                    if (this.Interior.Any(p => !area.Contains(p)))
                    {
                        // determine which is the dominant room
                        if (area.Count > (float)this.Interior.Count / 2) // if current room is larger 
                        {
                            var oldPositions = this.Interior;
                            this.Interior = area;

                            oldPositions.RemoveWhere(p => area.Contains(p));
                            newRooms.Add(new Room(map, oldPositions));
                        }
                        else
                        {
                            newRooms.Add(new Room(map, area));
                                this.RemovePositions(this.Interior.Where(area.Contains).ToList());
                        }
                    }
            }
            return newRooms.Any();
        }

        internal void Draw(Camera cam)
        {
            this.Cells.DrawBlocks(this.Map, cam);
        }

        public void Validate()
        {
            this.Valid = true;
            this.value = 0;
            this.Furnitures.Clear();
            this.Border.Clear();
            var furnitureMultiplier = 10;
            foreach (var pos in this.Interior)
            {
                var cell = this.Map.GetCell(pos);
                var material = cell.Material;
                var value = material.Value;
                if (cell.Block.Furniture is FurnitureDef furn)
                {
                    this.Furnitures.Add(furn);
                    value *= furnitureMultiplier;
                }
                this.value += value;
                this.DetectBorders(pos);
            }
            this.value /= 20;
            if (this.roomRole is not null)
                if (!this.roomRole.Furniture.IsSubsetOf(this.Furnitures))
                    this.RoomRole = null;

            this.Cells = new(this.Interior);
            this.InvalidateBorderCells(); /// added for wall hiding, so that wall hidable blocks are drawn in a separate hidable mesh
        }
        void InvalidateBorderCells()
        {
            foreach (var b in this.Border)
                this.Map.InvalidateCell(b);
        }
        void DetectBorders(IntVec3 g)
        {
            foreach(var n in g.GetAdjacentCubeLazy())// g.GetAdjacentLazy())
                if (this.Map.GetCell(n).IsRoomBorder)
                    this.Border.Add(n);
        }
        public void TabGetter(Action<string, Action> getter)
        {
            throw new NotImplementedException();
        }
        public Actor Owner
        {
            get => GetOwner();
            set => this.OwnerRef = value?.RefId ?? -1;
        }

        public Actor GetOwner()
        {
            return this.Map.World.GetEntity(this.OwnerRef) as Actor;
        }
        public override string ToString()
        {
            return $"Room [id:{this.ID}][size:{this.Interior.Count}][owner:{this.GetOwner()?.Name ?? "<none>"}]";
        }
        public Control GetControl()
        {
            return new Label(this);
        }
        public void Refresh(IntVec3 center)
        {
            this.Valid = false;
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(this.ID.Save("ID"));
            tag.Add(this.Interior.Save("Positions"));
            tag.Add(this.OwnerRef.Save("OwnerRef"));
            tag.Add("Workplace", this.workplaceID);
            tag.Add("RoomDef", this.roomRole?.Name ?? "");
            return tag;
        }
        public ISaveable Load(SaveTag tag)
        {
            tag.TryGetTagValueOrDefault<int>("ID", out this.ID);
            tag.TryGetTag("Positions", t => this.Interior = new HashSet<IntVec3>().LoadIntVecs(t));
            this.Cells = new(this.Interior);
            tag.TryGetTagValueOrDefault("OwnerRef", out this.OwnerRef);
            tag.TryGetTagValue<int>("Workplace", ref this.workplaceID);
            tag.TryGetTagValue<string>("RoomDef", v => this.roomRole = !v.IsNullEmptyOrWhiteSpace() ? Def.GetDef<RoomRoleDef>(v) : null);
            return this;
        }
        public void Write(IDataWriter w)
        {
            w.Write(this.ID);
            w.Write(this.Interior);
            w.Write(this.OwnerRef);
            w.Write(this.workplaceID);
            w.Write(this.roomRole?.Name ?? "");
        }
        public ISerializableNew Read(IDataReader r)
        {
            this.ID = r.ReadInt32();
            this.Interior = new HashSet<IntVec3>().ReadIntVec3(r);
            this.Cells = new(this.Interior);
            this.OwnerRef = r.ReadInt32();
            this.workplaceID = r.ReadInt32();
            this.roomRole = (r.ReadString() is string s && !s.IsNullEmptyOrWhiteSpace()) ? Def.GetDef<RoomRoleDef>(s) : null;
            return this;
        }

        public bool IsWallHidable(IntVec3 global, int cameraRot)
        {
            if (!this.Border.Contains(global))
                throw new Exception();
            var map = this.Map;
            var south = global + Coords.Rotate(IntVec3.UnitY, cameraRot); // cache these 2
            var east = global + Coords.Rotate(IntVec3.UnitX, cameraRot);
            var scell = map.GetCell(south);
            var ecell = map.GetCell(east);
            var above = global.Above;
            var aboveCell = map.GetCell(above);
            return
                aboveCell.Block is BlockAir && !this.Interior.Contains(above) ||
                scell.Block is BlockAir && !this.Interior.Contains(south) ||
                ecell.Block is BlockAir && !this.Interior.Contains(east);
        }

        internal void ShowGUI(IntVec3 global)
        {
            var gui = GUI ??= Create();
            gui.GetData((this.Map, global));
            gui.GetWindow().Show();
        }

        static GroupBox Create()
        {
            var box = new GroupBox();
            Room currentRoom = null;
            IntVec3 center = default;
            box.AddControlsVertically(
                new ComboBoxNewNew<RoomRoleDef>(128, "Role", r => r?.Label ?? "none", setRoomDef, () => currentRoom?.RoomRole, () => currentRoom.Furnitures.SelectMany(f => RoomRoleDef.ByFurniture(f)).Distinct().Prepend(null)),
                new ComboBoxNewNew<Actor>(128, "Owner", a => a?.Name ?? "none", setOwner, () => currentRoom?.GetOwner(), () => currentRoom?.Map.Town.GetMembers().Prepend(null)),
                new ComboBoxNewNew<Workplace>(128, "Workplace", w => w?.Name ?? "none", setWorkplace, () => currentRoom?.Workplace, () => currentRoom.Map.Town.ShopManager.GetShops().Where(sh => sh.IsValidRoom(currentRoom)).Prepend(null)),
                new Label(() => $"Interior: {currentRoom?.Interior.Count} cells"),
                new Label(() => $"Edges: {currentRoom?.Border.Count} cells"),
                new Label(() => $"Value: {currentRoom?.Value}"),
                new Button("Refresh", refresh)
                );
            box.SetGetDataAction(o =>
            {
                var oo = ((MapBase map, IntVec3 global))o;
                var map = oo.map;
                var global = oo.global;
                currentRoom = map.Town.RoomManager.GetRoomAt(global);
                center = global;
                box.Tag = currentRoom;
                box.GetWindow().SetTitle(currentRoom.GetName());
            });
            box.ToWindow("Room settings");
            return box;

            void setRoomDef(RoomRoleDef rdef) => Packets.SetRoomType(currentRoom.Map.Net, currentRoom.Map.Net.CurrentPlayer, currentRoom, rdef);
            void setOwner(Actor actor) => Packets.SetOwner(currentRoom.Map.Net, currentRoom.Map.Net.CurrentPlayer, currentRoom, actor);
            void setWorkplace(Workplace wplace) => Packets.SetWorkplace(currentRoom.Map.Net, currentRoom.Map.Net.CurrentPlayer, currentRoom, wplace);
            void refresh() => Packets.Refresh(currentRoom.Map.Net, currentRoom.Map.Net.GetPlayer(), currentRoom, center);
        }

        public IEnumerable<(string name, Action action)> GetInfoTabs()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Control> GetSelectionDetails()
        {
            yield break;
        }

        public static ISerializableNew Create(IDataReader r) => new Room().Read(r);
    }
}

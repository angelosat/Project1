using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using System.IO;

namespace Start_a_Town_
{
    abstract public class Zone : Inspectable, ISelectable, ISaveable, ISerializable
    {
        public Town Town => this.Manager.Town;
        public MapBase Map => this.Town.Map;
        public INetPeer Net => this.Map.Net;
        public override string Label => this.Name;
        public readonly DrawableCellCollection Positions = new(Block.FaceHighlights[IntVec3.UnitZ]);
        public string Name;
        public ZoneManager Manager;
        public int ID { get; set; }
        public bool Hide;
        static readonly Random Random = new();
        public abstract ZoneDef ZoneDef { get; }
        public abstract string UniqueName { get; }
        public abstract string GetName();

        public bool Exists => this.Manager.Zones.ContainsKey(this.ID);

        protected Zone()
        {
            this.Positions.Color = GetRandomColor();
        }
        public Zone(ZoneManager manager) : this()
        {
            this.Manager = manager;
        }
        public Zone(ZoneManager manager, IEnumerable<IntVec3> cells) : this()
        {
            this.Manager = manager;
            this.Positions.Add(cells);
        }

        public IntVec3 Average()
        {
            return this.Positions.Average();
        }
        public void Delete()
        {
            this.Manager.Delete(this);
        }

        private static Color GetRandomColor()
        {
            var array = new byte[3];
            Random.NextBytes(array);
            var col = new Color(array[0], array[1], array[2]);
            return col;
        }
      
        internal virtual void OnBlockChanged(IntVec3 global)
        {
            var map = this.Map;
            var below = global.Below;
            if (this.Positions.Contains(global) && !Block.IsBlockSolid(map, global))
            {
                this.RemovePosition(global);
                return;
            }
            else if (this.Positions.Contains(below) && !map.IsAir(global))
            {
                this.RemovePosition(below);
                return;
            }
        }

        public void RemovePosition(IntVec3 pos)
        {
            this.RemovePositions(new[] { pos });
        }
        public void RemovePositions(IEnumerable<IntVec3> positions)
        {
            foreach (var pos in positions)
                this.Positions.Remove(pos);
            if (!this.Positions.Any())
            {
                this.Delete();
                return;
            }
            var splitgraphs = this.Positions.GetAllConnectedSubGraphs();
            if (splitgraphs.Count == 1)
                return;
            var largest = splitgraphs.OrderByDescending(g => g.Count).First();
            foreach (var pos in this.Positions.Except(largest).ToList())
                this.Positions.Remove(pos);
        }

        internal void Edit(IntVec3 begin, IntVec3 end, bool remove)
        {
            var inputpositions = begin.GetBoxLazy(end);

            if (!remove)
            {
                var finalPositions = inputpositions.Where(pos => this.Town.GetZoneAt(pos) == null).Union(this.Positions);
                if (!finalPositions.IsConnectedNew())
                {
                    this.Manager.RegisterNewZone(this.ZoneDef, inputpositions);
                    return;
                }
                foreach (var pos in inputpositions.Except(this.Positions))
                    if (this.Town.GetZoneAt(pos) is null)
                        this.Positions.Add(pos);
            }
            else
                this.RemovePositions(inputpositions);
        }
        public void Invalidate()
        {
            this.Validate();
        }
        protected virtual void Validate() { }
        internal void OnBlockChangedNew(IntVec3 pos)
        {
            if (!this.Positions.Contains(pos))
                return;
            if (!this.ZoneDef.Worker.IsValidLocation(this.Map, pos))
                this.RemovePosition(pos);
            this.Validate();
        }
        internal bool Contains(GameObject obj)
        {
            return this.Contains(obj.Cell - IntVec3.UnitZ);
        }
        internal bool Contains(IntVec3 pos)
        {
            return this.Positions.Contains(pos);// TODO use a hashset
        }

        internal static bool IsPositionValid(MapBase map, Vector3 pos)
        {
            if (!map.IsSolid(pos))
                return false;
            if (map.IsSolid(pos.Above()))
                return false;
            return true;
        }
        public void RequestDelete()
        {
            PacketZoneDelete.Send(Client.Instance, this.GetType(), this.ID);
        }
        public void Edit()
        {
            ToolManager.SetTool(new ToolDesignateZone(this.Town, this.ZoneDef));// this.GetType()));
        }
        static public void Edit(Town town, ZoneDef def)
        {
            ToolManager.SetTool(new ToolDesignateZone(town, def));// zoneType));
        }

        public virtual void GetSelectionInfo(IUISelection info)
        {
            info.AddInfo(new CheckBoxNew("Hide", () => this.Hide = !this.Hide, () => this.Hide));
        }
        public void GetSelectionInfo(SelectionManagerNew info)
        {
            info.AddInfo(new CheckBoxNew("Hide", () => this.Hide = !this.Hide, () => this.Hide));
        }
        public void GetQuickButtons(SelectionManager info)
        {
            info.AddButtons(new IconButton(Icon.Cross) { LeftClickAction = this.RequestDelete, HoverText = "Delete" });
            info.AddButtons(new IconButton(Icon.Construction) { LeftClickAction = this.Edit, HoverText = "Edit" });
        }
        public void GetQuickButtons(SelectionManagerNew info)
        {
            info.AddButtons(new IconButton(Icon.Cross) { LeftClickAction = this.RequestDelete, HoverText = "Delete" });
            info.AddButtons(new IconButton(Icon.Construction) { LeftClickAction = this.Edit, HoverText = "Edit" });
        }
        public void TabGetter(Action<string, Action> getter)
        {
            throw new NotImplementedException();
        }
        internal void OnCameraRotated(Camera cam)
        {
            this.Positions.Invalidate();
        }
        internal void DrawBeforeWorld(MySpriteBatch sb, MapBase map, Camera cam)
        {
            if (this.Hide)
                return;
            this.Positions.DrawBlocks(map, cam);
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.ID.Save(tag, "ID");
            this.Name.Save(tag, "Name");
            this.Positions.Save(tag, "Positions");
            this.Hide.Save(tag, "Hide");
            this.SaveExtra(tag);
            return tag;
        }
        protected virtual void SaveExtra(SaveTag tag) { }

        public ISaveable Load(SaveTag tag)
        {
            this.ID = tag.GetValue<int>("ID");
            tag.TryGetTagValueNew("Name", ref this.Name);
            //tag.TryGetTagValue<List<SaveTag>>("Positions", v => this.Positions = new HashSet<IntVec3>(new List<Vector3>().Load(v).Select(i => (IntVec3)i)));
            tag.TryGetTag("Positions", v => this.Positions.LoadIntVecs(v));
            this.Hide.TryLoad(tag, "Hide");
            this.LoadExtra(tag);
            return this;
        }
        protected virtual void LoadExtra(SaveTag tag) { }

        public void Write(BinaryWriter w)
        {
            w.Write(this.ID);
            w.Write(this.Name);
            w.Write(this.Hide);
            this.Positions.Write(w);
            this.WriteExtra(w);
        }
        protected virtual void WriteExtra(BinaryWriter w) { }

        public ISerializable Read(BinaryReader r)
        {
            this.ID = r.ReadInt32();
            this.Name = r.ReadString();
            this.Hide = r.ReadBoolean();
            this.Positions.Read(r);
            this.ReadExtra(r);
            return this;
        }
        protected virtual void ReadExtra(BinaryReader r) { }

        public virtual IEnumerable<(string name, Action action)> GetInfoTabs()
        {
            yield break;
        }
        public abstract bool Accepts(Entity obj, IntVec3 pos);

        public IEnumerable<Control> GetSelectionDetails()
        {
            yield break;
        }

     

      
    }
}

using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Graphics;
using Project1.Core.Helpers;
using Project1.Core.Input;
using Project1.Core.Input.CellRendering;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Core.Towns.Tools;
using Project1.Core.UI;
using Project1.Core.UI.Hud;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns.Zones
{
    abstract public class Zone : Inspectable, ISelectable, ISaveable, ISaveableNewNew<Zone>, ISerializableNew<Zone>
    {
        public ZoneManager Manager;
        public bool Exists => this.Manager.ZonesById.ContainsKey(this.ID);
        public Town Town => this.Manager.Town;
        public MapBase Map => this.Town.Map;
        public NetEndpoint Net => this.Map.Net;
        public override string LabelReadable => this.Name;
        public readonly DrawableCellCollection Cells = new(Block.FaceHighlights[IntVec3.UnitZ]);
        public HashSet<IntVec3> EmptyCells = [];
        public string Name { get; set; }
        public int ID { get; set; }
        public bool Hide;
        static readonly Random Random = new();
        internal readonly HashSet<Entity> CacheNew = [];
        public IReadOnlyList<Entity> Items => [.. this.CacheNew];
        public abstract ZoneDef ZoneDef { get; }
        public abstract string UniqueName { get; }
        protected bool _dirty = true;
        public IntVec3 this[int index] => this.Cells[index];
        public bool IsEmpty => this.Cells.Count == 0;

        public Vector3 Global => this.Cells.First();

        protected Zone()
        {
            this.Cells.Color = GetRandomColor();
        }
        public Zone(ZoneManager manager) : this()
        {
            this.Manager = manager;
        }
        public IntVec3 Average()
        {
            return this.Cells.Average();
        }
        private static Color GetRandomColor()
        {
            var array = new byte[3];
            Random.NextBytes(array);
            var col = new Color(array[0], array[1], array[2]);
            return col;
        }
        public void RemovePosition(IntVec3 pos)
        {
            this.RemovePositions([pos]);
        }
        public void RemovePositions(IEnumerable<IntVec3> positions)
        {
            foreach (var pos in positions)
                this.Cells.Remove(pos);
            if (this.Cells.Count == 0)
            {
                //this.Delete();
                return;
            }
            var splitgraphs = this.Cells.GetAllConnectedSubGraphs();
            if (splitgraphs.Count == 1)
                return;
            var largest = splitgraphs.OrderByDescending(g => g.Count).First();
            foreach (var pos in this.Cells.Except(largest).ToList())
                this.Cells.Remove(pos);
        }
        internal void Edit(IntVec3 begin, IntVec3 end, bool remove)
        {
            var inputpositions = begin.GetBoxLazy(end);

            if (!remove)
            {
                var finalPositions = inputpositions.Where(pos => this.Town.GetZoneAt(pos) == null).Union(this.Cells);
                if (!finalPositions.IsConnectedNew())
                {
                    this.Manager.CreateZone(this.ZoneDef, inputpositions);
                    return;
                }
                foreach (var pos in inputpositions.Except(this.Cells))
                    if (this.Town.GetZoneAt(pos) is null)
                        this.Cells.Add(pos);
            }
            else
                this.RemovePositions(inputpositions);
        }
        public void MarkDirty()
        {
            this._dirty = true;
        }
        protected virtual void Validate() { }
        internal void OnBlockChangedNew(IntVec3 pos)
        {
            if (!this.Cells.Contains(pos))
                return;
            if (!this.ZoneDef.Worker.IsValidLocation(this.Map, pos))
                this.RemovePosition(pos);
            this.MarkDirty();
        }
        internal bool Contains(Entity obj)
        {
            return this.Contains(obj.Cell - IntVec3.UnitZ);
        }
        internal bool Contains(IntVec3 pos)
        {
            return this.Cells.Contains(pos);// TODO use a hashset
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
            PacketPlayerZoneDelete.Send(Client.Instance, this.GetType(), this.ID);
        }
        public void Edit()
        {
            ToolManager.SetTool(new ToolDesignateZone(this.Town, this.ZoneDef));
        }
        static public void Edit(Town town, ZoneDef def)
        {
            ToolManager.SetTool(new ToolDesignateZone(town, def));
        }
        public virtual void GetSelectionInfo(IUISelection info)
        {
            info.AddInfo(new CheckBoxNew("Hide", () => this.Hide = !this.Hide, () => this.Hide));
        }
        public IEnumerable<Control> GetSelectionInfo()
        {
            yield return new CheckBoxNew("Hide", () => this.Hide = !this.Hide, () => this.Hide);
        }
        public void GetQuickButtons(SelectionManager info)
        {
            info.AddButtons(new IconButton(Icon.Cross) { LeftClickAction = this.RequestDelete, HoverText = "Delete" });
            info.AddButtons(new IconButton(Icon.Construction) { LeftClickAction = this.Edit, HoverText = "Edit" });
        }
        internal void OnCameraRotated(Camera cam)
        {
            this.Cells.Invalidate();
        }
        internal void DrawBeforeWorld(MySpriteBatch sb, MapBase map, Camera cam)
        {
            if (this.Hide)
                return;
            this.Cells.DrawBlocks(map, cam);
        }
        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.ZoneDef.Save(tag, "Def");
            this.ID.Save(tag, "ID");
            this.Name.Save(tag, "Name");
            this.Cells.Save(tag, "Positions");
            this.Hide.Save(tag, "Hide");
            this.SaveExtra(tag);
            return tag;
        }
        protected virtual void SaveExtra(SaveTag tag) { }
        public ISaveable Load(SaveTag tag)
        {
            this.ID = tag.GetValue<int>("ID");
            if (tag.TryGetTagValueOut("Name", out string name)) this.Name = name;
            if (tag.TryGetTag("Positions", out SaveTag t)) this.Cells.LoadIntVecs(t);
            this.Hide.TryLoad(tag, "Hide");
            this.LoadExtra(tag);
            return this;
        }
        public static Zone Create(SaveTag tag)
        {
            var zoneDef = tag.LoadDef<ZoneDef>("Def");
            var zone = zoneDef.CreateRuntimeWrapper();
            zone.Load(tag);
            return zone;
        }
        protected virtual void LoadExtra(SaveTag tag) { }
        public void Write(IDataWriter w)
        {
            w.Write(this.ZoneDef);
            w.Write(this.ID);
            w.Write(this.Name);
            w.Write(this.Hide);
            this.Cells.Write(w);
            this.WriteExtra(w);
        }
        protected virtual void WriteExtra(IDataWriter w) { }
        protected virtual void ReadExtra(IDataReader r) { }
        public virtual IEnumerable<(string name, Action action)> GetInfoTabs()
        {
            yield break;
        }
        public abstract bool Accepts(Entity obj, IntVec3 pos);
        public IEnumerable<Control> GetSelectionDetails()
        {
            yield break;
        }
        public Zone Read(IDataReader r)
        {
            this.ID = r.ReadInt32();
            this.Name = r.ReadString();
            this.Hide = r.ReadBoolean();
            this.Cells.Read(r);
            this.ReadExtra(r);
            return this;
        }
        public static Zone Create(IDataReader r)
        {
            var zoneDef = r.ReadDef<ZoneDef>();
            var zone = zoneDef.CreateRuntimeWrapper();
            zone.Read(r);
            return zone;
        }
        public IEnumerable<IconButton> GetMiniButtons()
        {
            yield break;
        }
    }
}
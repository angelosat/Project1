using Microsoft.Xna.Framework;
using Project1.Core.AI.Behaviors.Pathing;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Interactions;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Core.UI;
using Project1.Core.UI.Hud;
using Project1.Framework;
using Project1.Framework.Interfaces;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

#nullable enable

namespace Project1.Core
{
    public enum TargetType { Null, Entity, Slot, BlockEntitySlot, Cell, Direction, BlockEntity }

    public class TargetArgs : Inspectable, ITooltippable, IContextable, ISelectable, ILabeled, ISerializableNewNew<TargetArgs>
    {
        public void GetTooltipInfo(Control tooltip)
        {
            switch (this.Type)
            {
                case TargetType.Entity:
                    if (this.Object != null)
                        this.Object.GetTooltipInfo(tooltip);
                    break;

                case TargetType.Cell:
                    this.Map.GetBlock(this.Global).GetTooltip(tooltip, this.Map, this.Global, this.Face);

                    break;

                default: break;
            }
            this.Map.Town.OnTooltipCreated(tooltip, this);
            return;
        }
        

        public INetEndpoint Network
        {
            get { return this.Map.Net; }
        }
        public bool IsResolved => this.World is not null;
        
        public Vector2 Direction;
        public TargetType Type { get; private set; }
        Vector3 _global;
        public Vector3 Global
        {
            get
            {
                if (this.Type == TargetType.Slot)
                    if (this.Slot.Object != null)
                        return this.Slot.Object.Global;
                if (this.Type == TargetType.Entity)
                    return this.Object.Global;
                return this._global;
            }
            set => this._global = value;
        }
        public void InitializeProvider(WorldBase world)
        {
            if (this.World is not null)
                throw new Exception();
            this.World = world;
        }
        [Obsolete]
        public void InitializeProvider(INetEndpoint net)
        {
            this.InitializeProvider(net.World);
        }
        public void ResolveReferences(MapBase map)
        {
            this._resolvedMap = map;
            this.World = map.World;
        }
        public WorldBase World;// { get => this.Map.World; set { } }
        int _entityID = -1, _mapID = -1;
        public int EntityID
        {
            get => _entityID;
            set
            {
                this._entityID = value;
                this._resolvedEntity = null;
            }
        }
        public int MapID
        {
            get => _mapID;
            set
            {
                this._mapID = value;
                this._resolvedMap = null;
            }
        }
        MapBase? _resolvedMap;
        //public MapBase? Map => this._resolvedMap ??= this.Provider.Map;
       public MapBase? Map
        {
            get
            {
                return Type switch
                {
                    TargetType.Entity => this._resolvedEntity?.Map,
                    TargetType.Cell => _resolvedMap,// ??= this.World.Map,
                    TargetType.BlockEntity => _resolvedMap,
                    _ => null
                };
            }
            set
            {
                this._mapID = value?.ID ?? -1;
                this._resolvedMap = value;
            }
        }
        public BlockEntity BlockEntity;
        GameObject? _resolvedEntity;
        public GameObject? Entity => this._resolvedEntity ??= this.World?.GetEntity(this.EntityID);
        public GameObject Object
        {
            get
            {
                if (this.Type == TargetType.Entity)
                {
                    if (this.EntityID == -1)
                        throw new Exception();
                    if (this._resolvedEntity == null)
                        this._resolvedEntity = this.World.GetEntity(this.EntityID);
                }

                else if (this.Type == TargetType.Slot || this.Type == TargetType.BlockEntitySlot)
                {
                    if (this._resolvedEntity == null)
                        this._resolvedEntity = this.Slot.Object;
                }
                return this._resolvedEntity;
            }
        }

        public Vector3 Face;
        public Vector3 Precise;

        int ParentID, ContainerID, SlotID;
        string ContainerName;
        GameObjectSlot _Slot;
        public GameObjectSlot Slot
        {
            get
            {
                if (this._Slot != null)
                {
                    return this._Slot;
                }
                switch (this.Type)
                {
                    case TargetType.Slot:
                        GameObject parent = this.Network.World.GetEntity(this.ParentID);
                        return parent.GetChild(this.ContainerID, this.SlotID);

                    case TargetType.BlockEntitySlot:
                        var blockentity = this.Map.GetBlockEntity(this.Global);
                        return blockentity.GetChild(this.ContainerName, this.SlotID);

                    default:
                        return null;
                }
            }
            set { this._Slot = value; }
        }
        public bool TryGetBlockEntity(out BlockEntity blockEntity) => this.Map.TryGetBlockEntity(this.Global, out blockEntity);
        
        TargetArgs() { }
        public TargetArgs(BlockEntity blockEntity)
        {
            this.BlockEntity = blockEntity;
            this.Type = TargetType.BlockEntity;
            this._resolvedMap = blockEntity.Map;
        }
        public TargetArgs(GameObject obj)
        {
            this.Type = TargetType.Entity;
            this.EntityID = obj.RefId;
            this._resolvedEntity = obj;
            this._resolvedMap = obj.Map;
            this.World = obj.World;
            // struct assignments
            this._global = Vector3.Zero;
            this.Face = Vector3.Zero;
            this.Precise = Vector3.Zero;
            this.Direction = Vector2.Zero;
            this.ParentID = -1;
            this.ContainerID = -1;
            this._Slot = null;
            this.SlotID = -1;
            this.ContainerName = "";
        }
        public TargetArgs(EntityRefId entityID)
        {
            this.Type = TargetType.Entity;
            this.EntityID = entityID;
        }
        public TargetArgs(WorldBase world, EntityRefId entityID)
        {
            this.World = world;
            this.Type = TargetType.Entity;
            this.EntityID = entityID;
        }
        public TargetArgs(GameObject obj, Vector3 face)
        {
            this.Type = TargetType.Entity;
            this.EntityID = obj.RefId;
            this.Global = obj.Global;
            this._resolvedEntity = obj;
            this.World = obj.World;
        }
        public TargetArgs(GameObject obj, Vector3? face)
        {
            this.Type = TargetType.Entity;
            this.EntityID = obj.RefId;
            this.Global = obj.Global;
            this.Face = face.HasValue ? face.Value : Vector3.Zero;
            this._resolvedEntity = obj;
        }
        public TargetArgs(WorldBase provider, Vector3 global)
        {
            this.World = provider;
            this.Type = TargetType.Cell;
            this.Global = global;
        }
        public TargetArgs(MapBase map, Vector3 global)
        {
            this._resolvedMap = map;
            this.World = map.World;
            this.Type = TargetType.Cell;
            this.Global = global;
        }
        public TargetArgs(Vector3 global, Vector3 face)
        {
            this.Type = TargetType.Cell;
            this.Global = global;
            this.Face = face;
        }
        public TargetArgs(Vector3 global, Vector3 face, Vector3 precise)
        {
            this.Type = TargetType.Cell;
            this.Global = global;
            this.Face = face;
            this.Precise = precise;
        }
        public TargetArgs(WorldBase world, Vector3 global, Vector3 face, Vector3 precise)
        {
            this.World = world;
            this.Type = TargetType.Cell;
            this.Global = global;
            this.Face = face;
            this.Precise = precise;
        }
        public TargetArgs(MapBase map, Vector3 global, Vector3 face, Vector3 precise)
        {
            this.Type = TargetType.Cell;
            this._resolvedMap = map;
            this.Global = global;
            this.Face = face;
            this.Precise = precise;
            this.World = map.World;
        }
        public TargetArgs(WorldBase world, GameObjectSlot slot)
        {
            this.World = world;
            this.Type = TargetType.Slot;
            this.Slot = slot;
        }
        public TargetArgs(GameObjectSlot slot)
        {
            this.World = slot.Owner.World;
            this.Type = TargetType.Slot;
            this.Slot = slot;
        }
        public TargetArgs(Vector2 direction)
        {
            this.Type = TargetType.Direction;
            this.Direction = direction;
        }

        public TargetArgs Clone()
        {
            var copy = new TargetArgs
            {
                Type = this.Type,
                Global = this.Global,
                Face = this.Face,
                Precise = this.Precise,
                ContainerName = this.ContainerName,
                ContainerID = this.ContainerID,
                SlotID = this.SlotID,
                Direction = this.Direction,
                EntityID = this.EntityID,
                _resolvedMap = this._resolvedMap
            };
            return copy;
        }
        public TargetArgs(Vector3 global, GameObjectSlot slot)
            : this(null, global, slot)
        {

        }
        [Obsolete("world object will eventually not have a singular map field")]
        public TargetArgs(NetEndpoint provider, Vector3 global, GameObjectSlot slot)
        {
            this.World = provider.World;
            this.Map = provider.Map;
            if (slot == null)
                throw new Exception();
            this.Type = TargetType.BlockEntitySlot;
            this.Global = global;
            this.ContainerName = slot.ContainerNew.Name;
            this.SlotID = slot.ID;
        }

        public Cell Cell => this.Map.GetCell(this.Global);

        public TargetArgs Write(BinaryWriter w)
        {
            w.Write((int)this.Type);
            switch (this.Type)
            {
                case TargetType.Slot:

                    w.Write(this.Slot.Owner.RefId);
                    w.Write(this.Slot.ID);
                    w.Write(this.Slot.ContainerNew.ID);
                    return this;

                case TargetType.Cell:
                    w.Write(this.Global);
                    w.Write(this.Face);
                    w.Write(this.Precise);
                    return this;

                case TargetType.Entity:

                    w.Write(this.EntityID);
                    return this;

                case TargetType.Direction:
                    w.Write(this.Direction);
                    return this;

                case TargetType.BlockEntitySlot:
                    w.Write(this.Global);
                    w.Write(this.Slot.ContainerNew.Name);
                    w.Write(this.Slot.ID);
                    return this;

                default:
                    return this;
            }
        }
        public TargetArgs Write(IDataWriter w)
        {
            w.Write((int)this.Type);
            switch (this.Type)
            {
                case TargetType.Slot:

                    w.Write(this.Slot.Owner.RefId);
                    w.Write(this.Slot.ID);
                    w.Write(this.Slot.ContainerNew.ID);
                    return this;

                case TargetType.Cell:
                    w.Write(this.Global);
                    w.Write(this.Face);
                    w.Write(this.Precise);
                    return this;

                case TargetType.Entity:

                    w.Write(this.EntityID);
                    return this;

                case TargetType.Direction:
                    w.Write(this.Direction);
                    return this;

                case TargetType.BlockEntitySlot:
                    w.Write(this.Global);
                    w.Write(this.Slot.ContainerNew.Name);
                    w.Write(this.Slot.ID);
                    return this;

                //case TargetType.BlockEntity:
                //    w.Write(this.BlockEntity.CellsOccupied.First());
                //    return this;

                default:
                    return this;
            }
        }
        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name, this.SaveAsList());
            return tag;
        }
        public List<SaveTag> SaveAsList()
        {
            var tag = new List<SaveTag>();
            tag.Add(new SaveTag(SaveTag.Types.Int, "Type", (int)this.Type));
            switch (this.Type)
            {
                case TargetType.Slot:
                    tag.Add(new SaveTag(SaveTag.Types.Int, "ParentID", this.Slot.Owner.RefId));
                    tag.Add(new SaveTag(SaveTag.Types.Int, "SlotID", this.Slot.ID));
                    tag.Add(new SaveTag(SaveTag.Types.Int, "ContainerID", this.Slot.ContainerNew.ID));
                    break;

                case TargetType.Cell:
                    tag.Add(new SaveTag(SaveTag.Types.Vector3, "Global", this.Global));
                    tag.Add(new SaveTag(SaveTag.Types.Vector3, "Face", this.Face));
                    tag.Add(new SaveTag(SaveTag.Types.Vector3, "Precise", this.Precise));
                    break;

                case TargetType.Entity:
                    tag.Add(new SaveTag(SaveTag.Types.Int, "InstanceID", this.EntityID));
                    break;

                case TargetType.Direction:
                    tag.Add(new SaveTag(SaveTag.Types.Vector3, "Direction", new Vector3(this.Direction, 0)));
                    break;

                case TargetType.BlockEntitySlot:
                    tag.Add(new SaveTag(SaveTag.Types.Vector3, "Global", this.Global));
                    tag.Add(new SaveTag(SaveTag.Types.String, "ContainerName", this.Slot.ContainerNew.Name));
                    tag.Add(new SaveTag(SaveTag.Types.Int, "SlotID", this.Slot.ID));
                    break;

                default:
                    break;

            }
            return tag;
        }
        static public TargetArgs Read(NetEndpoint provider, IDataReader reader)
        {
            return Read(provider.Map, reader);
        }
        static public TargetArgs Read(MapBase map, IDataReader reader)
        {
            ArgumentNullException.ThrowIfNull(map);
            TargetType type = (TargetType)reader.ReadInt32();
            switch (type)
            {
                case TargetType.Null:
                    return TargetArgs.Null;

                case TargetType.Entity:
                    int netID = reader.ReadInt32();
                    return new TargetArgs(map.World, netID);

                case TargetType.Cell:
                    return new TargetArgs(map.World, reader.ReadVector3(), reader.ReadVector3(), reader.ReadVector3()) { Map = map };

                case TargetType.Slot:
                    int parentID = reader.ReadInt32();
                    GameObject parent = map.World.GetEntity(parentID);
                    byte slotID = reader.ReadByte();
                    int containerID = reader.ReadInt32();
                    var slot = parent.GetChild(containerID, slotID);
                    return new TargetArgs(map.World, slot);

                case TargetType.BlockEntitySlot:
                    var vector3 = reader.ReadVector3();
                    var blockentity = map!.GetBlockEntity(vector3);
                    var containerName = reader.ReadString();
                    var slotid = reader.ReadByte();
                    var s = blockentity.GetChild(containerName, slotid);
                    return new TargetArgs(map.Net, vector3, s);

                case TargetType.Direction:
                    return new TargetArgs(reader.ReadVector2());

                //case TargetType.BlockEntity:
                //    var global = reader.ReadIntVec3();
                //    var blockentity = 


                default:
                    throw new Exception("Invalid target type " + type.ToString());
            }
        }

        //static public TargetArgs Read(WorldBase provider, IDataReader reader)
        //{

        //    TargetType type = (TargetType)reader.ReadInt32();
        //    switch (type)
        //    {
        //        case TargetType.Null:
        //            return TargetArgs.Null;

        //        case TargetType.Entity:
        //            int netID = reader.ReadInt32();
        //            return new TargetArgs(provider, netID);

        //        case TargetType.Position:
        //            return new TargetArgs(provider, reader.ReadVector3(), reader.ReadVector3(), reader.ReadVector3());// { Map = net.Map };

        //        case TargetType.Slot:
        //            int parentID = reader.ReadInt32();
        //            GameObject parent = provider.GetEntity(parentID);
        //            byte slotID = reader.ReadByte();
        //            int containerID = reader.ReadInt32();
        //            var slot = parent.GetChild(containerID, slotID);
        //            return new TargetArgs(provider, slot);

        //        case TargetType.BlockEntitySlot:
        //            var vector3 = reader.ReadVector3();
        //            var blockentity = provider.Map.GetBlockEntity(vector3);
        //            var containerName = reader.ReadString();
        //            var slotid = reader.ReadByte();
        //            var s = blockentity.GetChild(containerName, slotid);
        //            return new TargetArgs(provider, vector3, s);

        //        case TargetType.Direction:
        //            return new TargetArgs(reader.ReadVector2());

        //        default:
        //            throw new Exception("Invalid target type " + type.ToString());
        //    }
        //}

        public void Load(SaveTag tag)
        {
            this.Type = (TargetType)tag.GetValue<int>("Type");
            switch (this.Type)
            {
                case TargetType.Entity:
                    this.EntityID = tag.GetValue<int>("InstanceID");
                    break;

                case TargetType.Cell:
                    this.Global = tag.GetValue<Vector3>("Global");
                    this.Face = tag.GetValue<Vector3>("Face");
                    this.Precise = tag.GetValue<Vector3>("Precise");
                    break;

                case TargetType.Slot:
                    int parentID = tag.GetValue<int>("ParentID");
                    int slotID = tag.GetValue<int>("SlotID");
                    int containerID = tag.GetValue<int>("ContainerID");
                    this.ParentID = parentID;
                    this.SlotID = slotID;
                    this.ContainerID = containerID;
                    break;

                case TargetType.Direction:
                    var dir3d = tag.GetValue<Vector3>("Direction");
                    this.Direction = new Vector2(dir3d.X, dir3d.Y);
                    break;

                case TargetType.BlockEntitySlot:
                    this.Global = tag.GetValue<Vector3>("Global");
                    var containerName = tag.GetValue<string>("ContainerName");
                    var slotid = tag.GetValue<int>("SlotID");
                    this.ContainerName = containerName;
                    this.SlotID = slotid;
                    break;
            }
        }

        public TargetArgs(SaveTag tag)
        {
            this.Load(tag);
        }

        public TargetArgs(WorldBase world, SaveTag tag)
        {
            this.World = world;

            this.Load(tag);
        }
        public TargetArgs(MapBase map, SaveTag tag)
        {
            this.World = map.World;
            this._resolvedMap = map;
            this.Load(tag);
        }
        public Vector3 FinalGlobal
        {
            get
            {
                return this.FaceGlobal + this.Precise;
            }
        }
        /// <summary>
        ///  TODO turn to IntVec3
        /// </summary>
        public Vector3 FaceGlobal
        {
            get
            {
                return this.Global + this.Face;
            }
        }

        static readonly public TargetArgs Null = new();

        public override string LabelReadable
        {
            get
            {
                return this.Type switch
                {
                    TargetType.Entity => this.Object.DebugName,
                    TargetType.Cell => this.Map.GetBlock(this.Global).Name,
                    TargetType.Slot => this.Slot.ToString(),
                    _ => this.Type.ToString(),
                };
            }
        }
        public override string ToString()
        {
            return this.Type switch
            {
                TargetType.Entity => this.Object.DebugName,
                TargetType.Cell => this.FinalGlobal.ToString(),
                TargetType.Slot => this.Slot.ToString(),
                _ => this.Type.ToString(),
            };
        }
        public Dictionary<string, Interaction> GetInteractions(INetEndpoint net)
        {
            switch (this.Type)
            {
                case TargetType.Entity:
                    return this.Object.GetInteractions();

                case TargetType.Cell:
                    Block block = net.Map.GetBlock(this.Global);
                    var inters = block.GetAvailableTasks(net.Map, this.Global).ToDictionary(foo => foo.Name);
                    var dropInter = new UseHauledOnTarget();
                    inters.Add(dropInter.Name, dropInter); // TODO: WORKAROUND until i decide wether to use an interaction registry or add some basic interactions in the base block object
                    return inters;

                default:
                    var list = new Dictionary<string, Interaction>();
                    var dropinvitem = new DropInventoryItem();
                    var dropeq = new InteractionDropEquipped();
                    var throwInter = new InteractionThrow();
                    list.Add(dropinvitem.Name, dropinvitem);
                    list.Add(dropeq.Name, dropeq);
                    list.Add(throwInter.Name, throwInter);
                    return list;
            }
        }
        public Interaction GetInteraction(string name)
        {
            switch (this.Type)
            {
                case TargetType.Entity:
                    Interaction interaction;
                    this.Object.GetInteractions().TryGetValue(name, out interaction);
                    return interaction;

                case TargetType.Cell:
                    var rounded = this.Global.RoundXY();
                    Block block = this.Map.GetBlock(rounded);
                    var tasks = block.GetAvailableTasks(this.Map, rounded);
                    tasks.Add(new UseHauledOnTarget()); // TODO: WORKAROUND until i decide wether to use an interaction registry or add some basic interactions in the base block object
                    return tasks.FirstOrDefault(i => i.Name == name);

                default:
                    return null;
            }
        }
        internal List<Interaction> GetAvailableTasks(INetEndpoint net)
        {
            switch (this.Type)
            {
                case TargetType.Entity:
                    return this.Object.GetAvailableTasks();

                case TargetType.Cell:
                    Block block = net.Map.GetBlock(this.Global);
                    return block.GetAvailableTasks(net.Map, this.Global);

                default:
                    return new List<Interaction>();
            }
        }
        internal void GetContextAll(GameObject playerEntity, ContextArgs args)
        {
            var list = new ContextAction[]{
                this.GetContextRB(playerEntity),
                this.GetContextActivate(playerEntity)
            };
            args.Actions.AddRange(list.Where(i => i != null));
        }

        internal ContextAction GetContextRB(GameObject playerEntity)
        {
            switch (this.Type)
            {
                case TargetType.Entity:
                    return this.Object.GetContextRB(playerEntity);

                case TargetType.Cell:
                    var block = this.Network.Map.GetBlock(this.Global);
                    return block.GetContextRB(playerEntity, this.Global);

                default:
                    return null;
            }
        }

        internal ContextAction GetContextActivate(GameObject playerEntity)
        {
            switch (this.Type)
            {
                case TargetType.Entity:
                    return this.Object.GetContextActivate(playerEntity);

                case TargetType.Cell:
                    var block = this.Network.Map.GetBlock(this.Global);
                    return block.GetContextActivate(playerEntity, this.Global);

                default:
                    return null;
            }
        }

        public void GetContextActions(GameObject playerEntity, ContextArgs a)
        {
            switch (this.Type)
            {
                case TargetType.Entity:
                    this.Object.GetContextActions(playerEntity, a);
                    break;

                case TargetType.Cell:
                    var block = this.Network.Map.GetBlock(this.Global);
                    block.GetContextActions(playerEntity, this.Global, a);
                    // check if block is part of any town designations such as stockpiles or fields, and add corresponding actions
                    this.Map.Town.GetContextActions(playerEntity, this.Global, a);
                    break;

                default:
                    break;
            }
        }



        public bool IsEqual(TargetArgs target)
        {
            if (this.Type != target.Type)
                return false;
            if (this.Type == TargetType.Entity && this.Object != null && this.Object == target.Object)
                return true;
            if (this.Type == TargetType.BlockEntity && this.BlockEntity != null && this.BlockEntity == target.BlockEntity)
                return true;
            else if (this.Type == TargetType.Cell && this.Global == target.Global
                && this.Face == target.Face) // newly added
                return true;
            return false;
        }
        public bool IsEqualFace(TargetArgs target)
        {
            if (this.Type != target.Type)
                return false;
            if (this.Type == TargetType.Entity && this.Object != null && this.Object == target.Object)
                return true;
            else if (this.Type == TargetType.Cell && this.Global == target.Global && this.Face == target.Face)
                return true;
            return false;
        }
        Block BlockCached;
        public Block Block => BlockCached ??= this.Type == TargetType.Cell ? this.Map.GetBlock(this.Global) : null;
        public BlockEntity BlockEntityOld => this.Type == TargetType.Cell ? this.Map.GetBlockEntity(this.Global) : null;
        public RegionNode Node => this.Map.Regions.GetNodeAt(this.Global);
        public Region Region => this.Node?.Region;
        public RegionRoom RegionRoom => this.Region?.Room;

        internal T GetBlockEntity<T>() where T : BlockEntity => this.Map.GetBlockEntity(this.Global) as T;
        internal BlockEntity GetBlockEntity() => this.Map.GetBlockEntity(this.Global);

        public string Name
        {
            get
            {
                return this.Type switch
                {
                    TargetType.Entity => this.Object.Name,
                    TargetType.Cell => this.Block.GetName(this.Map, this.Global),
                    TargetType.BlockEntity => this.BlockEntity.Name,
                    _ => "",
                };
            }
        }

        public IEnumerable<(string name, Action action)> GetInfoTabs()
        {
            switch (this.Type)
            {
                case TargetType.Entity:
                    throw new Exception();

                case TargetType.Cell:
                    foreach (var i in this.Block.GetInfoTabs())
                        yield return i;
                    break;

                default:
                    yield break;
            }
            foreach (var i in this.Map.GetInfoTabs())
                yield return i;
        }
        public void GetSelectionInfo(IUISelection info)
        {
            switch (this.Type)
            {
                case TargetType.Entity:
                    this.Object.GetSelectionInfo(info);
                    break;

                case TargetType.Cell:
                    //this.Block.GetSelectionInfo(info, this.Map, this.Global);
                    this.Cell.GetSelectionInfo(info, this.Map, this.Global);
                    break;

                default:
                    return;
            }
            this.Map.OnTargetSelected(info, this);
        }
        public void GetSelectionInfo(SelectionManager info)
        {
            var box = new GroupBox();
            switch (this.Type)
            {
                case TargetType.Entity:
                    this.Object.GetSelectionInfo(info);
                    break;

                case TargetType.Cell:
                    this.Cell.GetSelectionInfo(box);
                    break;
                  
                case TargetType.BlockEntity:
                    this.BlockEntity.GetSelectionInfo(box);
                    break;

                default:
                    return;
            }
            box.AlignTopToBottom();
            info.AddInfo(box);
            this.Map!.OnTargetSelected(info, this);
        }
        public void GetQuickButtons(SelectionManager info)
        {
            switch (this.Type)
            {
                case TargetType.Entity:
                    this.Object.GetQuickButtons(info);
                    break;

                case TargetType.Cell:
                    this.Block.GetQuickButtons(info, this.Map, this.Global);
                    this.Map.GetQuickButtons((name, guiType) =>
                            //info.AddTabAction(name, () => UIManager.ToggleUnique<WorkstationGuiNew>(new TargetArgs(this.Map, this.BlockEntity.OriginGlobal))), 
                            info.AddTabAction(name, () => UIManager.ToggleUnique(guiType, this)), this.Global);
                    break;

                case TargetType.BlockEntity:
                    //this.BlockEntity.GetQuickButtons(info, this.Map, this.Global);
                    //this.BlockEntity.GetQuickButtons(info.AddTabAction, this.Map, this.Global);
                    this.BlockEntity.GetQuickButtons(
                        (name, guiType) => 
                            //info.AddTabAction(name, () => UIManager.ToggleUnique<WorkstationGuiNew>(new TargetArgs(this.Map, this.BlockEntity.OriginGlobal))), 
                            info.AddTabAction(name, () => UIManager.ToggleUnique(guiType, new TargetArgs(this.Map, this.BlockEntity.OriginGlobal))), 
                        this.Map, 
                        this.Global);
                    break;

                default:
                    return;
            }
        }
        public void TabGetter(Action<string, Action> getter)
        {
            throw new Exception();
        }

        public bool Exists
        {
            get
            {
                return this.Type switch
                {
                    TargetType.Entity => this.Object != null && this.Object.Exists,
                    //TargetType.Position => this.Block != BlockDefOf.Air, /// I WANT TO BE ABLE TO SELECT AIR BLOCKS IN SOME CASES
                    _ => true//throw new Exception(),
                };
            }
        }
        public bool IsForbidden => this.Type == TargetType.Entity && this.Object.IsForbidden;

        public bool HasObject { get { return this.Object != null; } }

        public T GetEntity<T>() where T : GameObject
        {
            return this.Object as T;
        }

        public static implicit operator GameObject(TargetArgs b) => b.Object;
        public static implicit operator Entity(TargetArgs b) => b.Object as Entity;
        public static implicit operator Actor(TargetArgs b) => b.Object as Actor;

        public static implicit operator TargetArgs(GameObject obj)
        {
            return new TargetArgs(obj);
        }
        public static implicit operator TargetArgs((MapBase map, Vector3 global) location)
        {
            return new TargetArgs(location.map, location.global);
        }

        public IEnumerable<Control> GetSelectionDetails()
        {
            switch (this.Type)
            {
                case TargetType.Entity:
                    foreach (var i in this.Object.GetSelectionDetails())
                        yield return i;
                    break;
                case TargetType.Cell:
                    yield break;
                default:
                    yield break;
            }
        }
        public override bool Equals(object obj)
        {
            if (obj is not TargetArgs o) return false;
            if (this.Type != o.Type) return false;

            return Type switch
            {
                TargetType.Entity => this.EntityID == o.EntityID,
                TargetType.BlockEntity => this.BlockEntity.OriginGlobal == o.BlockEntity.OriginGlobal,
                TargetType.Cell => this.Global == o.Global,
                TargetType.Direction => this.Direction == o.Direction,
                _ => false
            };
        }
        public override int GetHashCode() =>
        Type switch
        {
            TargetType.Entity => HashCode.Combine((int)Type, this.EntityID),
            TargetType.BlockEntity => HashCode.Combine((int)Type, this.BlockEntity.OriginGlobal.X, this.BlockEntity.OriginGlobal.Y, this.BlockEntity.OriginGlobal.Z),
            TargetType.Cell => HashCode.Combine((int)Type, this.Global.X, this.Global.Y, this.Global.Z),
            TargetType.Direction => HashCode.Combine((int)Type, this.Direction.X, this.Direction.Y),
            _ => (int)Type
        };

        IDataWriter ISerializableNewNew<TargetArgs>.Write(IDataWriter w)
        {
            w.Write((int)this.Type);
            switch (this.Type)
            {
                case TargetType.Slot:

                    w.Write(this.Slot.Owner.RefId);
                    w.Write(this.Slot.ID);
                    w.Write(this.Slot.ContainerNew.ID);
                    return w;

                case TargetType.Cell:
                    w.Write(this.Global);
                    w.Write(this.Face);
                    w.Write(this.Precise);
                    return w;

                case TargetType.Entity:

                    w.Write(this.EntityID);
                    return w;

                case TargetType.Direction:
                    w.Write(this.Direction);
                    return w;

                case TargetType.BlockEntitySlot:
                    w.Write(this.Global);
                    w.Write(this.Slot.ContainerNew.Name);
                    w.Write(this.Slot.ID);
                    return w;

                default:
                    return w;
            }
        }

        public static TargetArgs Create(IDataReader r)
        {
            var type = (TargetType)r.ReadInt32();
            return type switch
            {
                TargetType.Null => TargetArgs.Null,
                TargetType.Entity => new TargetArgs(r.ReadEntityRefId()),
                TargetType.Cell => new TargetArgs(r.ReadVector3(), r.ReadVector3(), r.ReadVector3()),
                TargetType.Slot => throw new NotImplementedException(),
                //int parentID = r.ReadInt32();
                //GameObject parent = map.World.GetEntity(parentID);
                //byte slotID = r.ReadByte();
                //int containerID = r.ReadInt32();
                //var slot = parent.GetChild(containerID, slotID);
                //return new TargetArgs(map.World, slot);
                TargetType.BlockEntitySlot => throw new NotImplementedException(),
                //var vector3 = r.ReadVector3();
                //var blockentity = map!.GetBlockEntity(vector3);
                //var containerName = r.ReadString();
                //var slotid = r.ReadByte();
                //var s = blockentity.GetChild(containerName, slotid);
                //return new TargetArgs(map.Net, vector3, s);
                TargetType.Direction => new TargetArgs(r.ReadVector2()),
                TargetType.BlockEntity => throw new NotImplementedException(),
                _ => throw new UnreachableException("Invalid target type " + type.ToString()),
            };
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.AI;
using Project1.Core.AI.MetaRoles;
using Project1.Core.Animations;
using Project1.Core.Attributes;
using Project1.Core.Blocks;
using Project1.Core.Components;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Graphics;
using Project1.Core.Helpers;
using Project1.Core.Interactions;
using Project1.Core.Needs;
using Project1.Core.Networking;
using Project1.Core.Networking.Entities;
using Project1.Core.Rendering;
using Project1.Core.Resources;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Core.Simulation.Physics;
using Project1.Core.Skills;
using Project1.Core.Systems.Alchemy;
using Project1.Core.Systems.Consumables.Scrolls;
using Project1.Core.Systems.Equipment;
using Project1.Core.Systems.Inventory;
using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Ownership;
using Project1.Core.Systems.Plants;
using Project1.Core.Systems.Quality;
using Project1.Core.Systems.Tools;
using Project1.Core.Towns;
using Project1.Core.UI;
using Project1.Core.UI.NamePlates;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Project1.Core.Entities;

public abstract class EntityBase : Inspectable
{
    public abstract MapBase Map { get; }
    public abstract Vector3 Global { get; }
}
public interface ITransformAnchor
{
    public abstract MapBase Map { get; }
    public Vector3 Global { get; }
}
public abstract class GameObject : Inspectable, ITransformAnchor, ITooltippable, IContextable, INameplateable, ISlottable, ISelectable//, ILabeled, IInspectable
{
    public static readonly Dictionary<int, GameObject> Templates = [];
    public string DebugName { get { return $"[{this.RefId}]{this.Name}"; } }
    public bool IsRegistered => this.RefId > 0;
    static int GetNextTemplateID()
    {
        return Templates.Count + 1;
    }

    public static void AddTemplates(IEnumerable<GameObject> templates)
    {
        foreach (var o in templates)
            AddTemplate(o);
    }
    public static int AddTemplate(GameObject obj)
    {
        var id = GetNextTemplateID();
        Templates.Add(id, obj);
        return id;
    }
    internal static GameObject CloneTemplate(int templateID)
    {
        return Templates[templateID].Clone();
    }
    public Color GetSlotColor()
        => this.GetNameplateColor();
    //{ return this.GetInfo().GetQualityColor(); }
    public string GetCornerText()
    { return this.StackSize.ToString(); }
    public void DrawUI(SpriteBatch sb, Vector2 pos)
    {
        var sprite = this.GetSpriteOrDefault();
        var source = sprite.GetSourceRect();
        sprite.Draw(sb, pos - new Vector2(source.Width, source.Height) * 0.5f, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
    }

    internal AttributeRuntime GetAttribute(AttributeDef att) => this.GetComponent<AttributesComponent>().GetAttribute(att);
    public ItemDef Def;
    //public QualityDef Quality { get { return this.DefComponent.Quality; } set { this.DefComponent.Quality = value; } }
    //public QualityDef Quality { get => this.QualityComp.Tier; set => this.QualityComp.Tier = value; }
    public GameObjectSlot ToSlotLink(int amount = 1)
    {
        return new GameObjectSlot() { Link = this };
    }
    public Memory ToMemory(GameObject actor)
    {
        return new Memory(this, 100, 100, 1, actor);
    }
    public static void LoadTemplates()
    {
        AddTemplate(EntityFactory.Request(ActorDnaDefOf.Npc, RoleMetaDefOf.Adventurer).Create());

        foreach (var t in MaterialSystem.GenerateTemplates().Where(t => t is not null))
            AddTemplate(t);

        foreach (var toolProp in Core.Def.Get<ToolProfileDef>())
        {
            var obj = ToolSystem.Create(toolProp, MaterialDefOf.LightWood, MaterialDefOf.LightWood);
            AddTemplate(obj);
        }

        AddTemplates(PotionSystem.GenerateTemplates());
        AddTemplates(ScrollSystem.GenerateTemplates());
        AddTemplates(EquipmentSystem.GenerateTemplates());

        AddTemplate(ItemDefOf.Coins.Create());
    }

    #region Common Properties
    public virtual string Name
    {
        //get => $"{this.GetInfo().ParentName}";//{(this.StackSize > 1 ? $" (x{this.StackSize})" : "")}";
        get => $"{this.GetInfo().ParentName}{(this.StackSize > 1 ? $" ({this.StackSize})" : "")}";
        set
        {
            var info = GetInfo();
            info.ParentName = value;
        }
    }
    public override string LabelReadable => this.Name;


    public EntityRefId OwnerId { get; private set; } = EntityRefId.Null;
    public bool HasOwner => this.OwnerId != EntityRefId.Null;
    public void SetOwnerNew(EntityRefId actorId)
    {
        var old = this.OwnerId;
        if (old == actorId)
            return;
        this.OwnerId = actorId;
        this.World.Events.Post(new ItemOwnerChangedEvent(this as Entity, old));
    }

    public void SetOwnerNew(Actor actor)
    {
        if (actor is not null && actor.RefId == EntityRefId.Null)
            throw new InvalidOperationException("Tried to assign an uninitialized owner");
        this.SetOwnerNew(actor?.RefId ?? EntityRefId.Null);
        //var @new = actor?.RefId ?? EntityRefId.Null;

        //var old = this.OwnerId;
        //if (old == @new)
        //    return;
        //this.OwnerId = @new;
        //this.World.Events.Post(new ItemOwnerChangedEvent(this as Entity, old));
    }
    public virtual float Height => this.Def.Height;

    public int RefId;

    public NetEndpoint Net { get => this.World?.Net; set { } }

    public WorldBase World { get; set; }

    //MapBase _map;
    public MapBase LastMap { get; private set; }
    public MapBase Map
    {
        get => this.Transform.Map;
        set
        {
            //this._map = value;
            this.Transform.Map = value;
            if (value is not null)
                this.LastMap = value;
        }
        //get => this._map;
        //set
        //{
        //    this._map = value;
        //    if (value is not null)
        //        this.LastMap = value;
        //}
    }
    public Town Town;
   
    public virtual IEnumerable<Control> GetInspectorControls()
    {
        yield return new LabelNew(() => $"Owner: {this.World.Get(this.OwnerId)?.Name ?? "<unassigned>"}");
        foreach (var comp in this.Components.Values)
        {
            var groupbox = new GroupBox();
            foreach (var ctrl in comp.GetInspectorControls())
                groupbox.AddControlsBottomLeft(ctrl);
            yield return groupbox;
        }
    }
    //public virtual IEnumerable<IconButton> GetMiniButtons()
    //{
    //    yield return IconCameraFollow.Value;
    //}
    public virtual IEnumerable<(string label, Type type)> GetInspectorTabs() { yield break; }
    
    internal void AttackTelegraph(GameObject parent)
    {
        throw new NotImplementedException();
    }
    //static readonly Lazy<IconButton> IconCameraFollow = new(()=> new(Icon.Replace) { BackgroundTexture = UIManager.Icon16Background, LeftClickAction = FollowCam, HoverText = "Camera follow" });
    //static void FollowCam()
    //{
    //    ScreenManager.CurrentScreen.Renderer.ToggleFollowing(SelectionManager.Instance.SelectedSource as GameObject);
    //}
    public void ToggleForbidden()
    {
        this.IsForbidden = !this.IsForbidden;
        this.Map.Events.Post(new EntityForbiddenEvent(this as Entity));
    }
    public void OnNameplateCreated(Nameplate plate)
    {
        plate.Controls.Add(new Label()
        {
            Font = UIManager.FontBold,
            TextFunc = () => this.Name,
            //TextColorFunc = parent.GetNameplateColor,
            //TintFunc = parent.GetNameplateColor, // we dont want tintfunc, we want to change textcolorfunc directly because the default textcolor is UIManager.DefaultTextColor = Color.LightGray
            TextColor = Color.White, // so i'll just set the text color to white, to get the full tint color
            TintFunc = this.GetNameplateColor, // but tintfunc is applied on every draw call for ui controls, while textcolorfunc is applied only on validation for labels
            MouseThrough = true,
            //TextBackgroundFunc = () => this.HasFocus() ? this.Quality.Color * .5f : Color.Black * .5f
        });

        foreach (var comp in Components.Values)
        {
            comp.OnNameplateCreated(this, plate);
        }
    }
    public Rectangle GetScreenBounds(MapView viewport)
    {
        var g = this.Global;
        var bounds = viewport.GetScreenBounds(g.X, g.Y, g.Z, this.SpriteComp.GetSpriteBounds(), 0, 0, this.Body.Scale);
        return bounds;
    }
    public Rectangle GetScreenBounds(RenderContext ctx)
    {
        var g = this.Global;
        var bounds = ctx.View.GetScreenBounds(g.X, g.Y, g.Z, this.SpriteComp.GetSpriteBounds(), 0, 0, this.Body.Scale);
        return bounds;
    }
    public virtual Color GetNameplateColor()
    {
        return this.GetComponent<QualityComp>()?.Tier.Color ?? Color.White;
    }
    public GameObject Owner
    {
        get => this.Transform.ParentEntity;
        set => this.Transform.ParentEntity = value;
    }
    public Vector3 Global
    {
        get => this.Transform.Global;
        set => this.Transform.Global = value;
    }
    
    public Vector3 Velocity
    {
        get => this.Transform.Velocity;
        set
        {
            if (float.IsNaN(value.X) || float.IsNaN(value.Y))
                throw new Exception();

            this.Transform.Velocity = value;
            if (value != Vector3.Zero)
                this.Physics.Enable();
        }
    }
    public Vector3 Direction
    {
        get => new(this.Transform.Direction, 0);
        set
        {
            if (float.IsNaN(value.X) || float.IsNaN(value.Y))
                throw new Exception();

            var newdir = new Vector2(value.X, value.Y);
            this.Transform.Direction = newdir;
        }
    }
    public int StackMax => this.Def.StackCapacity;
    public bool IsEmpty => this.StackSize == 0;
    public void Consume(int amount)
    {
        if (amount <= 0)
            return;
        this.StackSize -= amount;
        if (this.IsEmpty)
            this.World?.DisposeEntity(this.RefId);
        else
            this.World?.Events.Post(new EntityStackChangedEvent(this as Entity, -amount));
    }
    
    public void Add(int amount)
    {
        if (amount <= 0)
            return;
        this.StackSize += amount;
        this.World?.Events.Post(new EntityStackChangedEvent(this as Entity, amount));
    }
    protected int _stackSize = 1;
    public int StackSize
    {
        get { return this._stackSize; }
        private set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException();
            var oldSize = this.StackSize;
            var newSize = Math.Min(value, this.StackMax);
            if (oldSize == newSize)
            {
                return;
            }

            value = newSize;
            this._stackSize = value;
            if (value == 0)
            {
                
            }
            else if (value < 0)
                throw new Exception();
        }
    }
    public bool IsStackFull => this.StackSize == this.StackMax; 
    public Bone Body => this.SpriteComp.Body;
    internal MaterialDef PrimaryMaterial => this.Body.Material;
    
    public bool IsForbidden;
    //public bool IsSpawned => this._map is not null;
    public bool IsSpawned => this.Transform.IsSpawned;
    public bool IsReserved => this.Map?.Town.ReservationManager.IsReserved(this) ?? false;
    public bool IsPlayerControlled => this.Net.GetPlayers().Any(p => p.ControllingEntity == this); 
    public virtual bool IsHaulable => this.Def.IsHaulable;
    public Entity Hauled => this.Inventory?.HaulSlot.Object as Entity;
    public bool IsHauling => this.Hauled is not null;
    public GameObjectSlot Slot;
    #endregion
    public Def Profile;
    public GameObject Clone(int amount = -1)
    {
        var obj = this.Def.Create(this.Profile);
        foreach (var comp in this.Components.Values)
            obj.GetComponent(comp.GetType()).CopyFrom(comp);
        obj._stackSize = amount < 0 ? this.StackSize : amount;
        //obj.Name = this.Name;
        return obj;
    }
    
    public Entity SetStackSize(int value)
    {
        this.StackSize = value;
        return this as Entity;
    }
    public IEnumerable<GameObject> GetNearbyObjects(Func<float, bool> range, Func<GameObject, bool> filter = null)
    {
        return this.Map.GetNearbyObjectsNew(this.Global, range, filter).Except(new GameObject[] { this });
    }

    public QualityDef? Quality => this.QualityComp?.Tier;
    public Color QualityColor => this.QualityComp?.Tier.Color ?? Color.White;

    DefComponent _info;
    public DefComponent GetInfo()
    {
        return this._info ??= GetComponent<DefComponent>("Info");
    }
    DefComponent _defComponent;
    [InspectorHidden]
    public DefComponent DefComponent => this._defComponent ??= this.GetComponent<DefComponent>();

    TransformComp _transform;
    [InspectorHidden]
    public TransformComp Transform => this._transform ??= this.GetComponent<TransformComp>();

    PhysicsComp _physicsCached;
    [InspectorHidden]
    public PhysicsComp Physics => this._physicsCached ??= this.GetComponent<PhysicsComp>();

    SpriteComp _spriteCompCached;
    [InspectorHidden]
    public SpriteComp SpriteComp => this._spriteCompCached ??= this.GetComponent<SpriteComp>();

    public InventoryComp Inventory => this.GetComponent<InventoryComp>();
    public NeedsComp Needs => this.GetComponent<NeedsComp>();

    ResourcesComp _resourcesCached;
    [InspectorHidden]
    public ResourcesComp Resources => this._resourcesCached ??= this.GetComponent<ResourcesComp>();


    private StatsComp _stats;
    [InspectorHidden]
    internal StatsComp Stats => _stats ??= this.GetComponent<StatsComp>();

    [InspectorHidden]
    public AttributeRuntime this[AttributeDef att] => this.GetAttribute(att);

    public EntityCompCollection Components;

    public T GetComponent<T>(string name) where T : EntityComp
    {
        return this.GetComponent<T>();
    }
    public EntityComp GetComponent(Type type)
    {
        return this.Components.GetComponent(type);
    }
    public T GetComponent<T>() where T : EntityComp
    {
        return this.Components.GetComponent<T>();
    }
    public bool HasComponent<T>() where T : EntityComp
    {
        return this.Components.TryGetComponent<T>(out var _);
    }
    public bool TryGetComponent<T>(string name, out T component) where T : EntityComp
    {
        return this.TryGetComponent(out component);
    }
    public bool TryGetComponent<T>(out T component) where T : EntityComp
    {
        var result = this.Components.TryGetComponent<T>(out var c);
        component = (T)c;
        return result;
    }
    public bool TryGetComponent<T>(Action<T> action) where T : EntityComp
    {
        T component = this.GetComponent<T>();
        if (component is null)
        {
            return false;
        }

        action(component);
        return true;
    }
    public virtual void Tick()
    {
        this.Components.Tick();
    }

    public override string ToString()
    {
        //return $"{this.Net?.ToString()} [{this.RefId}] {this.Def} {this.Profile} {this.Name}";
        return $"{this.Net?.ToString()} [{this.RefId}] {this.Name}";
    }

    #region Children
    byte _ChildrenSequence = 0;
    public byte ChildrenSequence
    {
        get => _ChildrenSequence++;
        private set => _ChildrenSequence = value; 
    }
    byte _ContainerSequence = 0;
    byte ContainerSequence
    {
        get => _ContainerSequence++;
        set => _ContainerSequence = value; 
    }
    public List<GameObjectSlot> GetChildren()
    {
        var list = new List<GameObjectSlot>();
        foreach (var c in this.GetContainers())
            foreach (var s in c.Slots)
                list.Add(s);
        return list;
    }
    public List<Container> GetContainers()
    {
        var list = new List<Container>();
        foreach (var comp in this.Components.Values)
            comp.GetContainers(list);
        return list;
    }
    public Container GetContainer(int id)
    {
        return this.GetContainers().FirstOrDefault(c => c.ID == id);
    }
    public GameObjectSlot GetChild(int containerID, int slotID)
    {
        var c = this.GetContainer(containerID);
        if (c is null)
        {
            return null;
        }

        return c.Slots.FirstOrDefault(s => s.ID == slotID);
    }
    public void RegisterContainer(Container container)
    {
        container.ID = this.ContainerSequence;
        container.Parent = this;
    }
    #endregion

    public GameObject EnumerateChildren()
    {
        this.ChildrenSequence = 0;
        var list = new List<GameObjectSlot>();
        foreach (var comp in this.Components.Values)
        {
            comp.GetChildren(list);
        }

        foreach (var child in list)
        {
            child.ID = this.ChildrenSequence;
        }
        this.ChildrenSequence = 0;
        return this;
    }
    GameObjectSlot[] _cachedSlots;
    public GameObject EnumerateSlots()
    {
        this._cachedSlots = [.. Components.Values
            .SelectMany(c => c.GetSlots())
            .Select((slot, index) => { slot.ID = index; return slot; })];
        return this;
    }
    public GameObjectSlot GetSlot(int slotId)
    {
        return this._cachedSlots[slotId];
    }
    public void GetTooltip(Control tooltip)//Message msg)
    {
        //GetInfo().OnTooltipCreated(this, tooltip);
        // TODO: LOL fix, i need the object name to be on top
        //foreach (var comp in Components.Except(new KeyValuePair<string, EntityComp>[] { new KeyValuePair<string, EntityComp>("Info", GetInfo()) }))
        //tooltip.Controls.Add(new LabelNew(() => this.LabelReadable));

        //tooltip.Controls.Add(new Label(this.Quality.LabelReadable) { Fill = Color.Gold, Location = tooltip.Controls.BottomLeft, TextColorFunc = () => Color.Gold });

        var qualityColor = this.QualityColor;// this.GetNameplateColor();
        tooltip.Color = qualityColor;
        var namelabel = new Label(Vector2.Zero, this.Name, qualityColor, Color.Black, UIManager.FontBold) { TextColorFunc = () => qualityColor, TextFunc = () => this.Name };
        tooltip.AddControlsBottomLeft(namelabel);

        foreach (var comp in Components.Values)
        {
            //if(comp.GetType() != typeof(DefComponent))
            //if(!comp.GetType().IsAssignableFrom(typeof(DefComponent)))
                comp.OnTooltipCreated(tooltip);
        }

        var value = this.GetValueScore();
        if (value > 0)
            tooltip.AddControlsBottomLeft(new Label($"Value: {value * this.StackSize} ({value})"));

        tooltip.AddControlsBottomLeft(new Label($"{nameof(this.RefId)}: {this.RefId}"));
    }
    public void GetInventoryTooltip(Control tooltip)
    {
        GetInfo().OnTooltipCreated(tooltip);
        foreach(var comp in this.Components.Values)
        {
            if (!comp.GetType().IsAssignableFrom(typeof(DefComponent)))
                comp.GetInventoryTooltip(tooltip);
        }

        var value = this.GetValueScore();
        if (value > 0)
        {
            tooltip.AddControlsBottomLeft(new Label(string.Format("Value: {0} ({1})", value * this.StackSize, value)));
        }

        tooltip.AddControlsBottomLeft(new Label(string.Format("InstanceID: {0}", this.RefId)));
    }
    [Obsolete("use ondespawn(mapbase oldmap) instead")]
    public void OnDespawn()
    {
        throw new Exception();
        if (!this.IsSpawned)
            return;
        var oldmap = this.Map;
        this.OnDespawn(oldmap);
    }
    public void OnDespawn(MapBase oldMap)
    {
        
        foreach (var comp in this.Components.Values.ToList())
            comp.OnDespawn(oldMap);

        //oldMap.EventOccured(Message.Types.EntityDespawned, this);
        //oldMap.Events.Post(new EntityDespawnedEvent(this as Entity));
        oldMap.Events.Unsubscribe(this);
        //this.Unreserve(); // UNDONE dont unreserve here because the ai might continue manipulating (placing/carrying) the item during the same behavior
    }
    
    internal virtual void OnSpawn(MapBase newMap)
    {
        this.Net = newMap.Net;
        //this.Container?.Remove(this);
        this.Owner = null;
        this.Map = newMap;
        if (!newMap.TryGetChunk(this.Global, out var chunk))
            throw new Exception("Chunk not loaded");

        foreach (var comp in this.Components.Values)
            comp.OnSpawn(newMap);
    }
  
    
    //public void SyncSpawnNew(MapBase map)
    //{
    //    if (this.RefId != 0)
    //        //this.Spawn(map);
    //        map.Spawn(this as Entity);
    //    if (map.Net is not Server)
    //        return;
    //    SyncInstantiate(map.Net as NetEndpoint);
    //    map.SyncSpawn(this, this.Global, this.Velocity);
    //}

    //public void SyncSpawn(MapBase map, Vector3 global)
    //{
    //    if (map.Net is not Server)
    //        return;

    //    map.SyncSpawn(this, global, Vector3.Zero);
    //}

    //public virtual void Draw(MySpriteBatch sb, RenderContext ctx)
    //{
    //    foreach (var comp in this.Components.Values)
    //        comp.Draw(sb, ctx);
    //}
    public virtual void Draw(MySpriteBatch sb, DrawObjectArgs e)
    {
        foreach (var comp in this.Components.Values)
            comp.Draw(sb, e);
    }

    internal void DrawMouseover(MySpriteBatch sb, RenderContext ctx)
    {
        foreach (var comp in this.Components.Values)
            comp.DrawMouseover(sb, ctx);
    }
    internal void DrawInterface(SpriteBatch sb, MapView viewport)
    {
        foreach (var comp in this.Components.Values)
            comp.DrawUI(sb, viewport);
    }

    public void DrawPreview(MySpriteBatch sb, RenderContext ctx, InteractionTarget target, bool precise)
    {
        if (target.Type != TargetType.Cell)
            return;

        var blockHeight = Block.GetBlockHeight(target.Map, target.Global);
        var global = target.Global + target.Face * new Vector3(1, 1, blockHeight) + (precise ? target.Precise : Vector3.Zero);
        this.DrawPreview(sb, ctx.View, global);
    }

    public void DrawPreview(MySpriteBatch sb, MapView view, Vector3 global)
    {
        var body = this.Body;
        var pos = view.GetScreenPositionFloat(global);
        pos += body.OriginGroundOffset * view.Zoom;
        // TODO: fix difference between tint and material in this drawtree method
        var tint = Color.White * .5f;
        //body.DrawGhost(this, sb, pos, Color.White, Color.White, tint, Color.Transparent, 0, cam.Zoom, 0, SpriteEffects.None, 0.5f, global.GetDrawDepth(Engine.Map, cam));
        //var depth = global.GetDrawDepth(view.Camera);
        var depth = view.GetDrawDepth(global);

        body.DrawGhost(this, sb, pos, Vector4.One, Vector4.One, tint, Color.Transparent, 0, view.Zoom, 0, SpriteEffects.None, 0.5f, depth);
    }

    public virtual void GetTooltipInfo(Control tooltip)
    {
        GetTooltip(tooltip);
    }
    Sprite CachedSprite;
    public ContainerList Container;
    public InventoryList ContainerNew;

    public Sprite GetSprite()
    {
        return this.CachedSprite ??= this.SpriteComp?.Sprite;
    }
    public Sprite GetSpriteOrDefault()
    {
        return this.GetSprite() ?? Sprite.Default;
    }
    public Icon GetIcon()
    {
        return new Icon(GetSpriteOrDefault());
    }

    public byte[] GetSnapshotData()
    {
        var mem = new MemoryStream();
        var w = new DataWriter(mem);
        this.Write(w);
        return mem.ToArray();
    }

   
    public void Write(IDataWriter w)
    {
        w.Write(this.Def);
        w.Write(this.Profile?.Name ?? "");
        if (Core.Def.GetDef(this.Def.Name) is null)
            throw new Exception();
        w.Write(this.RefId);
        w.Write(this.StackSize);
        w.Write(this.OwnerId);
        this.Components.Write(w);
    }
    public static Entity Create(IDataReader r, WorldBase entityResolver)
    {
        string defName = r.ReadString();
        var def = Core.Def.Get<ItemDef>(defName);
        var profile = Core.Def.GetDef(r.ReadString());
        var obj = def.Create(profile);
        obj.World = entityResolver;
        obj.RefId = r.ReadInt32();
        var amount = r.ReadInt32();
        obj._stackSize = amount < 0 ? def.StackCapacity : amount;
        obj.OwnerId = r.ReadEntityRefId();
        obj.Components.Read(r);
        return obj;
    }
    public static Entity Create(IDataReader r)
    {
        string defName = r.ReadString();
        var def = Core.Def.Get<ItemDef>(defName);
        var profile = Core.Def.GetDef(r.ReadString());
        var obj = def.Create(profile);
        obj.RefId = r.ReadInt32();
        var amount = r.ReadInt32();
        obj._stackSize = amount < 0 ? def.StackCapacity : amount;
        obj.Components.Read(r);
        return obj;
    }
    public static GameObject CloneTemplate(int templateID, IDataReader reader)
    {
        GameObject obj = CloneTemplate(templateID);
        _ = reader.ReadString(); // def name not necessary because we copy it from the existing cloned object
        obj.RefId = reader.ReadInt32();
        obj.StackSize = reader.ReadInt32();
        obj.Components.Read(reader);
        obj.ObjectSynced();
        return obj;
    }
   
    public GameObject ObjectSynced()
    {
        foreach (var comp in Components.Values)
        {
            comp.OnObjectSynced(this);
        }

        this.EnumerateChildren();
        return this;
    }
    public void Save(SaveTag tag, string name)
    {
        tag.Add(this.Save(name));
    }
    public SaveTag Save(string name = "")
    {
        return new SaveTag(SaveTag.Types.Compound, name, this.SaveInternal());
    }
    internal List<SaveTag> SaveInternal()
    {
        //var data = new List<SaveTag>
        //{
        //    this.Def.Name.Save("Def"),
        //    // todo : items without profile (coins for now)
        //    //this.Profile?.Save("ProfileID"),
            
        //    ((int)this.RefId).Save("InstanceID"),
        //    this._stackSize.Save("Stack"),
        //    this.Components.Save("Components")
        //};
        var data = new List<SaveTag>();
        data.Add(this.Def.Name.Save("Def"));
        data.Add(this._stackSize.Save("Stack"));
        data.Add(this.Components.Save("Components"));
        if (this.Profile is not null)
            data.Add(this.Profile.Save("ProfileID"));
        data.Add(((int)this.RefId).Save("InstanceID"));
        return data;
    }
    /// <summary>
    /// Creates an object from a savetag node.
    /// </summary>
    /// <param name="tag">A tag with a list of tags as its value.</param>
    /// <returns></returns>
    internal static GameObject Load(SaveTag tag, WorldBase entityResolver)
    {
        tag.TryGetTagValueOrDefault("Def", out string defName);
        var def = Core.Def.Get<ItemDef>(defName);
        Def profile = null;
        if (tag.TryGetTagValueOrDefault("ProfileID", out string profileName)) profile = Core.Def.GetDef(profileName);

        if (def is null)
            return null;
        var obj = def.Create(profile);
        obj.World = entityResolver; // to resolve child entities (like inventory items saved as entityrefids)
        tag.TryGetTagValueOrDefault("InstanceID", out obj.RefId);
        tag.TryGetTagValue<int>("Stack", v => obj._stackSize = v);
        obj.Components.Load(tag["Components"]);
        obj.ResetName();
        return obj;
    }
    /// <summary>
    /// Creates an object from a savetag node.
    /// </summary>
    /// <param name="tag">A tag with a list of tags as its value.</param>
    /// <returns></returns>
    internal static GameObject Load(SaveTag tag)
    {
        tag.TryGetTagValueOrDefault("Def", out string defName);
        var def = Core.Def.Get<ItemDef>(defName);
        Def profile = null;
        if (tag.TryGetTagValueOrDefault("ProfileID", out string profileName)) profile = Core.Def.GetDef(profileName);

        if (def is null)
            return null;
        var obj = def.Create(profile);
        tag.TryGetTagValueOrDefault("InstanceID", out obj.RefId);
        tag.TryGetTagValue<int>("Stack", v=> obj._stackSize = v);
        obj.Components.Load(tag["Components"]);
        obj.ResetName();
        return obj;
    }
    internal ContextAction GetContextRB(GameObject player)
    {
        var list = new List<ContextAction>();
        foreach (var c in this.Components.Values)
        {
            var a = c.GetContextRB(this, player);
            if (a is null)
                list.Add(a);
        }
        return list.FirstOrDefault();
    }
    internal ContextAction GetContextActivate(GameObject player)
    {
        var list = new List<ContextAction>();
        foreach (var c in this.Components.Values)
        {
            var a = c.GetContextActivate(this, player);
            if (a is null)
            {
                list.Add(a);
            }
        }
        return list.FirstOrDefault();
    }
    public void GetContextActions(GameObject playerEntity, ContextArgs a)
    {
        if (playerEntity is null)
        {
            return;
        }

        foreach (var c in this.Components.Values)
        {
            c.GetClientActions(this, a.Actions);
        }
    }

    public bool IsDisposedOld => this.RefId > 0 && this.Net is null;
    public bool IsDisposed { get; private set; }

    [Obsolete("use world.disposeandsync")]
    internal void SyncDispose()
    {
        throw new Exception();
    }
    internal void OnDispose()
    {
        this.IsDisposed = true;
        foreach (var c in this.Components.Values)
            c.OnDispose();
    }
    

    public bool IsInInteractionRange(InteractionTarget target)
    {
        if (target.Type == TargetType.Cell)
        {
            var actorCoords = this.Global;
            var actorBox = new BoundingBox(actorCoords - new Vector3(1, 1, 1), actorCoords + new Vector3(1, 1, this.Physics.Height + 2));
            var targetBox = new BoundingBox(target.Global - new Vector3(.5f, .5f, .5f), target.Global + new Vector3(1.5f, 1.5f, 1.5f));
            var result = actorBox.Intersects(targetBox);
            if (!result)
            {
                throw new Exception();
            }
            return result;
        }
        else if (target.Type == TargetType.Entity)
        {
            var cylinderTarget = new BoundingCylinder(target.Global, .5f, target.Object.Physics.Height);
            var cylinderActor = new BoundingCylinder(this.Global - Vector3.UnitZ, Interaction.DefaultRange, this.Physics.Height + 2);
            var result = cylinderActor.Intersects(cylinderTarget);
            if (!result)
            {
                throw new Exception();
            }
            return result;
        }
        throw new Exception();
    }

    internal void OnMapLoaded(MapBase map)
    {
        //this.Map = map; // DONT set map here because this is called for every networkobject which includes inventory items, and unspawned items must NOT have their map field set
        foreach (var comp in this.Components.Values)
            comp.OnMapLoaded(this);
    }

    

    public bool HasMatchingBody(GameObject otherItem)
    {
        return this.SpriteComp.HasMatchingBody(otherItem);
    }

    public int StackAvailableSpace { get { return this.StackMax - this.StackSize; } }
    public bool CanAbsorb(GameObject otherItem, int amount = -1)
    {
        //ArgumentNullException.ThrowIfNull(otherItem);

        //if (!this.Matches(otherItem))
        //    return false;

        //if (amount == -1)
        //    return true;

        //if (this.IsStackFull)
        //    return false;

        //if (this.StackSize + amount > this.StackMax)
        //    throw new Exception();

        //return true;

        ArgumentNullException.ThrowIfNull(otherItem);

        if (this.Def.IsSingleUnit)
            return false;

        if (this == otherItem)
            return false;

        if (this.IsStackFull)
            return false;

        if (otherItem.Def != null && this.Def != otherItem.Def)
            return false;

        if (otherItem.Profile is not null && this.Profile != otherItem.Profile)
            return false;

        if (!this.HasMatchingBody(otherItem))
            return false;

        if (amount == -1)
            return true;

        if (this.StackSize + amount > this.StackMax)
            throw new Exception();

        return true;
    }
    public bool Matches(GameObject other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (this == other)
            return true;

        if (other.Def != null && this.Def != other.Def)
            return false;

        if (other.Profile is not null && this.Profile != other.Profile)
            return false;

        if (!this.HasMatchingBody(other))
            return false;

        return true;
    }
    
    internal GameObject ClearCarried()
    {
        var carried = this.Inventory.HaulSlot;
        var obj = carried.Object;
        carried.Clear();
        return obj;
    }
  
    internal List<GameObject> GetPossesions()
    {
        return NpcComponent.GetPossessions(this).Select(id => this.World.Get(id) as GameObject).ToList();
    }
    internal NeedRuntime GetNeed(NeedDef def)
    {
        return this.GetComponent<NeedsComp>().NeedsNew[def];//.First(n => n.NeedDef == def);
    }
    internal IEnumerable<NeedRuntime> GetNeeds(NeedCategoryDef cat)
    {
        return this.GetComponent<NeedsComp>().NeedsNew.Values.Where(n => n.NeedDef.CategoryDef == cat);
    }
    internal BoundingBox GetBoundingBox(Vector3 global)
    {
        return this.Physics.GetBoundingBox(global);
    }
    internal BoundingBox GetBoundingBox(Vector3 global, float height)
    {
        return PhysicsComp.GetBoundingBox(global, height);
    }
    internal bool IntersectsCorners(IntVec3 cell)
    {
        return this.GetBoundingBoxCorners().Any(c => c.ToCell() == cell);
    }
    internal IEnumerable<Vector3> GetBoundingBoxCorners()
    {
        return this.Physics.GetBoundingBoxCorners(this.Global);
    }
    internal IEnumerable<Vector3> GetBoundingBoxCorners(Vector3 global)
    {
        return this.Physics.GetBoundingBoxCorners(global);
    }
    /// <summary>
    /// checks if the entity's bounding box intersects the specified cell at its current position
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    internal bool Intersects(IntVec3 cell)
    {
        return this.GetBoundingBox(this.Global).Intersects(new BoundingBox(new Vector3(cell.X - .5f, cell.Y - .5f, cell.Z), new Vector3(cell.X + .5f, cell.Y + .5f, cell.Z + 1)));
    }
    /// <summary>
    /// checks if the entity's bounding box will intersect the specified cell at the specified global position
    /// </summary>
    /// <param name="global"></param>
    /// <param name="cell"></param>
    /// <returns></returns>
    internal bool Intersects(Vector3 global, IntVec3 cell)
    {
        return this.GetBoundingBox(global).Intersects(new BoundingBox(new Vector3(cell.X - .5f, cell.Y - .5f, cell.Z), new Vector3(cell.X + .5f, cell.Y + .5f, cell.Z + 1)));
    }
    internal bool Intersects(BoundingBox boundingBox)
    {
        return this.GetBoundingBox(this.Global).Intersects(boundingBox);
    }
    internal bool Intersects(Vector3 global, BoundingBox boundingBox)
    {
        return this.GetBoundingBox(global).Intersects(boundingBox);
    }
    /// <summary>
    /// Returns the next cell the entity will enter determined by its velocity vector
    /// </summary>
    public IntVec3 NextCell => this.Global + this.Velocity.Normalized();
    /// <summary>
    /// Returns the cell the entity moved from, determined by its velocity
    /// </summary>
    public IntVec3 LastCell => this.Global - this.Velocity.Normalized();

    internal Vector3 GetNextStep()
    {
        return this.Global + PhysicsComp.Decelerate(this.Velocity);
    }
    public bool IsStockpilable()
    {
        return this.Def?.Category != null;
    }
    
    
    public static void DrawIcon(Bone body, int w, int h, float scale = 1)
    {
        // same as Body.RenderNewererest
        GraphicsDevice gd = Game1.Instance.GraphicsDevice;
        var sprite = body.Sprite;
        var loc = new Vector2(0, 0);
        Effect fx = Renderer.Effect;// Game1.Instance.Content.Load<Effect>("blur");
        MySpriteBatch mysb = new MySpriteBatch(gd);
        fx.CurrentTechnique = fx.Techniques["EntitiesFog"];
        fx.Parameters["Viewport"].SetValue(new Vector2(w, h));
        Sprite.Atlas.Begin(fx);
        fx.CurrentTechnique.Passes["Pass1"].Apply();
        loc += sprite.OriginGround;
        body.DrawGhost(mysb, loc * scale, Vector4.One, Vector4.One, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);
        mysb.Flush();
    }

    public bool Exists => this.IsSpawned;
    //public bool ExistsOn(MapBase map) => this._map == map && this.Owner == null;
    public bool ExistsOn(MapBase map) => this.Transform.IsSpawnedIn(map);

    internal void MoveOrder(InteractionTarget target, bool enqueue)
    {
        this.GetComponent<AIComp>().MoveOrder(target, enqueue);
    }
    internal bool IsAt(Vector3 global)
    {
        var cylindermax = new BoundingCylinder(global, .1f, 1);
        return cylindermax.Contains(this.Global);
    }
    internal void DrawAfter(MySpriteBatch sb, RenderContext ctx)
    {
        foreach (var comp in this.Components.Values)
            comp.DrawAfter(sb, ctx);
    }
   
    internal bool IsIndoors()
    {
        var region = this.Map.GetRegionAt(this.Global.Below().ToCell()); // TODO: find first solid block below object
        return region != null && !region.Room.IsOutdoors;
    }

    internal bool IsForbiddable() => !this.HasComponent<NpcComponent>();
    
    internal void DrawHighlight(SpriteBatch sb, MapView viewport)
    {
        SpriteComp.DrawHighlight(this, sb, viewport);
    }

    internal void DrawBorder(SpriteBatch sb, MapView viewport)
    {
        var camera = viewport.Camera;
        this.GetScreenBounds(viewport).DrawHighlightBorder(sb, .5f, camera.Zoom);
    }
   
    static readonly Vector3[] HitboxCorners = [
                new Vector3(.25f, .25f, 0),
                new Vector3(-.25f, .25f, 0),
                new Vector3(.25f, -.25f, 0),
                new Vector3(-.25f, -.25f, 0)
            ];

    internal Vector3 GetCellStandingOn()
    {
        var global = this.Global;
        var below = global.CeilingZ().Below().ToCell();
        var belownode = this.Map.GetNodeAt(below);
        if (belownode is not null)
        {
            return below;
        }
        //else check corners because it's standing on the edge of a block
        foreach (var corner in HitboxCorners)
        {
            var pos = (global + corner).CeilingZ().Below().ToCell();
            belownode = this.Map.GetNodeAt(pos);
            if (belownode is not null)
            {
                return pos;
            }
        }
        throw new Exception(); //thrown when actor was stuck inside a block
    }

    internal bool IsFootprintWithinCell(Vector3 target)
    {
        return target.ContainsEntityFootprint(this);
    }
    internal BoundingBox GetFootprint()
    {
        return this.GetBoundingBox(this.Global, 0);
    }
    
    public MaterialDef Material => this.SpriteComp.GetMaterial(this.Body);

    internal bool HasFocus()
    {
        return Ingame.Instance.ToolManager.ActiveTool?.Target?.Object == this;
    }

    internal void Sync(NetEndpoint net)
    {
        PacketEntitySync.Send(net, this);
    }
    internal void SyncWrite(IDataWriter w)
    {
        foreach (var comp in this.Components.Values)
            comp.SyncWrite(w);
    }

    internal void SyncRead(IDataReader r)
    {
        foreach (var comp in this.Components.Values)
            comp.SyncRead(this, r);
    }

    internal void SetOwner(GameObject actor)
    {
        this.SetOwner(actor != null ? actor.RefId : -1);
    }
    internal void SetOwner(int actorID)
    {
        this.TryGetComponent<OwnershipComponent>(c => c.SetOwner(this, actorID));
    }

    internal bool IsPlant()
    {
        return this.HasComponent<PlantComponent>();
    }

    internal Skill GetSkill(SkillDef skill)
    {
        return this.GetComponent<SkillsComponent>().GetSkill(skill);
    }
    public float Fuel
    {
        get
        {
            return this.Material?.Fuel?.Value ?? 0;
        }
    }
    
    internal float TotalWeight
    {
        get
        {
            return this.Physics.Weight * this.StackSize;
        }
    }

    public Color? TooltipColor => this.QualityComp?.Tier.Color;

    internal List<StatNewModifier> GetStatModifiers(StatDef statNewDef)
    {
        this.TryGetComponent<StatsComp>(out var stats);
        return stats?.GetModifiers(statNewDef);
    }
    internal void AddResourceModifier(ResourceRateModifier resourceModifier)
    {
        this.GetComponent<ResourcesComp>().AddModifier(resourceModifier);
    }
    internal void AddStatModifier(StatNewModifier statNewModifier)
    {
        this.GetComponent<StatsComp>().AddModifier(statNewModifier);
    }
    public int GetValueScore()
    {
        if (this.Def.BaseValue == 0)
            return 0;
        var qualityMultiplier = this.GetComponent<QualityComp>()?.Tier.Multiplier ?? 1;
        var bones = this.Body.GetAllBones();
        var value = 0;
        foreach (var b in bones)
            value += b.Material?.ValueBase ?? 0;
        return (int)(value * this.Def.BaseValue * qualityMultiplier);
    }
    public int GetValueTotal()
    {
        return this.GetValueScore() * this.StackSize;
    }
    public virtual IEnumerable<Control> GetSelectionDetails()
    {
        yield break;
    }
    #region packets
    static readonly int PacketSyncSetStacksize, PacketSyncAbsorb;
    static GameObject()
    {
        PacketSyncSetStacksize = Registry.PacketHandlers.Register(SyncSetStacksize);
        PacketSyncAbsorb = Registry.PacketHandlers.Register(SyncAbsorb);
    }
    protected GameObject()
    {
        this.Components = new(this as Entity);
    }
    protected GameObject(ItemDef def, int amount) : this()
    {
        this.Def = def;
        this._stackSize = amount < 0 ? this.Def.StackCapacity : amount;
    }
    
    public void SyncSetStackSize(int v)
    {
        var net = this.Net;
        if (net is Server)
            this.SetStackSize(v);

        var w = net.BeginPacket(PacketSyncSetStacksize);

        w.Write(this.RefId);
        w.Write(v);
    }
    private static void SyncSetStacksize(NetEndpoint net, Packet packet)
    {
        var r = packet.PacketReader;
        var obj = net.World.Get(r.ReadEntityRefId());
        var value = r.ReadInt32();
        if (net is Client)
            obj.SetStackSize(value);
        else
            obj.SyncSetStackSize(value);
    }
    public void Absorb(GameObject obj)
    {
        if (!this.CanAbsorb(obj))
            return;

        this.StackSize += obj.StackSize;
        this.Map.World.DisposeEntity(obj as Entity);
    }
    public void SyncAbsorb(GameObject obj)
    {
        var net = this.Net;
        if (net is Client)
            throw new Exception();

        // First send the absorb packet 
        var w = net.BeginPacket(PacketSyncAbsorb);
        w.Write(this.RefId);
        w.Write(obj.RefId);

        // Otherwise dispose will sync first and the client won't have a target to absorb
        this.Absorb(obj);
    }
    private static void SyncAbsorb(NetEndpoint net, Packet packet)
    {
        var r = packet.PacketReader;
        if (net is Server)
            throw new Exception();

        var master = net.World.Get(r.ReadEntityRefId());
        var slave = net.World.Get(r.ReadEntityRefId());
        master.Absorb(slave);
    }
    #endregion
   
    public IEnumerable<IntVec3> GetOccupyingCells()
    {
        return this.Def.OccupyingCellsStanding(this.Global.ToCell());
    }

    
    public void ResolveReferences()
    {
        this.Components.ResolveReferences();
    }

    public IEnumerable<Control> GetTooltipControls()
    {
        var box = new GroupBox();
        //box.AddControls(new LabelNew(() => this.LabelReadable));
        this.GetTooltipInfo(box);
        foreach (var comp in this.Components.Values)
            foreach (Control ctrl in comp.GetTooltipControls())
                box.AddControlsBottomLeft(ctrl);
        yield return box;
    }
}

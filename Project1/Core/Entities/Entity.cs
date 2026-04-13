using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Animations;
using Project1.Core.Components;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Gear;
using Project1.Core.Input;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Ownership;
using Project1.Core.Systems.Tools;
using Project1.Framework;
using Project1.Framework.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Entities;

public class Entity : GameObject
{
  

    bool _initialized;
   
    public MobileComponent Mobile => field ??= this.GetComponent<MobileComponent>();

    public SpriteComp Sprite => field ??= this.GetComponent<SpriteComp>();

    [InspectorHidden]
    public float this[StatDef stat] => stat.CalculateFor(this);
    /// <summary>
    /// here or in tool class?
    /// </summary>
    public ToolComp ToolComponent => field ??= this.GetComponent<ToolComp>();

    public GearComponent Gear => field ??= this.GetComponent<GearComponent>();

    public OwnershipComponent Ownership => field ??= this.GetComponent<OwnershipComponent>();
    public Actor Author
    {
        get => this.DefComponent.Author;
        set => this.DefComponent.Author = value;
    }
    public Entity()
    {
        this.Components.Add(EntityCompDefOf.Transform);
        this.Components.Add(EntityCompDefOf.DefComp);
        this.Components.Add(EntityCompDefOf.Physics);
        this.Components.Add(EntityCompDefOf.Sprite);
    }
    public Entity(ItemDef def, int amount) : base(def, amount)
    {
        this.Components.Add(EntityCompDefOf.Transform);
        this.Components.Add(EntityCompDefOf.DefComp);
        this.Components.Add(EntityCompDefOf.Physics);
        this.Components.Add(EntityCompDefOf.Sprite);
    }

    internal void TickOffMap()
    {
        this.Components.TickOffMap();
    }
    public Entity Initialize()
    {
        if (this._initialized)
            throw new InvalidOperationException($"{this} initialized twice");
        this._initialized = true;
        this.Components.Initialize();
        return this;
    }
    internal void InitComps(ItemDef def)
    {
        this.Components.CreateAndResolve(def);
        this.EnumerateSlots();
    }
    internal bool ProvidesSkill(ToolUseDef skill)
    {
        return this.ToolComponent?.ToolUse == skill;
    }

    internal MaterialDef GetMaterial(BoneDef def)
    {
        return this.Sprite.GetMaterial(def);
    }
    internal virtual GameObject SetName(string v)
    {
        this.Name = v;
        return this;
    }

    internal Texture2D RenderIcon(int scale = 1)
    {
        return this.Body.RenderIcon(this, scale);
    }

    internal Entity SetMaterial(MaterialDef mat)
    {
        foreach (var c in this.Components.Values)
            c.SetMaterial(mat);
        this.Name = $"{mat.Prefix}";
        if (!this.Def.ReplaceName)
            this.Name += $" {this.Def.LabelReadable}";
        mat.Apply(this);
        return this;
    }
    internal Entity SetMaterials(Dictionary<string, MaterialDef> materials)
    {
        foreach (var c in this.Components.Values)
            c.ApplyMaterials(this, materials);
        return this;
    }
    internal Entity SetQuality(QualityDef quality)
    {
        if (this.Def.QualityLevels)
            foreach (var c in this.Components.Values)
                c.ApplyQuality(this, quality);
        return this;
    }

    

    public GameObject Randomize(RandomThreaded random)
    {
        if (this.Def.CraftingProperties is not null) // HACK
            this.SetQuality(QualityDef.GetRandom());
        foreach (var comp in this.Components.Values)
            comp.Randomize(this, random);
        return this;
    }
    void RandomizeMaterials()
    {
        var random = new Random();
        var materials = Core.Def.Get<MaterialDef>().ToArray();
        foreach(var bone in this.Body.GetAllBones())
            bone.Material = materials.SelectRandom(random);
    }
    internal void Select()
    {
        //SelectionManager.Select(this);
        Ingame.Instance.Events.Post(new PlayerSelectionSingleEvent(Single: new InteractionTarget(this)));
    }
    /// <summary>
    /// reset name in case of errors or def changes
    /// </summary>
    internal void ResetName()
    {
        this.DefComponent.ParentName = this.Def.NameGetter?.Invoke(this) ?? this.DefComponent.ParentName; // reset name
        this.Name = this.Profile?.LabelReadable ?? this.Def.LabelReadable;
    }
    internal void Resolve()
    {
        this.Components.Resolve();
    }
    public Entity Take(int? amount)
    {
        if (!amount.HasValue)
            return this;
        if (amount.Value == this.StackSize)
            return this;
        ArgumentOutOfRangeException.ThrowIfGreaterThan(amount.Value, this.StackSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount.Value);
        return this.Split(amount.Value);
    }

    internal void ApplySpecs(List<EntityComp.Spec> overrides)
    {
        this.Components.ApplySpecs(overrides);
    }
    public bool IsDead { get; private set; }
    internal void Kill()
    {
        this.IsDead = true;
        foreach (var comp in this.Components.Values)
            comp.OnKill();
        //this.Map.Events.Post(new EntityKilledEvent(this));
    }

    public BoundingBox GetBoundingBoxNext()
    {
        var x = this.Global.X + this.Velocity.X;
        var y = this.Global.Y + this.Velocity.Y;
        var z = this.Global.Z + this.Velocity.Z;
        return new(
            new(x - .25f, y - .25f, z),
            new(x + .25f, y + .25f, z + this.Def.Height));
    }

    internal void HitTest(Camera camera)
    {
        this.SpriteComp.HitTest(this, camera);
    }
    internal int GetUnreservedAmount()
    {
        return this.Map.Town.ReservationManager.GetUnreservedAmount(this);
    }
    /// <summary>
    /// Changes the objects global position, removing the object from the previous chunk's object list and adding it to the new one's accordingly.
    /// </summary>
    /// <param name="nextGlobal"></param>
    /// <returns></returns>
    public GameObject SetPosition(Vector3 nextGlobal) // TODO: merge this with SetGlobal
    {
        // entity despawned and immediately respawned on the serve and sent a new snapshot while on client the entity's map was null
        if (this.Map is null) // entity has despawned on client before snapshot received?
            return this;
        //throw new Exception("set the object's map before setting its position");

        if (this.Map.IsSolid(nextGlobal))// + Vector3.UnitZ * 0.01f))// TODO: FIX THIS
            return this; // TODO: FIX: problem when desynced from server, block might be empty on server but solid on client

        this.Map.TryGetChunk(nextGlobal.RoundXY(), out var nextChunk);

        if (nextChunk is null)
            return this;

        this.Map.TryGetChunk(this.Global.ToRounded(), out var lastChunk);
        var lastCell = this.Global.ToCell();
        var nextCell = nextGlobal.ToCell();
        this.Global = nextGlobal;
        if(LastCell != nextCell)
        {
            this.Map.EntityChangedCell(this, LastCell, nextCell);
        }
        if (nextChunk != lastChunk)
        {
            bool removed = lastChunk.Remove(this);
            if (!removed)
                throw new Exception("Source chunk is't loaded"); //Could not remove object from previous chunk");

            nextChunk.Add(this);
        }
        this.Physics.Enable();
        return this;
    }
    public IntVec3 Cell
    {
        get => this.Global.ToCell();
        set => this.SetPosition(value);
    }
    public Vector3 GridCellOffset
    {
        get => this.Global - (Vector3)this.Cell;
        set => this.SetPosition((Vector3)this.Cell + value);
    }
    public IntVec3? CellIfSpawned => this.IsSpawned ? this.Cell : null;
    internal void Detach()
    {
        this.ContainerNew?.Remove(this);
        this.ContainerNew = null;
        this.Container?.Remove(this);
        this.Container = null;
        this.Slot?.Assign(null, out var _);
        this.Slot = null;
        //this.Map?.Despawn(this);
        //this.Map = null;
        this.Transform.Detach();
        this.Owner = null;
        //this.Transform.Anchor = null;
    }
    public IEnumerable<Entity> GetSelfAndChildren()
    {
        // moving this to the end because this is usually called when registering entities to the entity registry
        // so inventory items must be registered first so clients can resolve their entityrefids
        //yield return this;
        foreach (var c in this.Components.Values)
            foreach (var ch in c.GetChildren())
                foreach (var chch in ch.GetSelfAndChildren())
                    yield return chch;
        yield return this;
    }
    public Entity Split(int amount)
    {
        if (amount <= 0 || amount >= _stackSize)
            throw new ArgumentOutOfRangeException(nameof(amount));
        var newObject = this.Clone() as Entity;
        newObject._stackSize = amount;
        this.World.Register(newObject);
        this.Consume(amount);
        newObject.SetOwnerNew(this.OwnerId);
        return newObject;
    }

}

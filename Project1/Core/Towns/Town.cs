using Microsoft.Xna.Framework;
using Project1.Core.AI;
using Project1.Core.AI.MetaRoles;
using Project1.Core.AI.Reservations;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Graphics;
using Project1.Core.Input;
using Project1.Core.Networking;
using Project1.Core.Rooms;
using Project1.Core.Simulation;
using Project1.Core.Systems.Conversations;
using Project1.Core.Systems.Plants;
using Project1.Core.Systems.Quests;
using Project1.Core.Systems.Trading;
using Project1.Core.Towns.Constructions;
using Project1.Core.Towns.Designations;
using Project1.Core.Towns.Digging;
using Project1.Core.Towns.Duties;
using Project1.Core.Towns.Reputation;
using Project1.Core.Towns.Services;
using Project1.Core.Towns.Services.Spells;
using Project1.Core.Towns.Services.Inns;
using Project1.Core.Towns.Services.Repairing;
using Project1.Core.Towns.Services.Shops;
using Project1.Core.Towns.Storage;
using Project1.Core.Towns.UI;
using Project1.Core.Towns.Zones;
using Project1.Core.UI;
using Project1.Core.UI.Hud;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Input;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Project1.Core.Systems.Crafting;
using Project1.Core.Screens;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Rendering;

namespace Project1.Core.Towns;

public sealed class Town : Inspectable, IDutyProvider
{
    UIQuickMenu QuickMenu;
    public static HotkeyCategory HotkeyContext = new("Town");

    internal void OnTooltipCreated(Control tooltip, InteractionTarget targetArgs)
    {
        foreach (var c in this.TownComponents)
            c.OnTooltipCreated(tooltip, targetArgs);
    }

    public IReadOnlyCollection<DutyDef> AvailableDuties => field ??=
    [
        DutyDefOf.Repairsmith,
        DutyDefOf.Healer,
        DutyDefOf.Cashier,
        DutyDefOf.Innkeeper,
        DutyDefOf.Digger,
        DutyDefOf.Miner,
        DutyDefOf.Lumberjack,
        DutyDefOf.Forester,
        DutyDefOf.Craftsman,
        DutyDefOf.Smelter,
        DutyDefOf.Farmer,
        DutyDefOf.Harvester,
        DutyDefOf.Forager,
        DutyDefOf.Builder,
        DutyDefOf.Carpenter,
        DutyDefOf.Cook,
        DutyDefOf.Scribe,
        DutyDefOf.Guide,
        DutyDefOf.Hauler,
        DutyDefOf.MiscDuties,
    ];

    internal void Init()
    {
        this.RoomManager.Init();

        this.Map.World.Events.ListenTo<EntityDisposedEvent>(OnEntityDisposed);
    }
    
    private void OnEntityDisposed(EntityDisposedEvent e)
    {
        if(e.Entity is Actor actor && this.Members.Contains(actor)) 
            this.RemoveMember(actor);
    }

    public ObservableHashSet<Actor> Members = [];
    public IReadOnlySet<Actor> GetMembers()
    {
        return this.Members;//.Select(id => this.Map.World.GetEntity(id) as Actor);
    }
    public bool IsMember(Actor actor) => this.Members.Contains(actor);
    
    public GameObject GetNpc(Guid guid)
    {
        throw new NotImplementedException();
    }

    [InspectorHidden]
    public ZoneManager ZoneManager;
    public GrowingManager GrowingManager;
    [InspectorHidden]
    public ConstructionManager ConstructionsManager;
    [InspectorHidden]
    public DiggingManager DiggingManager;
    [InspectorHidden]
    public DesignationManager DesignationManager;
    [InspectorHidden]
    public RoomManager RoomManager;
    [InspectorHidden]
    public CraftingManager Crafting;
    [InspectorHidden]
    public DutyRoster DutiesManager;
    [InspectorHidden]
    public ReservationManager ReservationManager;
    public TownComp_Shops Shops;
    public TownComp_Repairs Repairs;
    public TownComp_Inns Inns;
    //[InspectorHidden]
    //public QuestsManager QuestManager;
    public TownComp_Quests QuestManagerNew;
    public TownComp_Spells Spells;
    [InspectorHidden]
    public StorageManager Storage;
    public FurnitureTracker Furniture;
    public OwnershipManager Ownership;
    public TownReputationComp Reputation;
    public ConversationSystem Conversations;
    public TownComp_Trade Trades;
    public TownComp_ServiceRequests ServiceRequests;

    public List<TownComp> TownComponents = [];

    public MapBase Map { get; private set; }
    public NetEndpoint Net => this.Map.Net;

    public IntVec3? Waypoint;
    public IEntityProvider Entities => this.Map.World;

    public Dictionary<EntityRefId, ServiceRequest> OpenTransactions = [];

    public Town(MapBase map)
    {
        this.Map = map;
        this.ZoneManager = new(this);
        this.GrowingManager = new(this);
        this.ConstructionsManager = new(this);
        this.DiggingManager = new(this);
        this.DesignationManager = new(this);
        this.RoomManager = new(this);
        this.Crafting = new(this);
        this.DutiesManager = new(this);
        this.ReservationManager = new(this);
        this.Shops = new(this);
        this.Inns = new(this);
        this.Storage = new(this);
        this.Furniture = new(this);
        this.Ownership = new(this);
        this.Reputation = new(this);
        this.QuestManagerNew = new(this);
        this.Spells = new(this);
        this.Conversations = new(this);
        this.Trades = new(this);
        this.ServiceRequests = new(this);
        this.Repairs = new(this);

        this.TownComponents.AddRange(
            this.ZoneManager,
            this.GrowingManager,
            this.ConstructionsManager,
            this.DiggingManager,
            this.DesignationManager,
            this.RoomManager,
            this.Crafting,
            this.ReservationManager,
            this.Shops,
            this.Inns,
            this.QuestManagerNew,
            this.Spells,
            this.Storage,
            this.Furniture,
            this.Ownership,
            this.Reputation,
            this.Conversations,
            this.Trades,
            this.ServiceRequests,
            this.Repairs
        );

        this.Map.Events.ListenTo<BlocksChangedEvent>(HandleBlocksChanged);
    }

    private void HandleBlocksChanged(BlocksChangedEvent e)
    {
        foreach(var ee in e.Changes)
        {
            if (ee.Block.BlockDef == BlockDefOf.Waypoint)
                this.Waypoint = ee.Global.Above;
            else if (this.Waypoint.HasValue && this.Waypoint.Value == ee.Global.Above)
                this.Waypoint = null;
        }
    }

    public void Update()
    {
        foreach (var comp in this.TownComponents)
            comp.Update();
    }
   
    internal void AddMember(Actor actor)
    {
        if (!actor.HasComponent<AIComp>())
            throw new Exception();
        this.Members.Add(actor);
        this.Map.Events.Post(new MemberAddedEvent(actor));
        RoleMetaDefOf.TownMember.AssignTo(actor);
        actor.Town = this;
        //actor.Net.Report($"{actor.Name} has joined the town!");
        this.DutiesManager.Add(actor);
        actor.AI.State.Log.Write("I joined the town!");
    }

    internal void RemoveMember(Actor actor)
    {
        if (actor.HasComponent<AIComp>())
        {
            this.DutiesManager.Remove(actor);
            this.Members.Remove(actor);
            this.Map.Events.Post(new MemberRemovedEvent(actor));
            actor.Town = null;
            //this.Net.ConsoleBox.Write($"{actor.Name} was dismissed from the town!");
        }
    }
   
    public void ToggleMembers(IEnumerable<Actor> actors)
    {
        foreach (var actor in actors) 
            this.ToggleMember(actor);
    }
    public void ToggleMember(Actor entity)
    {
        if (!this.Members.Contains(entity))
            this.AddMember(entity);
        else
            this.RemoveMember(entity);
    }
    
    internal void OnCameraRotated(Renderer camera)
    {
        foreach (var c in this.TownComponents)
            c.OnCameraRotated(camera);
    }

    internal void Tick()
    {
        foreach (var c in this.TownComponents)
            c.Tick();
    }

    public void DrawBeforeWorld(MySpriteBatch sb, RenderContext ctx)
    {
        foreach(var comp in this.TownComponents)
            comp.DrawBeforeWorld(sb, ctx);
    }

    internal void ResolveReferences()
    {
        foreach (var member in this.Members)
            member.Town = this;
            //this.Map.World.GetEntity(memberId).Town = this;

        foreach (var comp in this.TownComponents)
            comp.ResolveReferences();
    }

    public SaveTag Save(string name)
    {
        var tag = new SaveTag(SaveTag.Types.Compound, name);

        var compsTag = new SaveTag(SaveTag.Types.Compound, "Components");
        foreach (var comp in this.TownComponents)
            compsTag.Add(comp.Save());

        tag.Add(compsTag);

        SaveAgents(tag);

        return tag;
    }

    private void SaveAgents(SaveTag tag)
    {
        var agentsTag = new SaveTag(SaveTag.Types.List, "Agents", SaveTag.Types.Int);
        foreach (var a in this.Members)
            agentsTag.Add(new SaveTag(SaveTag.Types.Int, "", a.RefId));
        tag.Add(agentsTag);
    }

    public void Load(SaveTag save)
    {
        Dictionary<string, SaveTag> compsTag = new Dictionary<string, SaveTag>();
        if (save.TryGetTagValueOrDefault("Components", out compsTag))
            foreach (var tag in compsTag)
            {
                var comp = this.TownComponents.FirstOrDefault(c => c.Name == tag.Key);
                if (comp != null)
                    comp.Load(tag.Value);
            }
       
        LoadAgents(save);
    }

    private void LoadAgents(SaveTag save)
    {
        if (save.TryGetTagValueOrDefault("Agents", out List<SaveTag> agentsTag))
            foreach (var bytes in agentsTag)
            {
                var id = (int)bytes.Value;
                //this.AddMember(id);
                this.Members.Add(this.Map.World.Get<Actor>(id));
            }

        foreach (var member in this.Members)//.Select(this.Map.World.GetEntity<Actor>))
            this.DutiesManager.Add(member);
    }

    public void Write(IDataWriter w)
    {
        foreach (var comp in this.TownComponents)
            comp.Write(w);

        w.Write(this.Members.Count);
        foreach (var a in this.Members)
            w.Write(a.RefId);

        this.DutiesManager.Write(w);
    }
    public void Read(IDataReader r)
    {
        foreach (var comp in this.TownComponents)
            comp.Read(r);

        var acount = r.ReadInt32();
        for (int i = 0; i < acount; i++)
            this.Members.Add(this.Map.World.Get<Actor>(r.ReadInt32()));

        this.DutiesManager.Read(r);
    }

    public void GetContextActions(GameObject playerEntity, Vector3 pos, ContextArgs a)
    {
        var zone = this.QueryPosition(pos);
        if (zone.Count == 0)
            return;
        zone.First().GetContextActions(playerEntity, a);
    }

    public List<IContextable> QueryPosition(Vector3 pos)
    {
        var list = new List<IContextable>();
        foreach (var comp in this.TownComponents)
            list.Add(comp.QueryPosition(pos));
        return list.Where(t => t != null).ToList();
    }
    public IEnumerable<ISelectable> QuerySelectables(CellSelection target)
    {
        while (true)
        {
            foreach (var comp in this.TownComponents)
            {
                var item = comp.QuerySelectable(target);
                if (item != null)
                    yield return item;
            }
            yield return target;
        }
    }
    public IReadOnlyList<ISelectable> QuerySelectablesNew(CellSelection target)
    {
        List<ISelectable> list = [];
        foreach (var comp in this.TownComponents)
        {
            var item = comp.QuerySelectable(target);
            if (item is not null)
                list.Add(item);
        }
        list.Add(target);
        return list;
    }
    internal Zone GetZoneAt(Vector3 pos)
    {
        return this.ZoneManager.GetZoneAt(pos);
    }

    internal void DrawUI(SpriteBatch sb, MapView viewport)
    {
        foreach (var comp in this.TownComponents)
            comp.DrawUI(sb, viewport);
    }

    internal UIQuickMenu ToggleQuickMenu()
    {
        if(this.QuickMenu == null)
        {
            InitQuickMenu();
        }
        this.QuickMenu.Toggle();
        return this.QuickMenu;
    }

    private void InitQuickMenu()
    {
        var actions = new List<(Func<string>, Action)>();
        foreach (var comp in this.TownComponents)
            actions.AddRange(comp.OnQuickMenuCreated());
        actions.Add((() => "Debug commands", UIDebugCommands.RefreshNew));
        actions.Add((() => "Spawn objects", () => ObjectTemplatesWindow.Instance.Show()));
        actions.Add((() => "Edit blocks", () => TerrainWindow.Instance.Show()));

        actions.Add((() => "LaborsNew", this.DutiesManager.ToggleLaborsWindow));


        this.QuickMenu = new UIQuickMenu();
        this.QuickMenu.AddItems(actions);
        this.QuickMenu.SnapToMouse();
    }

    internal void OnHudCreated(Hud hud)
    {
        foreach (var c in this.TownComponents)
            c.OnHudCreated(hud);
    }

    internal IEnumerable<T> GetBusinesses<T>() where T : Workplace
    {
        return this.Shops.GetShops().OfType<T>();
        throw new NotImplementedException();
    }

    internal Workplace GetShop(int shopID)
    {
        return this.Shops.GetShop(shopID);
    }

    internal T GetShop<T>(int shopID) where T  : Workplace
    {
        return this.Shops.GetShop(shopID) as T;
    }

    internal void OnBlocksChanged(IEnumerable<IntVec3> positions)
    {
        foreach (var c in this.TownComponents)
            c.OnBlocksChanged(positions);
    }

    public bool IsClaimedBySystem(Entity item) => this.TownComponents.Any(c => c.IsClaimedBySystem(item));

    internal void Scan(BlockEntity entity)
    {
        foreach (var comp in this.TownComponents)
            comp.Scan(entity);
    }
    internal void Scan(Entity entity)
    {
        foreach (var comp in this.TownComponents)
            comp.Scan(entity);
    }
    public void Scan((Chunk chunk, Cell cell, CellId id) index)
    {
        if (index.cell.Block.BlockDef == BlockDefOf.Waypoint)
            this.Waypoint = index.id.GetGlobal(index.chunk);
        foreach (var comp in this.TownComponents)
            comp.Scan(index);
    }
}

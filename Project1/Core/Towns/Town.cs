using Microsoft.Xna.Framework;
using Project1.Core.AI;
using Project1.Core.AI.MetaRoles;
using Project1.Core.AI.Reservations;
using Project1.Core.Crafting;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Graphics;
using Project1.Core.Input;
using Project1.Core.Networking;
using Project1.Core.Plants;
using Project1.Core.Rooms;
using Project1.Core.Simulation;
using Project1.Core.Towns.Constructions;
using Project1.Core.Towns.Designations;
using Project1.Core.Towns.Digging;
using Project1.Core.Towns.Duties;
using Project1.Core.Towns.Storage;
using Project1.Core.Towns.Terrain;
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

namespace Project1.Core.Towns
{
    public class Town : Inspectable, IDutyProvider
    {
        UIQuickMenu QuickMenu;
        public static HotkeyCategory HotkeyContext = new("Town");

        internal void OnTooltipCreated(Control tooltip, TargetArgs targetArgs)
        {
            foreach (var c in this.TownComponents)
                c.OnTooltipCreated(tooltip, targetArgs);
        }

        public IReadOnlyCollection<DutyDef> AvailableDuties => field ??= 
                [
                    DutyDefOf.Workplace,
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
                    DutyDefOf.Guide,
                    DutyDefOf.QuestGiver,
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
        public CraftingManager CraftingManager;
        [InspectorHidden]
        public DutyRoster DutiesManager;
        [InspectorHidden]
        public ReservationManager ReservationManager;
        [InspectorHidden]
        public TerrainManager TerrainManager;
        [InspectorHidden]
        public WorkplaceManager ShopManager;
        [InspectorHidden]
        public QuestsManager QuestManager;
        [InspectorHidden]
        public StorageManager Storage;

        public List<TownComponent> TownComponents = [];

        public MapBase Map { get; private set; }
        public NetEndpoint Net => this.Map.Net;

        public IEntityProvider Entities => this.Map.World;

        public Dictionary<Utility.Types, HashSet<IntVec3>> TownUtilitiesNew = new();

        public Town(MapBase map)
        {
            this.Map = map;
            this.ZoneManager = new(this);
            this.GrowingManager = new(this);
            this.ConstructionsManager = new(this);
            this.DiggingManager = new(this);
            this.DesignationManager = new(this);
            this.RoomManager = new(this);
            this.CraftingManager = new(this);
            this.DutiesManager = new(this);
            this.ReservationManager = new(this);
            this.TerrainManager = new(this);
            this.ShopManager = new(this);
            this.QuestManager = new(this);
            this.Storage = new(this);

            this.TownComponents.AddRange([
                this.ZoneManager,
                this.GrowingManager,
                this.ConstructionsManager,
                this.DiggingManager,
                this.DesignationManager,
                this.RoomManager,
                this.CraftingManager,
                this.ReservationManager,
                this.TerrainManager,
                this.ShopManager,
                this.QuestManager,
                this.Storage
            ]);
            
            var utilities = (Utility.Types[])Enum.GetValues(typeof(Utility.Types));
            foreach(var u in utilities)
                this.TownUtilitiesNew[u] = new HashSet<IntVec3>();
        }

        public void Update()
        {
            foreach (var comp in this.TownComponents)
                comp.Update();
        }
        public void AddUtility(Utility.Types type, Vector3 global)
        {
            this.TownUtilitiesNew[type].Add(global);
        }
        public void RemoveUtility(Utility.Types type, Vector3 global)
        {
            if (!this.TownUtilitiesNew[type].Remove(global))
            {
            }
            if (this.TownUtilitiesNew.Any(ut => ut.Value.Contains(global)))
                throw new Exception();
        }
        public IEnumerable<IntVec3> GetUtilities(Utility.Types type)
        {
            return this.TownUtilitiesNew[type];
        }
        public bool HasUtility(Vector3 global, Utility.Types utility)
        {
            if (this.TownUtilitiesNew.TryGetValue(utility, out var list))
                return list.Contains(global);
            return false;
        }

        internal void AddMember(Actor actor)
        {
            if (!actor.HasComponent<AIComponent>())
                throw new Exception();
            this.Members.Add(actor);
            this.Map.Events.Post(new MemberAddedEvent(actor));
            RoleMetaDefOf.TownMember.AssignTo(actor);
            actor.Town = this;
            //actor.Net.Report($"{actor.Name} has joined the town!");
            actor.AI.State.Log.Write("I joined the town!");
        }

        internal void RemoveMember(Actor actor)
        {
            if (actor.HasComponent<AIComponent>())
            {
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
        
        internal void OnCameraRotated(Camera camera)
        {
            foreach (var c in this.TownComponents)
                c.OnCameraRotated(camera);
        }

        internal void Tick()
        {
            foreach (var c in this.TownComponents)
                c.Tick();
        }

        public void DrawBeforeWorld(MySpriteBatch sb, MapBase map, Camera cam)
        {
            foreach(var comp in this.TownComponents)
                comp.DrawBeforeWorld(sb, map, cam);
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

            var utilitiesTag = new SaveTag(SaveTag.Types.List, "Utilities", SaveTag.Types.Compound);
            
            foreach (var t in this.TownUtilitiesNew)
            {
                var typeTag = new SaveTag(SaveTag.Types.Compound);
                typeTag.Add(new SaveTag(SaveTag.Types.Int, "Type", (int)t.Key));
                var positionsTag = t.Value.ToList().Save("Positions");
                typeTag.Add(positionsTag);
                utilitiesTag.Add(typeTag);
            }
            tag.Add(utilitiesTag);

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

            if (save.TryGetTagValueOrDefault("Utilities", out List<SaveTag> utilitiesTag))
            {
                foreach (var tag in utilitiesTag)
                {
                    var utilityType = (Utility.Types)(int)tag["Type"].Value;
                    var positionList = new List<IntVec3>().Load(tag["Positions"].Value as List<SaveTag>);
                    var hash = new HashSet<IntVec3>(positionList);
                    this.TownUtilitiesNew[utilityType] = hash;
                }
            }

            //foreach (var c in this.TownComponents)
            //    c.ResolveReferences();
        }

        private void LoadAgents(SaveTag save)
        {
            if (save.TryGetTagValueOrDefault("Agents", out List<SaveTag> agentsTag))
                foreach (var bytes in agentsTag)
                {
                    var id = (int)bytes.Value;
                    //this.AddMember(id);
                    this.Members.Add(this.Map.World.GetEntity<Actor>(id));
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

            foreach (var ut in Utility.All())
                w.Write(this.TownUtilitiesNew[ut].ToList());

            this.DutiesManager.Write(w);
        }
        public void Read(IDataReader r)
        {
            foreach (var comp in this.TownComponents)
                comp.Read(r);

            var acount = r.ReadInt32();
            for (int i = 0; i < acount; i++)
            {
                this.Members.Add(this.Map.World.GetEntity<Actor>(r.ReadInt32()));
            }

            foreach (var ut in Utility.All())
                this.TownUtilitiesNew[ut] = [.. r.ReadListVector3()];

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

        internal void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera)
        {
            foreach (var comp in this.TownComponents)
                comp.DrawUI(sb, this.Map, camera);
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
            var actions = new List<Tuple<Func<string>, Action>>();
            foreach (var comp in this.TownComponents)
                actions.AddRange(comp.OnQuickMenuCreated());
            actions.Add(new Tuple<Func<string>, Action>(() => "Debug commands", UIDebugCommands.RefreshNew));
            actions.Add(new Tuple<Func<string>, Action>(() => "Spawn objects", () => ObjectTemplatesWindow.Instance.Show()));
            actions.Add(new Tuple<Func<string>, Action>(() => "Edit blocks", () => TerrainWindow.Instance.Show()));

            actions.Add(new Tuple<Func<string>, Action>(() => "LaborsNew", this.DutiesManager.ToggleLaborsWindow));


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
            return this.ShopManager.GetShops().OfType<T>();
            throw new NotImplementedException();
        }

        internal Workplace GetShop(int shopID)
        {
            return this.ShopManager.GetShop(shopID);
        }

        internal T GetShop<T>(int shopID) where T  : Workplace
        {
            return this.ShopManager.GetShop(shopID) as T;
        }

        internal void OnBlocksChanged(IEnumerable<IntVec3> positions)
        {
            foreach (var c in this.TownComponents)
                c.OnBlocksChanged(positions);
        }

        internal void GetQuickButtons(Action<string, Type> register, IntVec3 global)
        {
            foreach (var comp in this.TownComponents)
                comp.GetQuickButtons(register, global);
        }
    }
}

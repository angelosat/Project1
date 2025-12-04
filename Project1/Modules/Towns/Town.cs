using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public partial class Town : Inspectable
    {
        UIQuickMenu QuickMenu;
        public static HotkeyContext HotkeyContext = new("Town");
        internal void OnTooltipCreated(Control tooltip, TargetArgs targetArgs)
        {
            foreach (var c in this.TownComponents)
                c.OnTooltipCreated(tooltip, targetArgs);
        }

        internal void Init()
        {
            this.RoomManager.Init();
        }

        public ObservableHashSet<int> Members = new();
        public IEnumerable<Actor> GetAgents()
        {
            return this.Members.Select(id => this.Map.Net.GetNetworkEntity(id) as Actor);
        }
        public GameObject GetNpc(Guid guid)
        {
            throw new NotImplementedException();
        }

        [InspectorHidden]
        public ZoneManager ZoneManager;
        [InspectorHidden]
        public ConstructionsManager ConstructionsManager;
        [InspectorHidden]
        public DiggingManager DiggingManager;
        [InspectorHidden]
        public DesignationManager DesignationManager;
        [InspectorHidden]
        public RoomManager RoomManager;
        [InspectorHidden]
        public CraftingManager CraftingManager;
        [InspectorHidden]
        public JobsManager JobsManager;
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

        public List<TownComponent> TownComponents = new();

        public MapBase Map;
        public INetEndpoint Net => this.Map.Net;

        public Dictionary<Utility.Types, HashSet<Vector3>> TownUtilitiesNew = new();

        public Town(MapBase map)
        {
            this.Map = map;
            this.ZoneManager = new(this);
            this.ConstructionsManager = new(this);
            this.DiggingManager = new(this);
            this.DesignationManager = new(this);
            this.RoomManager = new(this);
            this.CraftingManager = new(this);
            this.JobsManager = new(this);
            this.ReservationManager = new(this);
            this.TerrainManager = new(this);
            this.ShopManager = new(this);
            this.QuestManager = new(this);
            this.Storage = new(this);

            this.TownComponents.AddRange([
                this.ZoneManager,
                this.ConstructionsManager,
                this.DiggingManager,
                this.DesignationManager,
                this.RoomManager,
                this.CraftingManager,
                this.JobsManager,
                this.ReservationManager,
                this.TerrainManager,
                this.ShopManager,
                this.QuestManager,
                this.Storage
            ]);
            
            var utilities = (Utility.Types[])Enum.GetValues(typeof(Utility.Types));
            foreach(var u in utilities)
                this.TownUtilitiesNew[u] = new HashSet<Vector3>();
        }

        public void Update()
        {
            foreach (var agent in this.Members.ToArray())
                if (this.Net.GetNetworkEntity(agent) == null)
                {
                    this.Members.Remove(agent);
                    $"Removed disposed townie entity with id: {agent}".ToConsole();
                }
            foreach (var comp in this.TownComponents)
                comp.Update();
        }

        public void HandleGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Message.Types.EntitySpawned:
                    var entity = e.Parameters[0] as GameObject;
                    break;

                case Message.Types.EntityDespawned:
                    entity = e.Parameters[0] as GameObject;
                    if(this.Members.Contains(entity.RefId)) //TODO: dont dismiss despawned entities (they might be active outside the map)
                        RemoveAgent(entity);
                    break;

                default:
                    break;
            }
            foreach (var comp in this.TownComponents)
                comp.OnGameEvent(e);
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
        public IEnumerable<Vector3> GetUtilities(Utility.Types type)
        {
            return this.TownUtilitiesNew[type];
        }
        public bool HasUtility(Vector3 global, Utility.Types utility)
        {
            if (this.TownUtilitiesNew.TryGetValue(utility, out var list))
                return list.Contains(global);
            return false;
        }

        private void AddAgent(GameObject entity)
        {
            if (!entity.HasComponent<AIComponent>())
                throw new Exception();
            this.AddCitizen(entity.RefId);
            entity.Town = this;
            entity.Net.ConsoleBox.Write($"{entity.Name} has joined the town!");
            this.Map.EventOccured(Message.Types.NpcsUpdated);
        }

        private void RemoveAgent(GameObject entity)
        {
            if (entity.HasComponent<AIComponent>())
            {
                this.RemoveCitizen(entity.RefId);
                entity.Town = null;
                this.Net.ConsoleBox.Write($"{entity.Name} was dismissed from the town!");
                this.Map.EventOccured(Message.Types.NpcsUpdated);
            }
        }
        public void ToggleAgent(GameObject entity)
        {
            if (!this.Members.Contains(entity.RefId))
                this.AddAgent(entity);
            else
                this.RemoveAgent(entity);
        }
        public void AddCitizen(Actor actor)
        {
            this.AddCitizen(actor.RefId);
        }
        public void AddCitizen(int id)
        {
            this.Members.Add(id);
            foreach (var c in this.TownComponents)
                c.OnCitizenAdded(id);
        }
        public void RemoveCitizen(int id)
        {
            this.Members.Remove(id);
            foreach (var c in this.TownComponents)
                c.OnCitizenRemoved(id);
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
            foreach (var memberId in this.Members)
                this.Map.World.GetEntity(memberId).Town = this;
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
                agentsTag.Add(new SaveTag(SaveTag.Types.Int, "", a));
            tag.Add(agentsTag);
        }
        public void Load(SaveTag save)
        {
            Dictionary<string, SaveTag> compsTag = new Dictionary<string, SaveTag>();
            if (save.TryGetTagValue("Components", out compsTag))
                foreach (var tag in compsTag)
                {
                    var comp = this.TownComponents.FirstOrDefault(c => c.Name == tag.Key);
                    if (comp != null)
                        comp.Load(tag.Value);
                }
           
            LoadAgents(save);

            if (save.TryGetTagValue("Utilities", out List<SaveTag> utilitiesTag))
            {
                foreach (var tag in utilitiesTag)
                {
                    var utilityType = (Utility.Types)(int)tag["Type"].Value;
                    var positionList = new List<Vector3>().Load(tag["Positions"].Value as List<SaveTag>);
                    var hash = new HashSet<Vector3>(positionList);
                    this.TownUtilitiesNew[utilityType] = hash;
                }
            }

            foreach (var c in this.TownComponents)
                c.ResolveReferences();
        }

        private void LoadAgents(SaveTag save)
        {
            List<SaveTag> agentsTag;
            if (save.TryGetTagValue("Agents", out agentsTag))
                foreach (var bytes in agentsTag)
                {
                    var id = (int)bytes.Value;
                    this.AddCitizen(id);
                }
        }

        public void Write(BinaryWriter w)
        {
            foreach (var comp in this.TownComponents)
                comp.Write(w);

            w.Write(this.Members.Count);
            foreach (var a in this.Members)
                w.Write(a);

            foreach (var ut in Utility.All())
                w.Write(this.TownUtilitiesNew[ut].ToList());
        }
        public void Read(IDataReader r)
        {
            foreach (var comp in this.TownComponents)
                comp.Read(r);

            var acount = r.ReadInt32();
            for (int i = 0; i < acount; i++)
            {
                this.AddCitizen(r.ReadInt32());
            }

            foreach (var ut in Utility.All())
                this.TownUtilitiesNew[ut] = [.. r.ReadListVector3()];
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
        public IEnumerable<ISelectable> QuerySelectables(TargetArgs target)
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
            actions.Add(new Tuple<Func<string>, Action>(() => "Spawn objects", () => UI.Editor.ObjectTemplatesWindow.Instance.Show()));
            actions.Add(new Tuple<Func<string>, Action>(() => "Edit blocks", () => UI.Editor.TerrainWindow.Instance.Show()));

            this.QuickMenu = new UIQuickMenu();
            this.QuickMenu.AddItems(actions);
            this.QuickMenu.Location = UIManager.Mouse;
        }

        internal void OnHudCreated(Hud hud)
        {
            foreach (var c in this.TownComponents)
                c.OnHudCreated(hud);
        }

        public IEnumerable<Button> GetTabs(ISelectable selected)
        {
            foreach (var comp in this.TownComponents)
                foreach (var i in comp.GetTabs(selected))
                    yield return i;
        }
        internal void Select(ISelectable target, SelectionManager info)
        {
            foreach (var comp in this.TownComponents)
            {
                comp.UpdateQuickButtons();
                comp.OnTargetSelected(info, target);
            }
        }
        internal IEnumerable<KeyValuePair<IntVec3, BlockEntity>> GetRefuelablesNew()
        {
            var entities = this.Map.GetBlockEntitiesCache();
            var count = entities.Count;
            for (int i = 0; i < count; i++)
            {
                var kv = entities.ElementAt(i);
                if (kv.Value.HasComp<BlockEntityCompRefuelable>())
                    yield return kv;
            }
        }

        internal virtual void OnTargetSelected(IUISelection info, ISelectable selection)
        {
            if(selection is TargetArgs targetArgs)
                foreach (var c in this.TownComponents)
                    c.OnTargetSelected(info, targetArgs);
        }

        internal virtual void OnTargetSelected(SelectionManager info, ISelectable selection)
        {
            if (selection is TargetArgs targetArgs)
                foreach (var c in this.TownComponents)
                    c.OnTargetSelected(info, targetArgs);
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
    }
}

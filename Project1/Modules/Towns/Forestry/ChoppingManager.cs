using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.GameEvents;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public sealed class ChoppingManager : TownComponent
    {
        public enum Types { Chopping, Foraging }
        public static readonly Icon ChopIcon = new(ItemContent.AxeFull);
        public static readonly Icon ForageIcon = new(ItemContent.BerriesFull);

        public override string Name => "Forestry";
        readonly HashSet<int> QueuedForaging = [];

        public List<GameObject> GetTrees()
        {
            var list = this.ChoppingTasks.Select(id => this.Town.Map.Net.GetNetworkEntity(id)).ToList();
            return list;
        }
        public List<GameObject> GetPlants()
        {
            var list = this.QueuedForaging.Select(id => this.Town.Map.Net.GetNetworkEntity(id)).ToList();
            return list;
        }
        public HashSet<int> ChoppingTasks = [];

        public ChoppingManager(Town town)
        {
            this.Town = town;
        }

        internal void Designate(int type, List<int> ids, bool remove)
        {
            var designationType = (Types)type;
            HashSet<int> collection = null;
            switch (designationType)
            {
                case Types.Chopping:
                    collection = this.ChoppingTasks;
                    break;

                case Types.Foraging:
                    collection = this.QueuedForaging;
                    break;

                default:
                    break;
            }
            foreach (var p in ids)
            {
                if (!remove)
                    collection.Add(p);
                else
                    collection.Remove(p);
            }
            this.UpdateQuickButtons();
        }

        public void Designate(int type, Vector3 begin, Vector3 end, bool value)
        {
            var bbox = new BoundingBox(begin + Vector3.UnitZ, end + Vector3.UnitZ);
            var designationType = (Types)type;
            switch (designationType)
            {
                case Types.Chopping:
                    this.DesignateChopping(value, bbox);
                    break;

                case Types.Foraging:
                    this.DesignateForaging(value, bbox);
                    break;

                default:
                    break;
            }
            this.UpdateQuickButtons();

        }

        private void DesignateChopping(bool value, BoundingBox bbox)
        {
            var trees = from entity in this.Town.Map.GetObjects(bbox)
                        where entity.HasComponent<TreeComponent>()
                        select entity;
            foreach (var tree in trees)
            {
                if (!value)
                {
                    this.ChoppingTasks.Add(tree.RefID);
                }
                else
                {
                    this.ChoppingTasks.Remove(tree.RefID);
                }
            }
        }
        private void DesignateForaging(bool value, BoundingBox bbox)
        {
            var plants = from entity in this.Town.Map.GetObjects(bbox)
                         where entity.HasComponent<PlantComponent>()
                         select entity;
            foreach (var p in plants)
            {
                if (!value)
                    this.QueuedForaging.Add(p.RefID);
                else
                    this.QueuedForaging.Remove(p.RefID);
            }
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Message.Types.PlantHarvested:
                    var plant = e.Parameters[0] as GameObject;
                    this.QueuedForaging.Remove(plant.RefID);
                    this.UpdateQuickButtons();
                    break;

                case Message.Types.PlantReady:
                    this.UpdateQuickButtons();
                    break;

                case Message.Types.EntityDespawned:
                    GameObject entity;
                    EventEntityDespawned.Read(e.Parameters, out entity);
                    this.ChoppingTasks.Remove(entity.RefID);
                    this.QueuedForaging.Remove(entity.RefID);
                    break;

                default:
                    break;
            }
        }

        protected override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.ChoppingTasks.ToList().Save("ChoppingTasks"));
        }
        public override void Load(SaveTag tag)
        {
            tag.TryGetTagValue<List<SaveTag>>("ChoppingTasks", v => this.ChoppingTasks = [.. new List<int>().Load(v)]);
        }
        public override void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.ChoppingTasks.ToList());
        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.ChoppingTasks = [.. r.ReadListInt()];
        }

        public override void DrawUI(SpriteBatch sb, MapBase map, Camera cam)
        {
            this.DrawIcons(sb, map, cam, ChopIcon, this.GetTrees());
            this.DrawIcons(sb, map, cam, ForageIcon, this.GetPlants());
        }

        private void DrawIcons(SpriteBatch sb, MapBase map, Camera camera, Icon icon, IEnumerable<GameObject> objects)
        {
            foreach (var parent in objects)
                icon.DrawFloating(sb, camera, parent);
        }

        internal bool IsChoppingTask(GameObject tree)
        {
            return this.ChoppingTasks.Contains(tree.RefID);
        }
        internal bool IsForagingTask(GameObject obj)
        {
            return this.QueuedForaging.Contains(obj.RefID);
        }

        static readonly IconButton ButtonChopAdd = new(ChopIcon) { HoverText = "Chop down" };
        static readonly IconButton ButtonChopRemove = new(ChopIcon, Icon.Cross) { HoverText = "Cancel chop down" };

        static readonly QuickButton ButtonForageAdd = new(ForageIcon, null, "Forage");
        static readonly IconButton ButtonForageRemove = new(ForageIcon, Icon.Cross) { HoverText = "Cancel forage" };

        static void ChopDownAdd(List<TargetArgs> targets)
        {
            PacketEntityDesignation.Send(Client.Instance, (int)Types.Chopping, targets, false);
        }
        static void ChopDownRemove(List<TargetArgs> targets)
        {
            PacketEntityDesignation.Send(Client.Instance, (int)Types.Chopping, targets, true);
        }
        static void ForageAdd(List<TargetArgs> targets)
        {
            PacketEntityDesignation.Send(Client.Instance, (int)Types.Foraging, targets, false);
        }
        static void ForageRemove(List<TargetArgs> targets)
        {
            PacketEntityDesignation.Send(Client.Instance, (int)Types.Foraging, targets, true);
        }
        static void AddGeneric(List<TargetArgs> targets)
        {
            PacketEntityDesignation.Send(Client.Instance, (int)Types.Foraging, targets, true);
        }
        internal override void UpdateQuickButtons()
        {
            return;
            if (this.Town.Net is Server)
                return;
            var entities = SelectionManager.GetSelectedEntities();
            this.UpdateQuickButtonsChopping(entities);
            this.UpdateQuickButtonsForaging(entities);
        }
        private void UpdateQuickButtonsChopping(IEnumerable<GameObject> entities)
        {
            var areTask = entities.Where(e => this.ChoppingTasks.Contains(e.RefID));
            var areNotTask = entities.Except(areTask).OfType<Plant>().Where(IsChoppable);

            if (areTask.Any())
                SelectionManager.AddButton(ButtonChopRemove, ChopDownRemove, areTask);
            else
                SelectionManager.RemoveButton(ButtonChopRemove);

            if (areNotTask.Any())
                SelectionManager.AddButton(ButtonChopAdd, ChopDownAdd, areNotTask);
            else
                SelectionManager.RemoveButton(ButtonChopAdd);
        }
        private void UpdateQuickButtonsForaging(IEnumerable<GameObject> entities)
        {
            var areTask = entities.Where(e => this.QueuedForaging.Contains(e.RefID));
            var areNotTask = entities.Except(areTask).OfType<Plant>().Where(o => o.IsHarvestable);
            if (areTask.Any())
                SelectionManager.AddButton(ButtonForageRemove, ForageRemove, areTask);
            else
                SelectionManager.RemoveButton(ButtonForageRemove);

            if (areNotTask.Any())
                SelectionManager.AddButton(ButtonForageAdd, ForageAdd, areNotTask);
            else
                SelectionManager.RemoveButton(ButtonForageAdd);
        }

        private static bool IsChoppable(GameObject o)
        {
            return o.GetComponent<PlantComponent>()?.PlantProperties.ProductCutDown != null;
        }
    }
}

using System.Collections.Generic;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class NameplateManager : Control
    {
        static NameplateManager()
        {
            HotkeyManager.RegisterHotkey(Ingame.HotkeyContext, "Toggle nameplates", ToggleNameplates, System.Windows.Forms.Keys.N);
        }
        static NameplateManager Instance;
        public bool NameplatesEnabled { get { return this.Controls.Contains(this.Container); } }
        readonly Dictionary<INameplateable, Nameplate> Cache = new();
        readonly NameplatesContainer Container = new();
        readonly NameplatesContainer ContainerActors = new();
        public override Rectangle BoundsScreen => UIManager.Bounds;
        public override int Width { get => BoundsScreen.Width; }
        public override int Height { get => BoundsScreen.Height; }

        public NameplateManager(NetEndpoint net)
        {
            Instance = this;
            this.AddControls(this.ContainerActors);
            this.MouseThrough = true;
            net.Map.Events.ListenTo<EntityDespawnedEvent>(onEntityDespawned);
            net.Map.Events.ListenTo<EntitySpawnedEvent>(onEntitySpawned);
            foreach (var entity in net.Map.GetEntities())
                this.CreateNameplateFor(entity as Entity);
        }

        private void onEntityDespawned(EntityDespawnedEvent despawned)
        {
            this.DisposeNameplate(despawned.Entity);
        }
        private void onEntitySpawned(EntitySpawnedEvent spawned)
        {
            var entity = spawned.Entity;
            CreateNameplateFor(entity);
        }

        private void CreateNameplateFor(Entity entity)
        {
            var plate = Nameplate.Create(entity);
            var targetContainer = entity is Actor ? this.ContainerActors : this.Container;
            targetContainer.AddControls(plate);
            this.Cache.Add(entity, plate);
        }

        private void DisposeNameplate(GameObject entity)
        {
            if (!this.Cache.TryGetValue(entity, out var plate))
                return;
            this.Container.RemoveControls(plate);
            this.ContainerActors.RemoveControls(plate);
            this.Cache.Remove(entity);
        }
        
        public static void ToggleNameplates()
        {
            if (!Instance.Controls.Contains(Instance.ContainerActors))
                Instance.AddControls(Instance.ContainerActors);
            else if (!Instance.Controls.Contains(Instance.Container))
                Instance.AddControls(Instance.Container);
            else
                Instance.ClearControls();
        }
    }
}

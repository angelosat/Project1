using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

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
        HashSet<GameObject> PreviousScene = new();
        readonly Dictionary<INameplateable, Nameplate> Cache = new();
        readonly NameplatesContainer Container = new();
        readonly NameplatesContainer ContainerActors = new();
        public override Rectangle BoundsScreen => UIManager.Bounds;
        public override int Width { get => BoundsScreen.Width; }
        public override int Height { get => BoundsScreen.Height; }

        public NameplateManager()
        {
            Instance = this;
            this.AddControls(this.ContainerActors);
            this.MouseThrough = true;
        }
        public void Update(SceneState scene)
        {
            var added = scene.ObjectsDrawn.Except(this.PreviousScene);
            var removed = this.PreviousScene.Except(scene.ObjectsDrawn);
            Nameplate plate;

            foreach (var entity in added)
            {
                if (!this.Cache.TryGetValue(entity, out plate))
                {
                    plate = Nameplate.Create(entity);
                    this.Cache[entity] = plate;
                }
                if (entity is Actor)
                    this.ContainerActors.AddControls(plate);
                else
                    this.Container.AddControls(plate);
            }
            foreach (var entity in removed)
            {
                if (this.Cache.TryGetValue(entity, out plate))
                {
                    if (entity is Actor)
                        this.ContainerActors.RemoveControls(plate);
                    else
                        this.Container.RemoveControls(plate);
                }
            }
            this.PreviousScene = scene.ObjectsDrawn;
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch ((Components.Message.Types)e.Type)
            {
                case Components.Message.Types.EntityDespawned:
                    var entity = e.Parameters[0] as GameObject;
                    this.DisposeNameplate(entity);
                    break;

                default:
                    break;
            }
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

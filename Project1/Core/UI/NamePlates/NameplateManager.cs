using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Input;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Framework.UI;
using System.Collections.Generic;

namespace Project1.Core.UI.NamePlates;

class NameplateManager : Control
{
    static NameplateManager()
    {
        HotkeyManager.RegisterHotkey(Ingame.HotkeyContextInterface, "Toggle nameplates", ToggleNameplates, System.Windows.Forms.Keys.N);
    }
    static NameplateManager Instance;
    public bool NameplatesEnabled => this.Controls.Contains(this.Container);
    readonly Dictionary<INameplateable, Nameplate> Cache = [];
    readonly NameplatesContainer Container = new();
    readonly NameplatesContainer ContainerActors = new();
    public override Rectangle BoundsScreen => UIManager.Bounds;
    public override int Width { get => BoundsScreen.Width; }
    public override int Height { get => BoundsScreen.Height; }

    public NameplateManager(MapBase map)
    {
        Instance = this;
        this.AddControls(this.ContainerActors);
        this.MouseThrough = true;
        map.Events.ListenTo<EntityDespawnedEvent>(OnEntityDespawned);
        map.Events.ListenTo<EntitySpawnedEvent>(OnEntitySpawned);
        foreach (var entity in map.Entities)
            this.CreateNameplateFor(entity);
    }

    private void OnEntityDespawned(EntityDespawnedEvent despawned)
    {
        this.DisposeNameplate(despawned.Entity);
    }
    private void OnEntitySpawned(EntitySpawnedEvent spawned)
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

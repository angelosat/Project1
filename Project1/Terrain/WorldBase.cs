using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using Start_a_Town_.Net;

#nullable enable

namespace Start_a_Town_
{
    public abstract class WorldBase : Inspectable
    {
        internal float GroundAirThreshold;

        public string Name { get; set; }
        public override string Label => this.Name;
        public abstract MapBase GetMap(Vector2 mapCoords);
        public Random Random { get; set; }
        public virtual float Gravity { get; }
        public int Seed { get; set; }
        public int MaxHeight { get; set;  }
        public virtual ulong CurrentTick { get; set; }
        public virtual TimeSpan Clock { get; }
        public NetEndpoint Net { get; set; }

        public byte[] SeedArray { get; set; }

        public virtual Block DefaultBlock { get; set; }
        public virtual PopulationManager Population { get; }

        public virtual List<Terraformer> Terraformers { get; set; }

        public T GetTerraformer<T>() where T : Terraformer => this.Terraformers.First(t => t is T) as T;

        public abstract void WriteData(BinaryWriter w);

        public abstract MapCollection GetMaps();

        public abstract void Draw(SpriteBatch sb, Camera cam);
        public abstract void Tick(INetEndpoint net);
        public abstract void OnHudCreated(Hud hud);
        public abstract void OnTargetSelected(IUISelection info, ISelectable selection);
        public abstract void OnTargetSelected(SelectionManager info, ISelectable selection);

        public abstract void ResolveReferences();

        readonly EntityRegistry EntityRegistry = [];
        public IReadOnlyDictionary<int, Entity> Entities => this.EntityRegistry;
        public ReadOnlyObservableCollection<Entity> EntitiesObservable => this.EntityRegistry.Entities;
        public void Register(Entity entity)
        {
            entity.World = this;
            this.EntityRegistry.Add(entity);
        }
        public Entity GetEntity(int refId)
        {
            this.EntityRegistry.TryGetValue(refId, out var obj);
            return obj;
        }
        public T? GetEntity<T>(int refId) where T : Entity
        {
            this.EntityRegistry.TryGetValue(refId, out var obj);
            return obj as T;
        }
        public IEnumerable<Entity> GetEntities()
        {
            foreach (var o in this.EntityRegistry.Values)
                yield return o;
        }
        public IEnumerable<Entity> GetEntities(IEnumerable<int> netIds)
        {
            return this.EntityRegistry.GetEntities(netIds);
        }
        public bool TryGetEntity(int netID, out Entity obj)
        {
            if(  this.EntityRegistry.TryGetValue(netID, out var entity))
            {
                obj = entity;
                return true;
            }
            obj = null!;
            return false;
        }
        public bool TryGetEntity<T>(int netID, out T obj) where T : Entity
        {
            if (this.EntityRegistry.TryGetValue(netID, out var entity) && entity is T t)
            {
                obj = t;
                return true;
            }
            obj = null!;
            return false;
        }

        internal void RemoveEntity(int netId)
        {
            this.EntityRegistry.Remove(netId);
        }
        public bool DisposeEntity(int netId)
        {
            if (!this.EntityRegistry.TryGetValue(netId, out Entity? o))
                return false;
            foreach (var obj in o.GetSelfAndChildren())
            {
                $"{this} at {this.Net} disposing {obj.DebugName}".ToConsole();
                obj.OnDispose();
                this.EntityRegistry.Remove(obj.RefId);
                obj.Net = null; // this also makes gameobject.isdisposed return true
                //obj.RefId = 0; // dont set it to 0 because systems must be able to remove this entity's reference by id
                
                if (obj.IsSpawned)
                    obj.Despawn();
                //foreach (var child in from slot in o.GetChildren() where slot.HasValue select slot.Object)
                //    this.DisposeObject(child);
            }
            this.Events.Post(new EntityDisposedEvent(o));
            return true;
        }
        public EventBus Events { get; } = new();
        
    }
}

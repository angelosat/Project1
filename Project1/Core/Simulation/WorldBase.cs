using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Map;
using Project1.Core.Networking;
using Project1.Core.UI;
using Project1.Core.UI.Hud;
using Project1.Core.World;
using Project1.Core.World.WorldAreas;
using Project1.Core.WorldGen;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

#nullable enable

namespace Project1.Core.Simulation
{
    public interface IEntityProvider
    {
        Entity GetEntity(EntityRefId refId);
        T Get<T>(EntityRefId refId) where T : Entity;

    }
    public abstract class WorldBase : Inspectable, IEntityProvider
    {
        internal float GroundAirThreshold;

        public string Name { get; set; }
        public override string LabelReadable => this.Name;
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

        public abstract void WriteData(IDataWriter w);

        public abstract MapCollection GetMaps();

        public abstract void Draw(SpriteBatch sb, Camera cam);
        public abstract void Tick();
        public abstract void OnHudCreated(Hud hud);
        public abstract void OnTargetSelected(IUISelection info, ISelectable selection);
        public abstract void OnTargetSelected(SelectionManager info, ISelectable selection);

        public abstract void ResolveReferences();

        readonly EntityRegistry EntityRegistry;
        public IReadOnlyDictionary<int, Entity> Entities => this.EntityRegistry;
        public ReadOnlyObservableCollection<Entity> EntitiesObservable => this.EntityRegistry.Entities;
        protected WorldBase()
        {
            this.EntityRegistry = new(this);
        }
        internal void RegisterInt(Entity entity)
        {
            this.EntityRegistry.Add(entity);
        }
        public void Register(Entity entity, bool immediate = false)
        {
            //entity.World = this;
            //entity.Net = this.Net;
            //foreach (var e in entity.GetSelfAndChildren())
            //    this.EntityRegistry.Add(e);
            //entity.World = this;
            //entity.Net = this.Net;
            var toRegister = entity.GetSelfAndChildren();
            foreach (var e in toRegister)
            {
                this.EntityRegistry.Add(e);
                this.Events.Post(new EntityRegisteredEvent(e, immediate));
            }
            //this.Events.Post(new EntityRegisteredEvent(entity, immediate));
        }
        public Entity GetEntity(EntityRefId refId)
        {
            if (refId == EntityRefId.Null)
                return null!;
            if (!this.EntityRegistry.TryGetValue(refId, out var obj))
                return null!; // dont throw because return might be null for early snapshots
            return obj;
        }
        public T? Get<T>(EntityRefId refId) where T : Entity
        {
            this.EntityRegistry.TryGetValue(refId, out var obj);
            return obj as T;
        }
        public IEnumerable<Entity> GetEntities()
        {
            foreach (var o in this.EntityRegistry.Values)
                yield return o;
        }
        public IEnumerable<TEntity> GetEntities<TEntity>() where TEntity : Entity
        {
            return this.EntityRegistry.Values.OfType<TEntity>();
        }
        public IEnumerable<Entity> GetEntities(IEnumerable<int> netIds)
        {
            return this.EntityRegistry.GetEntities(netIds);
        }
        public IEnumerable<Entity> GetEntities(IEnumerable<EntityRefId> netIds)
        {
            return this.EntityRegistry.GetEntities(netIds);
        }
        public IEnumerable<T> GetEntities<T>(IEnumerable<EntityRefId> netIds) where T : Entity
        {
            return this.EntityRegistry.GetEntities(netIds).OfType<T>();
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
        public bool TryDisposeEntity(EntityRefId id)
        {
            var entity = this.GetEntity(id);
            if (entity is not null)
                return this.DisposeEntity(entity);
            return false;
        }
        public bool DisposeEntity(Entity entity) => this.DisposeEntity(entity.RefId);
        public bool DisposeEntity(EntityRefId netId)
        {
            if (!this.EntityRegistry.TryGetValue(netId, out Entity? o))
                throw new Exception();

            /// TODO: don't flatten, instead make it recursive. detach each child from it's parent before disposing
            foreach (var obj in o.GetSelfAndChildren().ToList()) /// HACK solidify the list so that children can detach during iteration
            {
                //$"{this.Net} disposing {obj.DebugName} on tick {this.Net.CurrentTick}".ToConsole();
                obj.OnDispose();
                this.EntityRegistry.Remove(obj.RefId);
                obj.Net = null;
                //obj.Container?.Remove(obj);
                //obj.Slot?.Assign(null, out var _);
                //obj.Map?.Despawn(obj);
                obj.Detach();
            }
            this.Events.Post(new EntityDisposedEvent(o));
            return true;
        }
        
        public abstract MapBase GetMap(int mapId);
        public EventBus Events { get; } = new();

        internal SaveTag Save()
        {
            var savetag = new SaveTag(SaveTag.Types.Compound, "World");
            savetag.Add(this.EntityRegistry.Save("Registry"));
            return savetag;
        }
        internal void Load(SaveTag savetag)
        {
            if(savetag.TryGetTag("Registry", out var tag)) this.EntityRegistry.Load(tag);
            
        }
        internal void Write(IDataWriter w)
        {
            this.EntityRegistry.Write(w);
        }
        internal void Read(IDataReader r)
        {
            this.EntityRegistry.Read(r);
        }

        public abstract FrontierDef PlaceAt(Entity entity, WorldSpacePosition pos);
        public abstract FrontierDef GetFrontierOf(Entity entity);

        
    }
}

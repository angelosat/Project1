using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        //public WorldSeed Seed { get; set; }
        public int Seed { get; set; }
        public int MaxHeight { get; set;  }
        public virtual ulong CurrentTick { get; set; }
        public virtual TimeSpan Clock { get; }
        public NetEndpoint Net { get; set; }

        public byte[] SeedArray { get; set; }

        public virtual Block DefaultBlock { get; set; }
        public virtual PopulationManager Population { get; }

        public virtual List<Terraformer> Terraformers { get; set; }
        public ItemSystem Items = new();

        public T GetTerraformer<T>() where T : Terraformer => this.Terraformers.First(t => t is T) as T;

        public abstract void WriteData(IDataWriter w);

        public abstract MapCollection GetMaps();

        public abstract void Draw(SpriteBatch sb, Camera cam);
        public abstract void Tick(INetEndpoint net);
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
        public void RegisterOld(Entity entity)
        {
            entity.World = this;
            this.EntityRegistry.Add(entity);
        }
        public void Register(GameObject entity, bool immediate = false)
        {
            entity.World = this;
            entity.Net = this.Net;
            foreach (var e in entity.GetSelfAndChildren())
                this.EntityRegistry.Add(e as Entity);
            this.Events.Post(new EntityRegisteredEvent(entity as Entity, immediate));
        }
        public void RegisterAndSync(GameObject entity)
        {
            this.Register(entity);
            return;
            if (this.Net.IsClient)
                throw new Exception();
            this.Register(entity);
            //PacketsEntities.Send(entity);
        }
        public Entity GetEntity(int refId)
        {
            if (refId == EntityRefId.Null)
                return null!;
            if (!this.EntityRegistry.TryGetValue(refId, out var obj))
                return null!; // dont throw because return might be null for early snapshots
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
        public IEnumerable<TEntity> GetEntities<TEntity>() where TEntity : Entity
        {
            return this.EntityRegistry.Values.OfType<TEntity>();
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
        public bool TryDisposeEntity(EntityRefId id)
        {
            var entity = this.GetEntity(id);
            if (entity is not null)
                return this.DisposeEntity(entity);
            return false;
        }
        public bool DisposeEntity(Entity entity) => this.DisposeEntity(entity.RefId);
        public bool DisposeEntity(int netId)
        {
            if (!this.EntityRegistry.TryGetValue(netId, out Entity? o))
                throw new Exception();

            /// TODO: don't flatten, instead make it recursive. detach each child from it's parent before disposing
            foreach (var obj in o.GetSelfAndChildren().ToList()) /// HACK solidify the list so that children can detach during iteration
            {
                //$"{this.Net} disposing {obj.DebugName} on tick {this.Net.CurrentTick}".ToConsole();
                obj.OnDispose();
                this.EntityRegistry.Remove(obj.RefId);
                obj.Net = null; // this also makes gameobject.isdisposed return true
                                //obj.RefId = 0; // dont set it to 0 because systems must be able to remove this entity's reference by id

                // remove from potential slot or container so that it gets detached from the parent entity and map.despawn() can remove if from the correct chunk by its true position,
                // otherwise its parent position will be read
                obj.Container?.Remove(obj);
                obj.Slot?.Assign(null, out var _);
                obj.Map?.Despawn(obj);

                //if (obj.IsSpawned || obj.Container is not null || obj.Slot is not null)
                //    throw new Exception("entity must not be spawned, in a container, or in a slot, when disposing");

                //obj.OnDespawn();
                //foreach (var child in from slot in o.GetChildren() where slot.HasValue select slot.Object)
                //    this.DisposeObject(child);
                //this.Events.Post(new EntityDisposedEvent(o));

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

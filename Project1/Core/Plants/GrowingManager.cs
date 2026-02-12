using System;
using System.Collections.Generic;
using System.Linq;
using Project1.Framework;
using Project1.Core.Entities;
using Project1.Core.Towns;
using Project1.Core.Towns.Zones;
using Project1.Core.Components.Plants;

namespace Project1.Core.Plants
{
    public class GrowingManager : TownComponent
    {
        readonly List<GrowingZone> GrowZones = [];
        public ILookup<PlantSpeciesDef, GrowingZone> BySpecies => this.GrowZones.ToLookup(z => z.Plant);
        public IReadOnlyList<GrowingZone> AllGrowingZones => this.GrowZones;
        public GrowingManager(Town town) : base(town)
        {
            var map = town.Map;
            map.Events.ListenTo<ZoneCreatedEvent>(OnZoneCreated);
            map.Events.ListenTo<ZoneDeletedEvent>(OnZoneDeleted);
            map.Events.ListenTo<PlantHarvestableEvent>(OnPlantHarvestable);
            map.Events.ListenTo<PlantHarvestedEvent>(OnPlantHarvested);
            map.Events.ListenTo<EntityEnteredZoneEvent>(OnEntityEnteredZone);
            map.Events.ListenTo<EntityExitedZoneEvent>(OnEntityExitedZone);
        }

        private void OnEntityExitedZone(EntityExitedZoneEvent e)
        {
            this.UnregisterHarvestable(e.Entity);
        }

        private void OnEntityEnteredZone(EntityEnteredZoneEvent e)
        {
            var entity = e.Entity;
            this.TryRegisterHarvestable(entity);
        }

        private bool TryRegisterHarvestable(Entity entity)
        {
            var entitycell = entity.Cell.Below;
            if (!entity.TryGetComponent<PlantComponent>(out var comp))
                return false;
            if (!comp.IsHarvestable)
                return false;
            this.RegisterHarvestable(entity);
            return true;
        }
        private void OnPlantHarvested(PlantHarvestedEvent e)
        {
            this.UnregisterHarvestable(e.Entity);
        }
        private void OnPlantHarvestable(PlantHarvestableEvent e)
        {
            this.RegisterHarvestable(e.Entity);
        }
        void RegisterHarvestable(Entity entity)
        {
            var entitycell = entity.Cell.Below;
            
            if (this.Map.Town.GetZoneAt(entitycell) is not GrowingZone zone)
                return;
            zone.PlantsHarvestable.Add(entity);
        }
        void UnregisterHarvestable(Entity entity)
        {
            var entitycell = entity.Cell.Below;
            if (this.Map.Town.GetZoneAt(entitycell) is not GrowingZone zone)
                return;
            zone.PlantsHarvestable.Remove(entity);
        }

        internal override void ResolveReferences()
        {
            var growzones = this.Town.ZoneManager.GetZones<GrowingZone>();
            this.GrowZones.AddRange(growzones);
            foreach (var gz in growzones)
                ValidateHarvestables(gz);
        }
        private void OnZoneCreated(ZoneCreatedEvent e)
        {
            if (e.Zone is not GrowingZone growzone)
                return;
            this.GrowZones.Add(growzone);
            ValidateHarvestables(growzone);
        }
        
        private void ValidateHarvestables(GrowingZone growzone)
        {
            foreach(var item in growzone.Items)
                TryRegisterHarvestable(item);
        }

        private void OnZoneDeleted(ZoneDeletedEvent e)
        {
            if (e.Zone is not GrowingZone growzone)
                return;
            this.GrowZones.Remove(growzone);
        }
        public IEnumerable<Entity> GetHarvestablePlants()
        {
            foreach(var gz in this.AllGrowingZones)
            {
                if (!gz.Harvesting)
                    continue;
                foreach (var plant in gz.PlantsHarvestable)
                    yield return plant;
            }
        }
        public IEnumerable<IntVec3> GetNextTillingPos()
        {
            return this.AllGrowingZones
                .SelectMany(z => z.GetTillingPositions());
        }
        public IEnumerable<IntVec3> GetSowingTargets(Entity preferredSeed)// = null)
        {
            if (preferredSeed.Profile is not PlantSpeciesDef seed)
                throw new ArgumentException($"{nameof(preferredSeed)} not a seed");
            return this.GetSowingTargets(seed);
        }
        public IEnumerable<SowingBatch> GetSowingTargetsAll(Entity preferredSeed)
        {
            if (preferredSeed.Profile is not PlantSpeciesDef seed)
                throw new ArgumentException($"{nameof(preferredSeed)} not a seed");
            return this.GetSowingTargetsAll(seed);
        }
        public IEnumerable<SowingBatch> GetSowingTargetsAll(PlantSpeciesDef species)
        {
            var validZones = this.AllGrowingZones.Where(z => z.Plant == species);
                                                                    
            foreach (var zone in validZones)
            {
                // return all positions and let the caller use the first or all
                var targets = zone.GetSowingPositions().ToList();
                if (targets.Count == 0)
                    continue;
                yield return new(targets, zone);
            }
        }
        public IEnumerable<IntVec3> GetSowingTargets(PlantSpeciesDef species)// = null)
        {
            var validZones = //preferredEntity is Entity seed ?
                this.AllGrowingZones.Where(z => z.Plant == species); //:
                                                                     //this.AllGrowingZones;
            foreach (var zone in validZones)
            {
                // return first sowing position of each zone
                var nextPos = zone.GetNextSowingPosition();
                if (nextPos.HasValue)
                    yield return nextPos.Value;
            }
        }

        internal bool IsValidTillingTarget(IntVec3 global)
        {
            var growzone = this.Town.ZoneManager.GetZoneAt<GrowingZone>(global);
            if (growzone is null)
                return false;
            return growzone.IsValidTilling(global);
        }

        public override string Name => "GrowingManager";
    }

    public record SowingBatch(IReadOnlyList<IntVec3> Positions, Zone Zone) { }
}
